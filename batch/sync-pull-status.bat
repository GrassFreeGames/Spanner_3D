@echo off
cd /d "C:\[YourProjectPath]"
git pull
echo.
echo === Current Status ===
git status
echo.
echo === Current Locks ===
git lfs locks
pause