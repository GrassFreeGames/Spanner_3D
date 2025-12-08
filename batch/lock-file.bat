@echo off
cd /d "C:\[YourProjectPath]"
set /p filepath="Enter file path to lock: "
git lfs lock "%filepath%"
pause