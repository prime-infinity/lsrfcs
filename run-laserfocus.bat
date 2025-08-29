@echo off
echo Starting LaserFocus Productivity App...
echo.
echo Note: This application requires administrator privileges to function properly.
echo If prompted by Windows UAC, please click "Yes" to allow the application to run.
echo.
pause
cd /d "%~dp0"
dotnet run --project src/LaserFocus/LaserFocus.csproj
pause