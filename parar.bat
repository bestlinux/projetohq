@echo off
title ProjetoHQ - Parar
cls
echo ============================================================
echo           FINALIZANDO SERVICOS DO PROJETOHQ
echo ============================================================
echo.

echo Encerrando API .NET...
taskkill /F /IM "ProjetoHQApi.WebApi.exe" /T 2>nul

echo Encerrando janelas do ProjetoHQ...
taskkill /F /FI "WINDOWTITLE eq ProjetoHQ - Backend API*" /T 2>nul
taskkill /F /FI "WINDOWTITLE eq ProjetoHQ - Frontend Web*" /T 2>nul

echo.
echo Servicos finalizados com sucesso.
ping 127.0.0.1 -n 3 > nul
exit