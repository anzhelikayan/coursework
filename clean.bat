@echo off
chcp 65001 >nul

echo ========================================
echo Очищення папок збірки
echo ========================================
echo.

cd /d "%~dp0"

if not exist "BusStationTicketSystem" (
    echo [ПОМИЛКА] Папка BusStationTicketSystem не знайдена!
    pause
    exit /b 1
)

cd BusStationTicketSystem

echo Очищення папок bin та obj...
if exist "bin" (
    rmdir /s /q "bin" 2>nul
    if %errorlevel% equ 0 (
        echo [OK] Папка bin видалена
    ) else (
        echo [WARNING] Не вдалося видалити папку bin (можливо, файли використовуються)
    )
) else (
    echo [INFO] Папка bin не існує
)

if exist "obj" (
    rmdir /s /q "obj" 2>nul
    if %errorlevel% equ 0 (
        echo [OK] Папка obj видалена
    ) else (
        echo [WARNING] Не вдалося видалити папку obj (можливо, файли використовуються)
    )
) else (
    echo [INFO] Папка obj не існує
)

echo.
echo ========================================
echo Очищення завершено!
echo ========================================
echo.
pause

