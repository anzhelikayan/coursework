@echo off
chcp 65001 >nul

echo ========================================
echo Повна перезбірка BusStationTicketSystem
echo ========================================
echo.

cd /d "%~dp0"

echo Крок 1: Очищення...
call clean.bat

echo.
echo Крок 2: Збірка...
call build.bat

echo.
echo ========================================
echo Перезбірка завершена!
echo ========================================

