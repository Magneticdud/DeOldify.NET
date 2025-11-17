@echo off
MD "Release" 2>nul

REM Try to find the latest Roslyn C# compiler
set CSC_PATH=

REM Check for Visual Studio 2022
if exist "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\Roslyn\csc.exe" (
    set CSC_PATH=C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\Roslyn\csc.exe
)
if exist "C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\Roslyn\csc.exe" (
    set CSC_PATH=C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\Roslyn\csc.exe
)
if exist "C:\Program Files\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\Roslyn\csc.exe" (
    set CSC_PATH=C:\Program Files\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\Roslyn\csc.exe
)

REM Check for Visual Studio 2019
if "%CSC_PATH%"=="" (
    if exist "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\Roslyn\csc.exe" (
        set CSC_PATH=C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\Roslyn\csc.exe
    )
    if exist "C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\Roslyn\csc.exe" (
        set CSC_PATH=C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\Roslyn\csc.exe
    )
)

REM Check for Build Tools
if "%CSC_PATH%"=="" (
    if exist "C:\Program Files (x86)\Microsoft Visual Studio\2019\BuildTools\MSBuild\Current\Bin\Roslyn\csc.exe" (
        set CSC_PATH=C:\Program Files (x86)\Microsoft Visual Studio\2019\BuildTools\MSBuild\Current\Bin\Roslyn\csc.exe
    )
)

REM Fall back to old compiler if nothing else found
if "%CSC_PATH%"=="" (
    echo WARNING: Modern C# compiler not found. Using legacy compiler (C# 5 only).
    echo For better compatibility, install Visual Studio or Build Tools.
    set CSC_PATH=C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe
)

echo Using compiler: %CSC_PATH%
"%CSC_PATH%" @"Windows.artistic.rsp"

if %ERRORLEVEL% EQU 0 (
    echo.
    echo Build successful! Output: Release\DeOldify.NET.artistic.windows.exe
) else (
    echo.
    echo Build failed!
)

pause