Write-Host "============================================================" -ForegroundColor Cyan
Write-Host "          INICIANDO PROJETOHQ (FULL STACK)                  " -ForegroundColor Cyan
Write-Host "============================================================" -ForegroundColor Cyan

$root = $PSScriptRoot
if (-not $root) { $root = (Get-Location).Path }

Write-Host "[1/2] Iniciando Backend .NET 10 API (https://localhost:9001)..." -ForegroundColor Yellow
$argApi = "/k title ProjetoHQ - Backend API & cd /d `"$root\ProjetoHQAPI\ProjetoHQAPI.WebApi`" && dotnet run"
Start-Process cmd.exe -ArgumentList $argApi

Write-Host "[2/2] Iniciando Frontend React + Vite (http://localhost:5173)..." -ForegroundColor Yellow
$argWeb = "/k title ProjetoHQ - Frontend Web & cd /d `"$root\ProjetoHQWeb`" && npm run dev"
Start-Process cmd.exe -ArgumentList $argWeb

Write-Host "`nAguardando 5 segundos para subir os servicos..." -ForegroundColor Gray
Start-Sleep -Seconds 5

Start-Process "http://localhost:5173"
Write-Host "Servicos iniciados com sucesso! Use .\parar.ps1 ou .\parar.bat para encerrar.`n" -ForegroundColor Green