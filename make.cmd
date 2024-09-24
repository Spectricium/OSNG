@echo off
cls
msbuild -noLogo -property:WarningLevel=4;OutDir=bin\output -nodeReuse:true