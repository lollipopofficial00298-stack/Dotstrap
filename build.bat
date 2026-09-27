@echo off
setlocal

set CONFIG=Release
set RID=win-x64
set PROJECT=Dotstrap\Dotstrap.csproj
set PUBLISH_DIR=Dotstrap\bin\%CONFIG%\net8.0-windows\%RID%\publish

echo.
echo === Building Dotstrap (%CONFIG%, %RID%) as a single-file exe ===
echo.

dotnet publish "%PROJECT%" -c %CONFIG% -r %RID% --self-contained false -p:PublishSingleFile=true
if errorlevel 1 (
    echo.
    echo Build failed.
    exit /b 1
)

if not exist "%PUBLISH_DIR%\Dotstrap.exe" (
    echo.
    echo Build succeeded but Dotstrap.exe was not found in %PUBLISH_DIR%
    exit /b 1
)

copy /y "%PUBLISH_DIR%\Dotstrap.exe" "%~dp0Dotstrap.exe" >nul

echo.
echo === Done ===
echo Single-file exe: %~dp0Dotstrap.exe
echo Full publish output: %PUBLISH_DIR%
echo.

endlocal
