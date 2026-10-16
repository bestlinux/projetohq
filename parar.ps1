Write-Host "Encerrando servicos do ProjetoHQ..." -ForegroundColor Yellow
Stop-Process -Name "ProjetoHQApi.WebApi" -Force -ErrorAction SilentlyContinue
Get-Process | Where-Object { $_.MainWindowTitle -like "ProjetoHQ*" } | Stop-Process -Force -ErrorAction SilentlyContinue
Write-Host "Servicos finalizados com sucesso!" -ForegroundColor Green