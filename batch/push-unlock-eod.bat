@echo off
cd /d "C:\[YourProjectPath]"
echo Pushing any uncommitted work...
git add -A
git status
set /p msg="Commit message (or press Enter to skip): "
if not "%msg%"=="" git commit -m "%msg%"
git push
echo.
echo Your current locks (unlock manually if done):
git lfs locks | findstr /i "%USERNAME%"
pause