@echo off
echo Building Epic Markdown Manager...
echo.

REM Restore NuGet packages
echo Restoring packages...
dotnet restore

if %ERRORLEVEL% NEQ 0 (
    echo Error: Failed to restore packages. Make sure .NET SDK is installed.
    echo Download from: https://dotnet.microsoft.com/download
    pause
    exit /b %ERRORLEVEL%
)

REM Build the project
echo Building project...
dotnet build -c Release

if %ERRORLEVEL% NEQ 0 (
    echo Error: Build failed.
    pause
    exit /b %ERRORLEVEL%
)

REM Publish as single executable
echo Publishing as single executable...
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true -o publish

if %ERRORLEVEL% NEQ 0 (
    echo Error: Publish failed.
    pause
    exit /b %ERRORLEVEL%
)

echo.
echo Build complete!
echo Executable location: publish\EpicMarkdownManager.exe
echo.
pause