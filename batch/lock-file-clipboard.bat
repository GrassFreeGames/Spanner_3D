@echo off
cd /d "C:\[YourProjectPath]"
for /f "delims=" %%a in ('powershell Get-Clipboard') do set filepath=%%a
git lfs lock "%filepath%"
echo Locked: %filepath%
pause