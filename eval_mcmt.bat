@echo off

echo BUILDING CMTS...

set path=https://github.com/HueSamai/CementMods/raw/main/

setlocal enabledelayedexpansion

set str=""
for /f "delims=" %%d in ('dir /ad/b') do (
    cd %%d

    if not "%%d" == ".vscode" (
        if not "%%d" == ".git" (
            echo. > %%d.cmt
            for /f "delims=" %%x in (%%d.mcmt) do (
                set str=%%x
                set str=!str:{}=%path%%%d!
                echo !str! >> %%d.cmt
            )
        )
    )
    cd ..
)

echo DONE