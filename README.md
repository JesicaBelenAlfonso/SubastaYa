# SubastaYa

Plataforma de subastas en línea (Clean Architecture: `Domain`, `Application`, `Infrastructure`, `SubastaYa` API + `Frontend`).

## Requisitos

- .NET SDK 8
- SQL Server Express LocalDB (`(localdb)\MSSQLLocalDB`) — viene con Visual Studio
- Cualquier navegador para el frontend (Live Server) o Postman/curl para la API

## Puesta en marcha

1. Abrir la solución:

   ```bash
   dotnet restore
   ```

2. Aplicar migraciones y ejecutar:

   ```bash
   dotnet run --project SubastaYa
   ```

   Al arrancar, el `Program.cs`:
   - **Aplica automáticamente las migraciones pendientes**.
   - **Siembra datos de prueba** (solo si la base está vacía; es idempotente).
   - Levanta el **worker de liquidación** (cada 30 s: activa subastas, finaliza vencidas).

3. La API queda en `http://localhost:5253`. Swagger: `http://localhost:5253/swagger`.

4. Frontend: abrir `Frontend/index.html` con Live Server (CORS ya está habilitado para cualquier origen en desarrollo).

## Base de datos

Cadena por defecto en `appsettings.json`:

```
Server=(localdb)\MSSQLLocalDB;Database=SubastaYaDb;Trusted_Connection=True
```

Si necesitás regenerarla desde cero:

```powershell
# borra la base (los datos de prueba se recargan solos al arrancar)
sqlcmd -S "(localdb)\MSSQLLocalDB" -Q "ALTER DATABASE [SubastaYaDb] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [SubastaYaDb];"
```

## Datos de prueba (seed)

Al primer arranque se crean:

| Usuario | Email | Password | Wallet |
|---|---|---|---|
| Vendedor | `vendedor@subastaya.com` | `Password123` | 0 (recibe cobros) |
| Comprador Uno | `comprador1@subastaya.com` | `Password123` | 200.000 disponible |
| Comprador Dos | `comprador2@subastaya.com` | `Password123` | 140.000 total / 45.000 retenidos (líder) |
| Sin Fondos | `sinfondos@subastaya.com` | `Password123` | 500 (no puede pujar) |

**4 categorías**: Electrónica, Vehículos, Coleccionables, Hogar.

**5 subastas**:

| Id | Título | Estado | Detalle |
|---|---|---|---|
| 1 | Notebook Gamer RTX 16GB | `ACTIVA` | 2 pujas, líder 45.000 (Comprador Dos) |
| 2 | Bicicleta Mountain Bike 29 | `ACTIVA` | Termina en < 2 min (para probar anti-sniping) |
| 3 | Figura de colección edición limitada | `PROXIMA` | Comienza en +24 h |
| 4 | Juego de living de roble | `FINALIZADA` | Vencida con ganador: la liquida el worker al arrancar (60.000) |
| 5 | Monitor 27'' 144Hz | `DESIERTA` | Vencida sin pujas |

Los movimientos del **ledger** (DEPOSITO/RETENCION/LIBERACION/PAGO/COBRO) son coherentes con los saldos de las wallets.

## Estados de una subasta

`PROXIMA` → `ACTIVA` → `FINALIZADA` (con pujas) / `DESIERTA` (sin pujas).

- **Activación automática**: al pasar `StartDate` la subasta pasa de `PROXIMA` a `ACTIVA` (el worker y la lectura la reflejan al instante).
- **Solo se puja sobre `ACTIVA`**.
- `CreateAuction` crea la subasta directamente con `ACTIVA` (si el inicio ya pasó) o `PROXIMA` — nunca queda "Pending" huérfana.

## Flujo escrow de una puja (`POST /api/v1/bids`)

