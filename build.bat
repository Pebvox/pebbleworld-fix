@echo off
chcp 65001 > nul
set CSC="C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
if not exist %CSC% (
  for /f "tokens=*" %%i in ('where csc 2^>nul') do set CSC="%%i"
)
echo Компиляция PebbleFix.exe...
%CSC% /nologo /target:winexe /optimize+ /win32manifest:src\app.manifest /win32icon:src\icon.ico /r:System.dll /r:System.Core.dll /r:System.Windows.Forms.dll /r:System.Drawing.dll /r:System.ServiceProcess.dll /r:System.IO.Compression.dll /r:System.IO.Compression.FileSystem.dll /resource:src\bundle.zip,bundle.zip /out:PebbleFix.exe src\Program.cs
if %ERRORLEVEL% equ 0 (
  echo [OK] PebbleFix.exe успешно скомпилирован!
) else (
  echo [!] Ошибка компиляции.
)
