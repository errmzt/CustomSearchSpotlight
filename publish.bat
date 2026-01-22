@echo off
echo Budowanie CustomSearch Spotlight...

REM Czyszczenie
dotnet clean

REM Publikacja
dotnet publish -c Release -r win10-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o ./publish

echo.
echo Gotowe! Plik .exe znajduje się w folderze ./publish
pause
