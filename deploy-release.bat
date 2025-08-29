@echo off
echo Building Laser Focus Release...

REM Clean previous builds
dotnet clean --configuration Release

REM Build the application
dotnet build --configuration Release --no-restore

REM Publish self-contained executable
dotnet publish src\LaserFocus\LaserFocus.csproj ^
    --configuration Release ^
    --runtime win-x64 ^
    --self-contained true ^
    --output "dist\LaserFocus-win-x64" ^
    --verbosity minimal

REM Create deployment package
if exist "dist\LaserFocus-Release.zip" del "dist\LaserFocus-Release.zip"
powershell -Command "Compress-Archive -Path 'dist\LaserFocus-win-x64\*' -DestinationPath 'dist\LaserFocus-Release.zip'"

echo.
echo Build completed successfully!
echo Output directory: dist\LaserFocus-win-x64
echo Package created: dist\LaserFocus-Release.zip
echo.
echo To run the application with administrator privileges:
echo Right-click LaserFocus.exe and select "Run as administrator"
pause