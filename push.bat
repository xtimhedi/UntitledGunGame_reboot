@echo off
set "msg=%~1"

:: If no message was provided as an argument, prompt for one
if "%msg%"=="" (
    set /p "msg=Enter commit message: "
)

:: Check if the user entered an empty message
if "%msg%"=="" (
    echo [ERROR] Commit message cannot be empty.
    pause
    exit /b 1
)

echo.
echo === Adding files ===
git add .

echo.
echo === Committing changes ===
git commit -m "%msg%"

echo.
echo === Pushing to main ===
git push -u origin main

echo.
echo Done!
pause