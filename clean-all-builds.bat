@echo off
echo Cleaning all build outputs...
echo.

REM Clean solution
dotnet clean --verbosity quiet

REM Remove bin and obj directories
echo Removing bin and obj directories...
for /d /r . %%d in (bin,obj) do @if exist "%%d" rd /s /q "%%d"

REM Remove dist directory if it exists
if exist "dist" (
    echo Removing dist directory...
    rd /s /q "dist"
)

echo.
echo All build outputs cleaned!
echo Next run will build fresh from your latest code.
echo.
pause