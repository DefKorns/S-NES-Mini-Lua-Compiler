@echo off
rem Builds a static LuaJIT 2.0.4 for Windows x64 from third_party\luajit (the source Nintendo publishes for the
rem consoles, see third_party\luajit\NINTENDO.md) and copies luajit.exe + jit\bcsave.lua to Lib\luajit.
rem Needs Visual Studio (or the Build Tools) with the C++ tools.
setlocal
rem the LuaJIT build scripts run tools from the current folder, which this Windows setting blocks
set "NoDefaultCurrentDirectoryInExePath="
set "ROOT=%~dp0.."
set "OUT_DIR=%ROOT%\Lib\luajit"

set "VSWHERE=%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe"
if not exist "%VSWHERE%" ( echo vswhere.exe not found: install Visual Studio with the C++ tools & exit /b 1 )
set "VSFILE=%TEMP%\luajit-vsdir-%RANDOM%.txt"
"%VSWHERE%" -latest -products * -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64 -property installationPath > "%VSFILE%"
set /p VS=<"%VSFILE%"
del "%VSFILE%"
if not defined VS ( echo Visual Studio with the C++ tools not found & exit /b 1 )
call "%VS%\VC\Auxiliary\Build\vcvars64.bat" >nul || exit /b 1

rem msvcbuild.bat builds inside the source tree, so build in a copy
set "WORK=%TEMP%\luajit-build-%RANDOM%"
xcopy /e /i /q /y "%ROOT%\third_party\luajit" "%WORK%" >nul || exit /b 1
pushd "%WORK%\src"
call "%WORK%\src\msvcbuild.bat" static || ( popd & exit /b 1 )
popd

if not exist "%OUT_DIR%\jit" mkdir "%OUT_DIR%\jit"
copy /y "%WORK%\src\luajit.exe" "%OUT_DIR%\luajit.exe" >nul
copy /y "%WORK%\src\jit\bcsave.lua" "%OUT_DIR%\jit\bcsave.lua" >nul
rmdir /s /q "%WORK%"
"%OUT_DIR%\luajit.exe" -v
