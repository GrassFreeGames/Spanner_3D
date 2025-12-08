@echo off
cd /d "C:\[YourProjectPath]"
set /p msg="Commit message: "
git add -A
git commit -m "%msg%"
pause