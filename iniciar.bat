@echo off
chcp 65001 > nul
title Inicializador ProjetoHQ
cls
echo ============================================================
echo           🚀 INICIANDO PROJETOHQ (FULL STACK)
echo ============================================================
echo.

:: 1. Inicia o Backend (.NET 10 WebAPI)
echo [1/2] Iniciando Backend (.NET 10 API em https://localhost:9001)...
start "ProjetoHQ - Backend API" cmd /k "cd /d %~dp0ProjetoHQAPI\ProjetoHQAPI.WebApi && dotnet run"

:: 2. Inicia o Frontend (Vite + React)
echo [2/2] Iniciando Frontend (Vite + React em http://localhost:5173)...
start "ProjetoHQ - Frontend Web" cmd /k "cd /d %~dp0ProjetoHQWeb && npm run dev"

echo.
echo ============================================================
echo  Aguardando inicialização dos serviços (4 segundos)...
echo ============================================================
timeout /t 4 /nobreak > nul

:: Abre o navegador padrão na tela da aplicação
start http://localhost:5173

echo.
echo ✅ Ambos os serviços foram iniciados em janelas dedicadas.
echo    - Para desligar tudo facilmente, execute o arquivo 'parar.bat'.
echo.
timeout /t 5 > nul
exit
