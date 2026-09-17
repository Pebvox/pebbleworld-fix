@echo off
chcp 65001 > nul
echo Остановка Zapret...
net stop zapret >nul 2>&1
sc delete zapret >nul 2>&1
taskkill /f /im winws.exe >nul 2>&1
echo Обход полностью остановлен.
pause