1. Valida que exista la subasta (404) y que esté `ACTIVA`.
2. Rechaza si el tiempo se agotó (409) o si el monto es menor a oferta + incremento (400).
3. **Libera** la retención del líder anterior (ledger `LIBERACION`).
4. **Retiene** el monto nuevo del ganador (ledger `RETENCION`).
5. Si el pujador **se supera a sí mismo**, el saldo congelado queda neto por la diferencia (delta).
6. Anti-sniping (ver abajo).
7. Un único `SaveChanges`: puja + wallets + transacciones son atómicos.

## Anti-sniping

Configuración en `appsettings.json` (`Auction`):

```json
"Auction": {
  "AntiSnipingWindowSeconds": 60,
  "AntiSnipingExtensionMinutes": 2
}
```

- Puja con **≤ 60 s** restantes → extiende el fin **+2 min**.
- Puja con **> 60 s** → no extiende.
- Puja con **0 s** restantes → 409 (la subasta ya venció).
- El `userId` del pujador queda registrado en auditoría (`AUCTION_TIME_EXTENDED`).

## Worker de liquidación

`AuctionStatusWorker` ejecuta `AuctionFinalizationService` cada 30 s:

- **Vencida con ganador** (puja máxima): el monto retenido pasa del comprador al vendedor,
  ledger `PAGO` (comprador) / `COBRO` (vendedor), estado `FINALIZADA`.
- **Vencida sin pujas**: estado `DESIERTA`.
- **Idempotente**: solo procesa subastas `ACTIVA` ya vencidas; repetir el worker no duplica movimientos
  (verificado: al reiniciar varias veces el vendedor cobra una sola vez por subasta).

## Auditoría (AuditLog)

Toda operación sensible queda registrada en la tabla `Audits` con `Entity`, `EntityId`,
`Action`, `UserId`, `DetalleJson` y `Date`. Se consulta con `GET /api/v1/audits?entity=&action=`.

| Acción | Qué registra | Detalle |
|---|---|---|
| `BID_REJECTED` | Puja rechazada | `auctionId`, `amount`, `minimum`, `auctionStatus`, `reason` |
| `AUCTION_TIME_EXTENDED` | Anti-sniping | `previousEnd`, `newEnd`, `extendedByMinutes` + **userId del pujador** |
| `AUCTION_STATUS_CHANGED` | Worker | `from`, `to`, `winnerId`, `winningAmount`, `reason` |
| `WALLET_MANUAL_CREDIT` | Depósito | `walletId`, `amount`, `newTotalBalance` |
| `WALLET_WITHDRAWAL` | Retiro | `walletId`, `amount`, `newTotalBalance` |
| `CREATE` / `UPDATE` / `DELETE` | Altas, cambios y bajas | Datos relevantes del recurso |

**Rechazos auditados** (`BID_REJECTED`): subasta inexistente, subasta no activa/vencida,
monto por debajo del mínimo, **saldo insuficiente** y **conflicto de concurrencia**.

**Auditoría en concurrencia.** Cuando dos pujas compiten por la misma subasta, la que pierde
dispara `DbUpdateConcurrencyException`; el handler hace `DetachAll()` para descartar las
entidades mutadas del request y persiste el `BID_REJECTED` **con contexto limpio**, sin
reintentar el insert/update (la auditoría es best-effort y nunca enmascara el 409).

```bash
# pujas rechazadas
curl "http://localhost:5253/api/v1/audits?entity=Bid&action=BID_REJECTED"
# extensiones por anti-sniping
curl "http://localhost:5253/api/v1/audits?action=AUCTION_TIME_EXTENDED"
```

## Endpoints principales

```
POST   /api/v1/users                      Registro (crea wallet)
POST   /api/v1/auth/sessions              Login
GET    /api/v1/users/{userId}/wallets                 Wallet
POST   /api/v1/users/{userId}/wallets/transactions    DEPOSITO / RETIRO (solo manuales)
GET    /api/v1/users/{userId}/wallets/transactions    Historial (ledger)
GET    /api/v1/users/{userId}/activities              "Mis actividades" (VENDEDOR/PUJADOR)
GET    /api/v1/categories
GET    /api/v1/auctions                   Lista (con oferta actual y cantidad de pujas)
GET    /api/v1/auctions/{id}              Detalle
GET    /api/v1/auctions/{id}/bids         Historial de pujas anonimizado (sala en vivo)
POST   /api/v1/auctions                   Crear subasta (ACTIVA o PROXIMA según fechas)
POST   /api/v1/bids                       Pujar (expectativas: 201 con Location a la subasta)
GET    /api/v1/audits?entity=&action=     Auditoría
```

