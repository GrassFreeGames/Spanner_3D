@echo off
cd /d "C:\[YourProjectPath]"
set /p filepath="Enter file path to FORCE unlock: "
git lfs unlock --force "%filepath%"
echo Force unlocked: %filepath%
pause