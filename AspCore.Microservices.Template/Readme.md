# Intro
шаблон стандартного микросервиса на платформе netcore 3.1, с подготовленными проверками работоспособности (HealthChecks), автодокументируемым API (swagger) и postgres-ready ORM (EFCore)

# Build

## Linux (CentOS 8)
    
1. 
    ``` 
    sudo dnf install dotnet-sdk-3.1 
    ```

0. 
    ``` 
    cd <full path to repo>
    ```

0. 
    ```bash
    publishDirectory = <Full path to publish directory>
    appName = <Current app name>;
    version = date +'%Y.%m%d.%H%M';
    dotnet clean;
    dotnet restore;
    dotnet build -c Release;
    dotnet publish -c Release -o "$publishDirectory/$appName" --runtime linux-x64 --force -p:Version=$version --no-self-contained;
    ``` 
 
## Windows
    
1. Install [aspnet core SDK](https://dotnet.microsoft.com/download/dotnet-core/3.1)

0. 
    ``` 
    cd <full path to repo>
    ```

0. 
    ```powershell
    [string]$AppName = "AspCore.Microservices.Reports";
    [string]$publishDirectory = "d:/Release/";
    [string]$runtime = "win-x64";
    [string]$version = Get-Date -Format 'yyyy.MMdd.HHmm';
    dotnet clean;
    dotnet restore;
    dotnet build -c Release;
    dotnet publish -c Release -o "$($publishDirectory)$($AppName)" --runtime $runtime --force -p:Version=$version -p:OutputType=WinExe --no-self-contained;
    ``` 

### Production

1. ``` sudo dnf install aspnetcore-runtime-3.1 ```

0. 

# Build and Pack (for linux from windows)

```
[string]$AppName = "AspCore.Microservices.Example";
[string]$publishDirectory = "с:/Release/";
[string]$runtime = "linux-x64";
[string]$version = Get-Date -Format 'yyyy.MMdd.HHmm';
Clear-Host;
Remove-Item -Path "$($publishDirectory)$($AppName)" -Recurse -Force -ErrorAction Ignore;
dotnet clean;
dotnet restore;
dotnet build -c Release;
dotnet publish -c Release -o "$($publishDirectory)$($AppName)" --runtime $runtime --force -p:Version=$version --no-self-contained;
Compress-Archive -Path "$($publishDirectory)$($AppName)/*" -DestinationPath "$($publishDirectory)$($AppName).$($runtime).$($version).zip" -Force;
Remove-Item -Path "$($publishDirectory)$($AppName)" -Recurse -Force -ErrorAction Ignore;
```

```
dotnet new -i <full path to template.csproj folder>
```

## Uninstall template 

```
dotnet new -u <full path to template.csproj folder>
```