### Retenciones y liberaciones: solo del sistema

`POST /api/v1/users/{userId}/wallets/transactions` acepta **únicamente** `DEPOSITO` y `RETIRO`.
Los tipos `RETENCION`/`LIBERACION` son rechazados con `409` aunque el cliente mande un `auctionId`
real: ese ledger solo lo escriben `CreateBidCommandHandler` y `AuctionFinalizationService`
(que lo persisten a través del repositorio interno, nunca por esta API abierta).

### Sala de pujas en vivo

`Frontend/subasta.html?id={id}` implementa la sala en vivo con **short-polling** (cada 3 s refresca
detalle + historial):

- **Countdown por subasta** (tick por segundo) con cierre real de `endDate`.
- **Historial anonimizado**: cada pujador aparece como "Pujador N" (orden de primera aparición),
  sin exponer id ni nombre. Con `?userId=` se marcan las pujas propias con `(vos)`.
- **Sugerencia**: el monto mínimo siguiente = oferta actual + incremento mínimo, precargado en el input.
- **Liderando / Superado**: badge sobre la puja del usuario según quién lleve la oferta vigente.
- **Aviso anti-sniping**: cuando quedan ≤ 60 s se muestra el cartel de extensión automática (+2 min).

Las páginas `crear-subasta.html` (publicación) y `mis-actividades.html` (subastas publicadas y
participadas, con indicador de liderazgo) completan el flujo desde el frontend.

### Ejemplo: pujar

```bash
curl -X POST http://localhost:5253/api/v1/bids \
  -H "Content-Type: application/json" \
  -d '{"auctionId":1,"amount":50000,"buyerId":2}'
```

- Éxito → `201 Created` con `Location: http://localhost:5253/api/v1/auctions/1`
- Subasta inexistente → `404`
- Puja muy baja → `400`
- Saldo insuficiente / subasta no activa / vencida / conflicto de concurrencia → `409`

## Demo de concurrencia

Dos pujas simultáneas sobre la misma subasta: **una debe confirmarse (201) y la otra rechazarse (409)**.
Se logra con el versionado optimista (`RowVersion` en `Auctions` y `Wallets`).

Con la API corriendo:

```powershell
# crear una subasta activa nueva (fin en 1 hora)
$now = [DateTime]::UtcNow
$body = @{
  categoryId=1; title="Subasta concurrencia"; descripcion="demo";
  urlImagen="https://picsum.photos/seed/conc/600/400"; basePrice=10000;
  minIncrement=1000; startDate=$now.AddHours(-1).ToString("o");
  endDate=$now.AddHours(1).ToString("o"); sellerId=1
} | ConvertTo-Json
$a = Invoke-RestMethod -Uri http://localhost:5253/api/v1/auctions -Method Post `
      -ContentType "application/json" -Body $body

# disparar 2 pujas simultáneas (buyerId 2 y 3)
$p1 = @{ auctionId=$a.id; amount=12000; buyerId=2 } | ConvertTo-Json
$p2 = @{ auctionId=$a.id; amount=12000; buyerId=3 } | ConvertTo-Json
$j1 = Start-Job { param($u,$p) try { Invoke-RestMethod -Uri $u -Method Post -ContentType "application/json" -Body $p } catch { $_.Exception.Response.StatusCode.value__ } } -ArgumentList "http://localhost:5253/api/v1/bids", $p1
$j2 = Start-Job { param($u,$p) try { Invoke-RestMethod -Uri $u -Method Post -ContentType "application/json" -Body $p } catch { $_.Exception.Response.StatusCode.value__ } } -ArgumentList "http://localhost:5253/api/v1/bids", $p2
Receive-Job $j1, $j2 -Wait; Remove-Job $j1, $j2 -Force
```

O directamente con el script incluido para la defensa:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File docs/demo-concurrencia.ps1
```

