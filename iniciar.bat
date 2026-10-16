@echo off
title ProjetoHQ - Inicializador
cls
echo ============================================================
echo           INICIANDO PROJETOHQ (FULL STACK)
echo ============================================================
echo.

REM 1. Inicia o Backend (.NET 10 WebAPI)
echo [1/2] Iniciando Backend (.NET 10 API em https://localhost:9001)...
start "ProjetoHQ - Backend API" cmd /k "cd /d ""%~dp0ProjetoHQAPI\ProjetoHQAPI.WebApi"" && dotnet run"

REM 2. Inicia o Frontend (Vite + React)
echo [2/2] Iniciando Frontend (Vite + React em http://localhost:5173)...
start "ProjetoHQ - Frontend Web" cmd /k "cd /d ""%~dp0ProjetoHQWeb"" && npm run dev"

echo.
echo ============================================================
echo  Aguardando inicializacao dos servicos (5 segundos)...
echo ============================================================
ping 127.0.0.1 -n 6 > nul

REM Abre o navegador padrao na tela da aplicacao
start http://localhost:5173

echo.
echo Servicos iniciados com sucesso!
echo Para desligar tudo facilmente, execute 'parar.bat' ou 'parar.ps1'.
echo.
ping 127.0.0.1 -n 3 > nul
exit