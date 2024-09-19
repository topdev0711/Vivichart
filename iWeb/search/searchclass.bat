@echo off
setlocal enabledelayedexpansion

:: Change to the parent directory
cd ..

:: Create or clear the output files in the 'search' subdirectory
type nul > search\found.txt
type nul > search\notfound.txt

:: Loop through each CSS file in the 'search' subdirectory
for %%c in (search\*.css) do (
    :: Loop through each line in the current CSS file
    echo %%~nxc
    echo %%~nxc >> search\found.txt
    echo %%~nxc >> search\notfound.txt

    for /f "tokens=*" %%a in (%%c) do (
        set "searchWord=%%a"
        echo    !searchWord!

        :: Search for the word in .vb and .aspx files in the current directory and its subdirectories
        findstr /m /i /s "!searchWord!" *.vb *.aspx *.js *.master > nul
        if not errorlevel 1 (
            echo   !searchWord! >> search\found.txt
        ) else (
            echo   !searchWord! >> search\notfound.txt
        )
    )
)
endlocal
