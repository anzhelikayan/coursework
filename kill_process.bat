@echo off
chcp 65001 >nul

echo ========================================
echo Завершення процесів BusStationTicketSystem
echo ========================================
echo.

taskkill /F /IM BusStationTicketSystem.exe >nul 2>&1

if %errorlevel% equ 0 (
    echo [OK] Процес BusStationTicketSystem.exe завершено
) else (
    echo [INFO] Процес BusStationTicketSystem.exe не знайдено (можливо, не запущений)
)

timeout /t 1 >nul

