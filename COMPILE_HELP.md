# Compilation Help

## C# Compiler Version Issue

If you see the error: "This compiler is provided as part of the Microsoft (R) .NET Framework, but only supports language versions up to C# 5..."

### Quick Solutions

#### Solution 1: Use Developer Command Prompt (Easiest)
1. Install **Visual Studio 2019/2022** or **Build Tools for Visual Studio**
   - Download: https://visualstudio.microsoft.com/downloads/
2. Open **Developer Command Prompt for VS** from Start Menu
3. Navigate to the Implementation folder
4. Run the compile script: `Compile.artistic.bat`

#### Solution 2: Manual Compiler Path
If you have Visual Studio installed, manually run the compiler:

```bat
"C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\Roslyn\csc.exe" @"Windows.artistic.rsp"
```

Adjust the path based on your Visual Studio version and edition (Community/Professional/Enterprise).

#### Solution 3: Use the Updated Script
The `Compile.artistic.bat` script has been updated to automatically find the modern C# compiler. Just run it and it will:
1. Search for Visual Studio 2019/2022 Roslyn compiler
2. Use the modern compiler if found
3. Fall back to the old compiler with a warning if not found

### Installing Build Tools (No Visual Studio Required)

If you don't want the full Visual Studio IDE:

1. Download **Build Tools for Visual Studio 2022**:
   https://visualstudio.microsoft.com/downloads/#build-tools-for-visual-studio-2022

2. Run the installer and select:
   - **.NET desktop build tools**
   - Or just **C# compiler**

3. After installation, the compile scripts will automatically find the new compiler

### Verify Installation

To check if you have the modern compiler, open Command Prompt and run:
```bat
where csc
```

You should see paths to csc.exe. The Roslyn compiler will be in a path containing "Roslyn".

## All Compile Scripts Updated

The following script has been updated with automatic compiler detection:
- `Compile.artistic.bat`

You can apply the same pattern to other compile scripts if needed.
