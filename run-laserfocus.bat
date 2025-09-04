@echo off
echo Starting LaserFocus Productivity App...
echo.
echo Note: This application requires administrator privileges to function properly.
echo If prompted by Windows UAC, please click "Yes" to allow the application to run.
echo.
pause
cd /d "%~dp0"

REM Clean and rebuild to ensure latest version
echo Cleaning and rebuilding to ensure latest version...
dotnet clean --configuration Debug --verbosity quiet
dotnet build --configuration Debug --verbosity quiet

REM Check if executable exists
if not exist "src\LaserFocus\bin\Debug\net9.0-windows\LaserFocus.exe" (
    echo Error: LaserFocus.exe not found. Build may have failed.
    echo Please check the build output above for errors.
    pause
    exit /b 1
)

REM Run the application
echo Starting application...
src\LaserFocus\bin\Debug\net9.0-windows\LaserFocus.exe
pause