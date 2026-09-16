# Demo de concurrencia: 2 pujas simultáneas sobre la misma subasta.
# Resultado esperado: 1 confirmada (201) y 1 rechazada (409).
#
# Requiere la API corriendo en http://localhost:5253 (dotnet run --project SubastaYa)

$ErrorActionPreference = "Stop"
$base = "http://localhost:5253/api/v1"

# 1) Crear una subasta activa nueva (empieza hace 1h, termina en 1h)
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

$a = Invoke-RestMethod -Uri "$base/auctions" -Method Post -ContentType "application/json" -Body $body
Write-Output "Subasta creada: id=$($a.id) status=$($a.status)"

# 2) Dos pujas simultáneas del MISMO monto (Comprador Uno = 2, Comprador Dos = 3).
#    Ambas son válidas contra el estado previo; la que pierde la carrera de guardado
#    choca contra la RowVersion de la subasta y recibe 409 (conflicto de concurrencia).
$j1 = Start-Job -ScriptBlock { param($u, $p) Invoke-RestMethod -Uri $u -Method Post -ContentType "application/json" -Body $p -TimeoutSec 30 } -ArgumentList "$base/bids", (@{ auctionId = $a.id; amount = 12000; buyerId = 2 } | ConvertTo-Json)
$j2 = Start-Job -ScriptBlock { param($u, $p) Invoke-RestMethod -Uri $u -Method Post -ContentType "application/json" -Body $p -TimeoutSec 30 } -ArgumentList "$base/bids", (@{ auctionId = $a.id; amount = 12000; buyerId = 3 } | ConvertTo-Json)

$res1 = $null; $res2 = $null
try { $res1 = Receive-Job $j1 -Wait -ErrorAction Stop } catch { }
try { $res2 = Receive-Job $j2 -Wait -ErrorAction Stop } catch { }
Remove-Job $j1, $j2 -Force

$s1 = if ($res1) { "201" } else { "409" }
$s2 = if ($res2) { "201" } else { "409" }

# 3) Verificar que quedó exactamente UNA puja
$final = Invoke-RestMethod -Uri "$base/auctions/$($a.id)"
Write-Output ("Comprador Uno => {0} | Comprador Dos => {1} | pujas en la subasta = {2}" -f $s1, $s2, $final.cantidadPujas)

$ok = (($res1 -ne $null) -xor ($res2 -ne $null)) -and $final.cantidadPujas -eq 1
Write-Output ("Resultado: " + $(if ($ok) { "OK - 1 confirmada y 1 rechazada (409 por conflicto de concurrencia)" } else { "REVISAR - se esperaba una confirmada y una rechazada" }))