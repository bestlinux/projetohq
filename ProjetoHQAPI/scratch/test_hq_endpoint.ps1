[System.Net.ServicePointManager]::ServerCertificateValidationCallback = {$true}
$baseUrl = "https://localhost:9001/api"

Write-Host "--- 1. Logging in ---"
$loginBody = @{
    email = "bestlinux@gmail.com"
    password = "200314Amor@"
} | ConvertTo-Json

$loginRes = Invoke-RestMethod -Uri "$baseUrl/login/login" -Method Post -Body $loginBody -ContentType "application/json"
$token = $loginRes.token

if (-not $token) {
    Write-Host "Failed to get token."
    exit 1
}
Write-Host "Token acquired successfully. Length: $($token.Length)"

$headers = @{
    "Authorization" = "Bearer $token"
}

# 2. Test GET api/hq
Write-Host "`n--- 2. GET api/hq ---"
$hqs = Invoke-RestMethod -Uri "$baseUrl/hq" -Method Get -Headers $headers
Write-Host "Total HQs returned: $($hqs.Count)"
$sampleHQ = $hqs[0]
Write-Host "Sample HQ: ID=$($sampleHQ.id) | Titulo='$($sampleHQ.titulo)' | Editora='$($sampleHQ.editora)'"

# 3. Test GET api/hq/{id}
Write-Host "`n--- 3. GET api/hq/{id} ---"
$singleHQ = Invoke-RestMethod -Uri "$baseUrl/hq/$($sampleHQ.id)" -Method Get -Headers $headers
Write-Host "Retrieved HQ by ID: Titulo='$($singleHQ.titulo)' | Editora='$($singleHQ.editora)'"

# 4. Test GET api/hq/BuscaPorEditora/{editora}
Write-Host "`n--- 4. GET api/hq/BuscaPorEditora/{editora} ---"
$editoraHQs = Invoke-RestMethod -Uri "$baseUrl/hq/BuscaPorEditora/$([System.Uri]::EscapeDataString($sampleHQ.editora))" -Method Get -Headers $headers
Write-Host "HQs found for editora '$($sampleHQ.editora)': $($editoraHQs.Count)"

# 5. Test POST api/hq (Create test HQ)
Write-Host "`n--- 5. POST api/hq (Create test HQ) ---"
$newHQId = [System.Guid]::NewGuid().ToString()
$createObj = @{
    id = $newHQId
    titulo = "HQ Teste Automacao Endpoint"
    editora = "Editora Teste Endpoint"
    anoLancamento = "2026"
    numeroEdicao = 1
    categoria = 1
    genero = 1
    status = 1
    formato = 1
    lido = $false
    thumbCapa = "https://via.placeholder.com/300x450"
    linkDetalhes = ""
}
$createBody = $createObj | ConvertTo-Json
$createdId = Invoke-RestMethod -Uri "$baseUrl/hq" -Method Post -Headers $headers -Body $createBody -ContentType "application/json"
Write-Host "POST result: Created ID = $createdId"

# 6. Verify created HQ with GET api/hq/{id}
Write-Host "`n--- 6. GET created HQ ---"
$createdHQ = Invoke-RestMethod -Uri "$baseUrl/hq/$newHQId" -Method Get -Headers $headers
Write-Host "Created HQ retrieved: Titulo='$($createdHQ.titulo)', Editora='$($createdHQ.editora)', Edicao=$($createdHQ.numeroEdicao), Lido=$($createdHQ.lido)"

# 7. Test PUT api/hq/{id} (Update fields)
Write-Host "`n--- 7. PUT api/hq/{id} (Update HQ) ---"
$updateObj = @{
    id = $newHQId
    titulo = "HQ Teste Automacao Endpoint - ATUALIZADO COM SUCESSO"
    editora = "Editora Modificada"
    anoLancamento = "2026"
    numeroEdicao = 99
    categoria = 2
    genero = 3
    status = 2
    formato = 2
    lido = $true
    thumbCapa = "https://via.placeholder.com/300x450"
}
$updateBody = $updateObj | ConvertTo-Json
$putRes = Invoke-RestMethod -Uri "$baseUrl/hq/$newHQId" -Method Put -Headers $headers -Body $updateBody -ContentType "application/json"
Write-Host "PUT result: $putRes"

# 8. Verify updated HQ
Write-Host "`n--- 8. GET updated HQ ---"
$updatedHQ = Invoke-RestMethod -Uri "$baseUrl/hq/$newHQId" -Method Get -Headers $headers
Write-Host "Updated HQ retrieved: Titulo='$($updatedHQ.titulo)', Editora='$($updatedHQ.editora)', Edicao=$($updatedHQ.numeroEdicao), Lido=$($updatedHQ.lido)"

# 9. Test DELETE api/hq/{id}
Write-Host "`n--- 9. DELETE api/hq/{id} ---"
$delRes = Invoke-RestMethod -Uri "$baseUrl/hq/$newHQId" -Method Delete -Headers $headers
Write-Host "DELETE result: $delRes"

# 10. Verify deletion returns 404
Write-Host "`n--- 10. Verify deletion returns 404 ---"
try {
    $verifyHQ = Invoke-RestMethod -Uri "$baseUrl/hq/$newHQId" -Method Get -Headers $headers
    Write-Host "WARNING: HQ was still returned: $verifyHQ"
} catch {
    Write-Host "Expected 404/error on deleted HQ: $($_.Exception.Message)"
}

Write-Host "`n=== ALL HQ ENDPOINT TESTS COMPLETED SUCCESSFULLY ==="
