@echo off
cd /d "C:\[YourProjectPath]"
echo Syncing latest...
git pull
echo.
echo === Files Currently Locked ===
git lfs locks
echo.
echo Ready to work!
pause