Resultado esperado:

```
buyer1=201 buyer2=409 | pujas en la subasta = 1  ->  1 confirmada + 1 rechazada: OK
```

## Prueba rápida de anti-sniping (59 s / 61 s / 0 s)

```powershell
# 59 s restantes -> DEBE extender +2 min
$now=[DateTime]::UtcNow
$body=@{categoryId=1;title="Snip 59s";descripcion="x";urlImagen="x";basePrice=10000;
  minIncrement=1000;startDate=$now.ToString("o");endDate=$now.AddSeconds(59).ToString("o");sellerId=1}|ConvertTo-Json
$a=Invoke-RestMethod -Uri http://localhost:5253/api/v1/auctions -Method Post -ContentType "application/json" -Body $body
$b=Invoke-RestMethod -Uri http://localhost:5253/api/v1/bids -Method Post -ContentType "application/json" `
     -Body (@{auctionId=$a.id;amount=12000;buyerId=3}|ConvertTo-Json)
$nuevo=(Invoke-RestMethod -Uri "http://localhost:5253/api/v1/auctions/$($a.id)").endDate
# $nuevo debe ser endDate original + 2 minutos
```

- 61 s → `endDate` NO cambia.
- 0 s (endDate en el pasado) → la puja responde `409`.

## Organización

- `Domain/` — entidades puras y reglas de estado (`AuctionStatus`).
- `Application/` — casos de uso (handlers), servicios (`AuctionFinalizationService`, `AuditService`), contratos de repositorios.
- `Infrastructure/` — EF Core (`AppDbContext`), repositorios, migraciones y `DbSeeder`.
- `SubastaYa/` — API REST, worker, middleware de excepciones (400/404/409/401/500).
- `Frontend/` — catálogo, login/registro, billetera, sala de pujas en vivo (short-polling),
  creación de subastas y "Mis actividades" (ES6 puro + Bootstrap). Muestra los
  estados `ACTIVA`, `PROXIMA`, `FINALIZADA` y `DESIERTA`; si la API no responde y la variable
  `USE_MOCK_FALLBACK` está activada, el catálogo usa un respaldo de ejemplo con un aviso visible.

## Swagger

Disponible en desarrollo: **`http://localhost:5253/swagger`**. El esquema OpenAPI se genera
con los **comentarios XML** de los controllers y con ejemplos de request/response tomados de
los DTOs (`<example>`), además de los códigos de respuesta declarados con `[ProducesResponseType]`.

Para que los XML comments se incluyan, `SubastaYa.csproj` tiene `GenerateDocumentationFile` en
`true` y `Program.cs` apunta a `SubastaYa.xml` en el directorio de salida.

## Migraciones

Las migraciones viven en `Infrastructure/Migrations` y se aplican **automáticamente al arrancar**
(`db.Database.MigrateAsync()` en `Program.cs`). Para trabajar manualmente:

```bash
# 1) instalar la herramienta EF (una vez)
dotnet tool install --global dotnet-ef

# 2) listar migraciones y su estado (las aplicadas vs. las pendientes)
dotnet ef migrations list --project Infrastructure --startup-project SubastaYa

# 3) crear una migración nueva (con la API detenida)
dotnet ef migrations add NombreDescriptivo --project Infrastructure --startup-project SubastaYa

# 4) aplicar solo las pendientes sin levantar la API
dotnet ef database update --project Infrastructure --startup-project SubastaYa

# 5) revertir a una migración anterior (opcional)
dotnet ef database update NombreMigracionAnterior --project Infrastructure --startup-project SubastaYa
```

Para recargar el seed desde cero, borrar la base (sección *Base de datos*): al arrancar,
`MigrateAsync` la recrea y `DbSeeder` vuelve a sembrar los datos.