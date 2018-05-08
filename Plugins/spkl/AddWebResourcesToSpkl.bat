@echo off
pushd "%~dp0"
Powershell.exe -executionpolicy remotesigned -File  AddWebResourcesToSpkl.ps1
popd
pause
