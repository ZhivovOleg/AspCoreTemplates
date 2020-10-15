# Introduction
Набор шаблонов для команды ```dotnet new``` для создания приложений AspCore 

# Include

1. _AspCore.Microservices.Template_ - шаблон стандартного микросервиса на платформе netcore 3.1, с подготовленными проверками работоспособности (HealthChecks), автодокументируемым API (swagger) и postgres-ready ORM (EFCore).
    - install: dotnet new saltwebapi

# Build

Attention! DO NOT RESTORE, BUILD!

1. Download repo
    ```
    git clone https://github.com/ZhivovOleg/AspCoreTemplates.git
    ```

0. Move into repo
    ```
    cd aspcoretemplates
    ```

0. Prepare package (windows powershell)
    ```
    dotnet pack --configuration Release --force --output .\ -p:PackageVersion=$(Get-Date -Format 'yyyy.MMdd.HHmm')         
    ```

0. Push to Nuget
    ```
    dotnet nuget push .\AspCore.Microservices.Templates.2020.723.1332.nupkg --source <YOUR NUGET SOURCE> --api-key <API_KEY>
    ```

# Using

1. Connect to nuget source
    ```
    dotnet nuget add source <YOUR NUGET SOURCE> --name <NUGET NAME> --username <NAME> --password <PASSWORD> 
    ```

0. Install templates
    ```
    dotnet new -i AspCore.Microservices.Templates --nuget-source <NUGET NAME>
    ```

0. Create new service from template
    ```
    mkdir ServiceFromTemplate
    cd .\ServiceFromTemplate
    dotnet new saltwebapi
    ```

# Remove

```
dotnet new -u AspCore.Microservices.Templates
```