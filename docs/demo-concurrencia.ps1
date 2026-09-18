# Demo de concurrencia: 2 pujas simultaneas e identicas sobre la misma subasta.
# Resultado esperado: UNA confirmada (201) y UNA rechazada (409 por conflicto de concurrencia).
#
# Requiere la API corriendo en http://localhost:5253 (dotnet run --project SubastaYa)
#
# Por que usa dos HttpClient y NO Start-Job:
# Start-Job levanta un proceso PowerShell por cada puja (arranque de cientos de ms),
# por lo que las peticiones se serializan: la primera commitea y la segunda ya lee la
# oferta nueva, recibiendo 400 por incremento minimo en vez de 409. Para provocar el
# choque real contra la RowVersion, ambas peticiones deben LEER el mismo estado previo
# antes de que la otra commitee. Eso se logra disparando los PostAsync en el mismo
# proceso (dos conexiones HTTP independientes) antes de esperar los resultados.

$ErrorActionPreference = "Stop"
Add-Type -AssemblyName System.Net.Http
try { [Console]::OutputEncoding = [System.Text.Encoding]::UTF8 } catch { }

$base = "http://localhost:5253/api/v1"
$maxIntentos = 5
$monto = 12000
$compradorUnoId = 2
$compradorDosId = 3

function New-Auction {
    $now = [DateTime]::UtcNow
    $body = @{
        categoryId   = 1
        title        = "Subasta concurrencia"
        descripcion  = "Demo: dos pujas simultaneas sobre la misma subasta."
        urlImagen    = "https://picsum.photos/seed/concurrencia/600/400"
        basePrice    = 10000
        minIncrement = 1000
        startDate    = $now.AddHours(-1).ToString("o")
        endDate      = $now.AddHours(1).ToString("o")
        sellerId     = 1
    } | ConvertTo-Json

    Invoke-RestMethod -Uri "$base/auctions" -Method Post -ContentType "application/json" -Body $body
}

function New-ClienteHttp {
    $handler = New-Object System.Net.Http.HttpClientHandler
    $cliente = New-Object System.Net.Http.HttpClient($handler)
    $cliente.Timeout = [TimeSpan]::FromSeconds(30)
    # Sin negociacion 100-continue: menos latencia y mejor solapamiento.
    $cliente.DefaultRequestHeaders.ExpectContinue = $false
    return $cliente
}

function New-ContenidoJson($texto) {
    return New-Object System.Net.Http.StringContent($texto, [System.Text.Encoding]::UTF8, "application/json")
}

$exito = $false

for ($intento = 1; $intento -le $maxIntentos -and -not $exito; $intento++) {
    $auction = New-Auction
    $id = $auction.id

    # Dos clientes con conexiones HTTP independientes para que las pujas salgan en paralelo.
    $cliente1 = New-ClienteHttp
    $cliente2 = New-ClienteHttp

    $json1 = @{ auctionId = $id; amount = $monto; buyerId = $compradorUnoId } | ConvertTo-Json
    $json2 = @{ auctionId = $id; amount = $monto; buyerId = $compradorDosId } | ConvertTo-Json

    $tarea1 = $cliente1.PostAsync("$base/bids", (New-ContenidoJson $json1))
    $tarea2 = $cliente2.PostAsync("$base/bids", (New-ContenidoJson $json2))

    try {
        [System.Threading.Tasks.Task]::WaitAll(@($tarea1, $tarea2))
    }
    catch {
        Write-Output ("Intento {0}: error de red -> {1}" -f $intento, $_.Exception.Message)
        $cliente1.Dispose(); $cliente2.Dispose()
        continue
    }

    # Codigos HTTP reales (no se infieren del exito de Invoke-RestMethod).
    $status1 = [int]$tarea1.Result.StatusCode
    $status2 = [int]$tarea2.Result.StatusCode
    $cuerpo1 = $tarea1.Result.Content.ReadAsStringAsync().Result
    $cuerpo2 = $tarea2.Result.Content.ReadAsStringAsync().Result

    $cliente1.Dispose(); $cliente2.Dispose()

    $pujas = (Invoke-RestMethod -Uri "$base/auctions/$id").cantidadPujas

    Write-Output ("Intento {0} (subasta id={1}): Comprador Uno({2}) => {3} | Comprador Dos({4}) => {5} | pujas={6}" -f `
        $intento, $id, $compradorUnoId, $status1, $compradorDosId, $status2, $pujas)

    $hayGanadora = ($status1 -eq 201 -and $status2 -eq 409)
    $hayPerdedora = ($status1 -eq 409 -and $status2 -eq 201)

    if ($hayGanadora -or $hayPerdedora) {
        # El 409 debe ser por concurrencia, no por incremento minimo (400 disfrazado).
        $perdedorId = if ($status1 -eq 409) { $compradorUnoId } else { $compradorDosId }
        $cuerpoPerdedor = if ($status1 -eq 409) { $cuerpo1 } else { $cuerpo2 }

        $rechazos = Invoke-RestMethod -Uri "$base/audits?entity=Bid&action=BID_REJECTED"
        $auditoria = $rechazos | Where-Object {
            $_.userId -eq $perdedorId -and $_.detalleJson -match ('"auctionId":' + $id + '(?!\d)')
        } | Select-Object -First 1

        $esConcurrencia = $auditoria -and ($auditoria.detalleJson -match "Conflicto de concurrencia")

        Write-Output ("Body del 409: {0}" -f $cuerpoPerdedor.Trim())
        if ($auditoria) {
            Write-Output ("Auditoria del rechazo: action={0} | detail={1}" -f $auditoria.action, $auditoria.detalleJson)
        }
        else {
            Write-Output "Auditoria del rechazo: NO se encontro el BID_REJECTED del perdedor en el audit log."
        }

        if ($esConcurrencia -and $pujas -eq 1) {
            $exito = $true
            Write-Output "Resultado: OK - 1 confirmada (201) y 1 rechazada (409 por conflicto de concurrencia)"
        }
        else {
            Write-Output "Intento descartado: el 409 no quedo probado como conflicto de concurrencia en el audit log."
        }
    }
    else {
        Write-Output "Intento descartado: las peticiones no se solaparon (no hubo 409 real). Se reintenta con otra subasta."
    }
}

if (-not $exito) {
    Write-Output ("Resultado: REVISAR - no se pudo reproducir una carrera real en {0} intentos." -f $maxIntentos)
    exit 1
}
