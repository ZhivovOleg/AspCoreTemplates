﻿[string]$AppName = "SALT.WebApi.Example";
[string]$publishDirectory = "с:/Release/";
[string]$runtime = "win-x64";
[string]$version = Get-Date -Format 'yyyy.MMdd.HHmm';
Clear-Host;
Remove-Item -Path "$($publishDirectory)$($AppName)" -Recurse -Force -ErrorAction Ignore;
dotnet clean;
dotnet restore;
dotnet build -c Release;
dotnet publish -c Release -o "$($publishDirectory)$($AppName)" --runtime $runtime --force -p:Version=$version -p:OutputType=WinExe --no-self-contained;
Compress-Archive -Path "$($publishDirectory)$($AppName)/*" -DestinationPath "$($publishDirectory)$($AppName).$($runtime).$($version).zip" -Force;
Remove-Item -Path "$($publishDirectory)$($AppName)" -Recurse -Force -ErrorAction Ignore;