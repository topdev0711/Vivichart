@echo off
setlocal enabledelayedexpansion

:: Create or clear the output files
type nul > found.txt
type nul > notfound.txt

for /f "tokens=*" %%a in (list.txt) do (
    set "searchWord=%%a"
    echo Searching for: !searchWord!

    findstr /m /i /C:"!searchWord!" *.css > nul

    if not errorlevel 1 (
        echo !searchWord! >> found.txt
    ) else (
        echo !searchWord! >> notfound.txt
    )
)
pause
