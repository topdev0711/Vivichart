@echo off
setlocal enabledelayedexpansion

rem Create a temporary file to store the intermediate results
set "tempFile=temp_list.txt"

rem Run the findstr command and store results in the temporary file
findstr /I /S /R /C:"::LT_" /C:"::IN_" /C:"::WB_" /C:"::ER_" *.aspx* > "%tempFile%"

rem Clear the final output file if it already exists
if exist list del list

rem Process each line in the temporary file
for /f "tokens=1,* delims=:" %%a in (%tempFile%) do (
    set "fileName=%%a"
    set "fullLine=%%b"

    rem Extract the desired substring using findstr
    echo !fullLine! | findstr /R "::LT_[A-Z][0-9][0-9][0-9][0-9] ::IN_[A-Z][0-9][0-9][0-9][0-9] ::WB_[A-Z][0-9][0-9][0-9][0-9] ::ER_[A-Z][0-9][0-9][0-9][0-9]" > nul
    if not errorlevel 1 (
        for %%c in (::LT_ ::IN_ ::WB_ ::ER_) do (
            for /f "tokens=2 delims==>< " %%d in ('echo !fullLine! ^| findstr /C:"%%c"') do (
                set "substring=%%d"
                rem Append the filename and the extracted substring to the final list
                echo !fileName!:   !substring! >> list
            )
        )
    )
)

rem Clean up
del "%tempFile%"
endlocal
pause
