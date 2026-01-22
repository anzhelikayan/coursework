@echo off
chcp 65001 >nul
setlocal enabledelayedexpansion

echo ========================================
echo Збірка та запуск BusStationTicketSystem
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

echo ========================================
echo Крок 1: Завершення запущених процесів
echo ========================================
echo.

echo Завершення процесів BusStationTicketSystem...
taskkill /F /IM BusStationTicketSystem.exe >nul 2>&1
if %errorlevel% equ 0 (
    echo [OK] Процеси завершено
    timeout /t 1 >nul
) else (
    echo [INFO] Процеси не знайдено (можливо, не запущені)
)
echo.

echo ========================================
echo Крок 2: Очищення та збірка проекту
echo ========================================
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

set BUILD_RESULT=%errorlevel%

echo.
if %BUILD_RESULT% neq 0 (
    echo ========================================
    echo [ПОМИЛКА] Збірка завершилася з помилками!
    echo ========================================
    echo.
    echo Неможливо запустити програму через помилки компіляції.
    echo Перевірте вивід вище для деталей помилок.
    echo.
    pause
    exit /b %BUILD_RESULT%
)

echo [OK] Збірка успішна
echo.

set EXE_PATH=bin\Release\net8.0-windows\BusStationTicketSystem.exe

if not exist "%EXE_PATH%" (
    echo ========================================
    echo [ПОМИЛКА] Виконуваний файл не знайдено!
    echo ========================================
    echo.
    echo Очікуваний шлях: %EXE_PATH%
    echo.
    pause
    exit /b 1
)

echo ========================================
echo Крок 3: Запуск програми
echo ========================================
echo.

echo [OK] Файл знайдено: %EXE_PATH%
echo Запуск програми...
echo.

start "" "%EXE_PATH%"

if errorlevel 1 (
    echo.
    echo [ПОМИЛКА] Помилка при запуску програми!
    pause
    exit /b 1
) else (
    echo.
    echo [УСПІХ] Програма запущена успішно!
    echo.
    timeout /t 2 >nul
)

