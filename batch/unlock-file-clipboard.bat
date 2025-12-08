@echo off
cd /d "C:\[YourProjectPath]"
set /p filepath="Enter file path to unlock: "
git lfs unlock "%filepath%"
pause