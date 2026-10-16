@echo off
title ProjetoHQ - Parar
cls
echo ============================================================
echo           FINALIZANDO SERVICOS DO PROJETOHQ
echo ============================================================
echo.

echo Encerrando API .NET (porta 9001)...
taskkill /F /IM "ProjetoHQApi.WebApi.exe" /T 2>nul
for /f "tokens=5" %%a in ('netstat -aon ^| findstr ":9001" ^| findstr "LISTENING"') do taskkill /F /PID %%a 2>nul

echo Encerrando Frontend Vite (porta 5173)...
for /f "tokens=5" %%a in ('netstat -aon ^| findstr ":5173" ^| findstr "LISTENING"') do taskkill /F /PID %%a 2>nul

echo Fechando janelas do ProjetoHQ...
taskkill /F /FI "WINDOWTITLE eq ProjetoHQ - Backend API*" /T 2>nul
taskkill /F /FI "WINDOWTITLE eq ProjetoHQ - Frontend Web*" /T 2>nul

echo.
echo Servicos finalizados e portas liberadas com sucesso.
ping 127.0.0.1 -n 3 > nul
exit