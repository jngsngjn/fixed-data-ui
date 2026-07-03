@echo off
setlocal

set "CSC=%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
if not exist "%CSC%" set "CSC=%WINDIR%\Microsoft.NET\Framework\v4.0.30319\csc.exe"

if not exist "%CSC%" (
    echo C# compiler not found.
    exit /b 1
)

if not exist dist mkdir dist

"%CSC%" /nologo /target:winexe /optimize+ /codepage:65001 /win32icon:assets\app.ico /out:dist\FixedDataUi.exe /reference:System.dll /reference:System.Drawing.dll /reference:System.Windows.Forms.dll src\FixedDataUi.cs
if errorlevel 1 exit /b 1

echo dist\FixedDataUi.exe created.
