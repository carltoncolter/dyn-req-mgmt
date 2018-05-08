@echo off
pushd "%~dp0"
REM pack+import must be run from the solution package
call "..\..\SolutionPackage\spkl\pack.bat"
popd