@echo off
:: E2E Map Editor Installer – Windows launcher
:: Double-click this file (or run from Command Prompt) to install.
:: Pass "uninstall" as an argument to uninstall: install.bat uninstall

title E2E Map Editor Installer

:: Check for Python 3
python --version >nul 2>&1
if %errorlevel% neq 0 (
    echo Python 3 is required but was not found on your PATH.
    echo Download it from https://www.python.org/downloads/ and try again.
    echo.
    pause
    exit /b 1
)

:: Run the installer script
if "%1"=="uninstall" (
    python "%~dp0install.py" uninstall
) else (
    python "%~dp0install.py"
)

echo.
pause
