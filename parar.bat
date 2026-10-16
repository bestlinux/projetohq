@echo off
chcp 65001 > nul
title Parar ProjetoHQ
cls
echo ============================================================
echo           🛑 FINALIZANDO SERVIÇOS DO PROJETOHQ
echo ============================================================
echo.

echo Parando processo da API .NET...
taskkill /F /IM "ProjetoHQApi.WebApi.exe" /T 2>nul

echo Fechando janelas do ProjetoHQ...
taskkill /F /FI "WINDOWTITLE eq ProjetoHQ - Backend API*" /T 2>nul
taskkill /F /FI "WINDOWTITLE eq ProjetoHQ - Frontend Web*" /T 2>nul

echo.
echo ✅ Serviços finalizados com sucesso!
timeout /t 3 > nul
exit
