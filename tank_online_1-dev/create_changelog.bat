@echo off
setlocal enabledelayedexpansion

REM =========================================
REM  create_changelog.bat (Enhanced Version)
REM  - Builds CHANGELOG.md from commit messages grouped by DATE
REM  - Groups by date, then by type (Features/Bugs/Performance/etc.)
REM  - UTF-8 output
REM
REM  Usage:
REM    create_changelog.bat [branch] [mode] [order] [edit]
REM      branch     : which branch to read (default: dev)
REM      mode       : "since-tag" to include only since latest tag
REM      order      : "reverse" for oldest -> newest
REM      edit       : "edit" to open Notepad after generating
REM =========================================

pushd "%~dp0"
chcp 65001 >nul

where git >nul 2>&1
if errorlevel 1 (
  echo [ERROR] Git is not installed or not in PATH.
  goto :end
)

git rev-parse --is-inside-work-tree >nul 2>&1
if errorlevel 1 (
  echo [ERROR] This folder is not a Git repository.
  goto :end
)

set "BRANCH=%~1"
if "%BRANCH%"=="" set "BRANCH=dev"

set "MODE=%~2"
set "ORDER=%~3"
set "EDITORFLAG=%~4"

set "OPENEDITOR=0"
if /I "%EDITORFLAG%"=="edit" set "OPENEDITOR=1"

echo.
echo ========================================
echo  Generating CHANGELOG.md
echo ========================================
echo  Branch: %BRANCH%
echo  Mode: %MODE%
echo  Order: %ORDER%
echo ========================================
echo.

git fetch --all --tags --prune >nul 2>&1
if errorlevel 1 (
  echo [WARNING] git fetch failed, using local data.
)

set "REV=origin/%BRANCH%"
set "LAST_TAG="

if /I "%MODE%"=="since-tag" (
  for /f "usebackq delims=" %%T in (`git describe --tags --abbrev^=0 origin/%BRANCH% 2^>nul`) do set "LAST_TAG=%%T"
  if defined LAST_TAG (
    set "REV=!LAST_TAG!..origin/%BRANCH%"
    echo  Using range: !LAST_TAG!..origin/%BRANCH%
    echo.
  ) else (
    echo  [INFO] No tag found. Using full history.
    echo.
  )
)

set "REV_ORDER="
if /I "%ORDER%"=="reverse" set "REV_ORDER=--reverse"

REM Export commits with date, hash, and subject
set "TMP_LOG=%TEMP%\_gitlog_commits.txt"
echo  Fetching commits...
git log %REV% --no-merges --pretty=format:"%%ad|%%h|%%s" --date=short %REV_ORDER% > "!TMP_LOG!"
if errorlevel 1 (
  echo [ERROR] git log failed.
  goto :end
)

REM Count commits
for /f %%A in ('type "!TMP_LOG!" ^| find /c /v ""') do set "COMMIT_COUNT=%%A"
echo  Found %COMMIT_COUNT% commits
echo.

REM Call PowerShell script to parse and generate
echo  Generating CHANGELOG.md...
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0generate_changelog.ps1" -InputFile "!TMP_LOG!" -OutputFile "CHANGELOG.md" -Branch "%BRANCH%" -OpenEditor %OPENEDITOR%

if errorlevel 1 (
  echo [ERROR] Failed to generate CHANGELOG.md
  goto :end
)

echo.
echo ========================================
echo  SUCCESS!
echo ========================================
if defined LAST_TAG echo  Since tag: %LAST_TAG%
echo  Output: CHANGELOG.md
echo  Commits: %COMMIT_COUNT%
echo ========================================
echo.
echo Usage tips:
echo   create_changelog.bat dev              - Full history, newest first
echo   create_changelog.bat dev since-tag    - Since latest tag only
echo   create_changelog.bat dev "" reverse   - Oldest first
echo   create_changelog.bat dev "" "" edit   - Open in Notepad
echo.

:end
popd
endlocal
exit /b