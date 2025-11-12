@echo off
echo.
echo ======================================
echo   PRE-COMMIT VALIDATION
echo ======================================
echo.

git diff --cached --quiet
if errorlevel 1 (
    echo Checking staged files...
    echo.
    git diff --cached --name-only | findstr /i "\.prefab$ \.unity$"
    if not errorlevel 1 (
        echo.
        echo WARNING: Unity files detected!
        echo.
        echo CHECKLIST:
        echo   [ ] Did you edit prefabs in Prefab Mode?
        echo   [ ] Did you test in Play mode?
        echo   [ ] Are you the owner of these files?
        echo   [ ] Did you announce in team chat?
        echo.
        pause
    ) else (
        echo No Unity files in this commit.
        echo Ready to commit!
    )
) else (
    echo.
    echo No files staged for commit.
    echo Run: git add [files]
)

echo.
echo ======================================
pause
