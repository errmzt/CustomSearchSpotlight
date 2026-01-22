
### **📄 11. publish.bat:**

```batch
@echo off
echo ========================================
echo    CustomSearch Spotlight - Builder
echo ========================================
echo.

echo [1] Cleaning previous builds...
dotnet clean

echo [2] Restoring packages...
dotnet restore

echo [3] Building release version...
dotnet build -c Release

echo [4] Publishing single executable...
dotnet publish -c Release -r win10-x64 ^
  --self-contained true ^
  -p:PublishSingleFile=true ^
  -p:IncludeNativeLibrariesForSelfExtract=true ^
  -p:PublishReadyToRun=true ^
  -p:PublishTrimmed=true ^
  -o ./dist

echo.
echo ========================================
echo    BUILD COMPLETE!
echo ========================================
echo.
echo The application is ready in: %cd%\dist
echo.
echo To run: .\dist\CustomSearchApp.exe
echo.
pause
