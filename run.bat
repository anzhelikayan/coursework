@echo off
chcp 65001 >nul
setlocal enabledelayedexpansion

echo ========================================
echo Запуск BusStationTicketSystem
echo ========================================
echo.

cd /d "%~dp0"

if not exist "BusStationTicketSystem" (
    echo [ПОМИЛКА] Папка BusStationTicketSystem не знайдена!
    echo Поточний шлях: %CD%
    pause
    exit /b 1
)

cd BusStationTicketSystem

set EXE_PATH_RELEASE=bin\Release\net8.0-windows\BusStationTicketSystem.exe
set EXE_PATH_DEBUG=bin\Debug\net8.0-windows\BusStationTicketSystem.exe

set EXE_PATH=
if exist "%EXE_PATH_RELEASE%" (
    set EXE_PATH=%EXE_PATH_RELEASE%
    echo [OK] Знайдено Release версію
) else if exist "%EXE_PATH_DEBUG%" (
    set EXE_PATH=%EXE_PATH_DEBUG%
    echo [OK] Знайдено Debug версію
) else (
    echo ========================================
    echo [ПОМИЛКА] Виконуваний файл не знайдено!
    echo ========================================
    echo.
    echo Перевірені шляхи:
    echo   - %EXE_PATH_RELEASE%
    echo   - %EXE_PATH_DEBUG%
    echo.
    echo Спочатку виконайте збірку проекту:
    echo   - build.bat (для Release)
    echo   - build_debug.bat (для Debug)
    echo.
    pause
    exit /b 1
)

echo.
echo Запуск програми: %EXE_PATH%
echo.

start "" "%EXE_PATH%"

if errorlevel 1 (
    echo.
    echo [ПОМИЛКА] Помилка при запуску програми!
    pause
    exit /b 1
) else (
    echo [УСПІХ] Програма запущена успішно!
    timeout /t 2 >nul
)

