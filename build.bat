@echo off
chcp 65001 >nul
setlocal enabledelayedexpansion

echo ========================================
echo Збірка BusStationTicketSystem (Release)
echo ========================================
echo.

cd /d "%~dp0"

echo Завершення запущених процесів...
taskkill /F /IM BusStationTicketSystem.exe >nul 2>&1
if %errorlevel% equ 0 (
    echo [OK] Процеси завершено
    timeout /t 1 >nul
) else (
    echo [INFO] Процеси не знайдено
)
echo.

if not exist "BusStationTicketSystem" (
    echo [ПОМИЛКА] Папка BusStationTicketSystem не знайдена!
    echo Поточний шлях: %CD%
    pause
    exit /b 1
)

cd BusStationTicketSystem

echo Перевірка наявності .NET SDK...
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo [ПОМИЛКА] .NET SDK не знайдено!
    echo Встановіть .NET 8.0 SDK з https://dotnet.microsoft.com/download
    pause
    exit /b 1
)

for /f "tokens=*" %%i in ('dotnet --version') do set DOTNET_VERSION=%%i
echo [OK] .NET SDK знайдено (версія: %DOTNET_VERSION%)
echo.

echo Очищення попередніх збірок...
if exist "bin" (
    rmdir /s /q "bin" 2>nul
    echo [OK] Папка bin видалена
)
if exist "obj" (
    rmdir /s /q "obj" 2>nul
    echo [OK] Папка obj видалена
)
echo.

echo Початок збірки (Release)...
dotnet build BusStationTicketSystem.csproj --configuration Release --verbosity minimal

if %errorlevel% equ 0 (
    echo.
    echo ========================================
    echo [УСПІХ] Збірка завершена успішно!
    echo ========================================
    echo.
    echo Виконуваний файл:
    echo   bin\Release\net8.0-windows\BusStationTicketSystem.exe
    echo.
    pause
    exit /b 0
) else (
    echo.
    echo ========================================
    echo [ПОМИЛКА] Збірка завершилася з помилками!
    echo ========================================
    echo.
    pause
    exit /b 1
)

