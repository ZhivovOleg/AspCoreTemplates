# Developer Environment

Этот раздел описывает подготовку рабочего места разработчика после создания сервиса из шаблона.

## Prerequisites

Установите:

- .NET SDK 10;
- Docker, если нужны integration tests с Testcontainers;
- PostgreSQL или доступ к локальному/тестовому PostgreSQL;
- Gitleaks для проверки секретов;
- VS Code с рекомендованными расширениями, если команда использует VS Code;
- доступ к корпоративному NuGet/Sonar/GitLab, если они используются в проекте.

## Setup

1. Восстановить зависимости:

```bash
dotnet restore
```

2. Настроить строку подключения. Для локальной разработки используйте User Secrets, см. [Local Secrets](#local-secrets).

3. Запустить сервис:

```bash
dotnet run --project src/SALT.WebApi.Template/SALT.WebApi.Template.csproj
```

## IDE

Рекомендуемая IDE-конфигурация для VS Code хранится в `.vscode`.

Текущая конфигурация:

- `launch.json` запускает Web API через `coreclr`, перед стартом выполняет задачу `build`;
- запускаемый файл: `src/SALT.WebApi.Template/bin/Debug/net10.0/SALT.WebApi.Template.dll`;
- рабочий каталог: `src/SALT.WebApi.Template`;
- переменная окружения: `ASPNETCORE_ENVIRONMENT=Development`;
- `serverReadyAction` открывает URL, который приложение выводит в `Now listening on:`;
- `tasks.json` содержит задачи `build`, `publish`, `watch`.

Рекомендуемые расширения VS Code перечислены в `.vscode/extensions.json`. Они покрывают C#/.NET, EditorConfig, Docker, REST-запросы, YAML, PostgreSQL, NuGet, подсветку ошибок и проверку русского/английского текста.

## Local Tools

Локальные .NET CLI tools зафиксированы в `.config/dotnet-tools.json`.

После создания сервиса из шаблона восстановите инструменты:

```bash
dotnet tool restore
```

В manifest входят:

- `dotnet-sonarscanner` - запуск SonarQube/SonarCloud анализа из CLI и CI;
- `dotnet-reportgenerator-globaltool` - генерация HTML/Markdown/Cobertura-отчетов по coverage.

Локальный tool manifest позволяет не ставить эти инструменты глобально и держать одну версию tooling в разработке и CI.

## Gitleaks

Для локальной проверки секретов установите Gitleaks.

macOS:

```bash
brew install gitleaks
```

Linux:

```bash
curl -sSfL https://raw.githubusercontent.com/gitleaks/gitleaks/master/install.sh | sh -s -- -b /usr/local/bin
```

Windows через WinGet:

```powershell
winget install gitleaks
```

Windows через Chocolatey:

```powershell
choco install gitleaks
```

Проверить установку:

```bash
gitleaks version
```

Docker-вариант без установки на хост:

```bash
docker pull zricethezav/gitleaks:latest
docker run --rm -v "$PWD:/repo" -w /repo zricethezav/gitleaks:latest dir --redact --config .gitleaks.toml .
```

Gitleaks нужен не для runtime сервиса, а для разработки: он проверяет, что в код, `appsettings*.json`, `.http`, `.env`, markdown и другие файлы не попали пароли, токены, API keys и connection strings.

## Local Secrets

После создания сервиса из шаблона инициализируйте хранилище User Secrets из каталога проекта:

```bash
dotnet user-secrets init
```

Затем задайте строку подключения, например к локальному PostgreSQL:

```bash
dotnet user-secrets set \
  "ConnectionStrings:SharedDb" \
  "Host=localhost;Port=5432;Database=test_postgres;Username=postgres;Password=<password>"
```

Проверить сохраненные значения:

```bash
dotnet user-secrets list
```

Команда `init` добавляет уникальный `UserSecretsId` в `.csproj` созданного сервиса. Сам идентификатор не является секретом и может храниться в Git. Значения User Secrets сохраняются вне каталога проекта и предназначены только для локальной разработки.

## First Local Check

После подготовки окружения выполните короткую проверку:

```bash
dotnet tool restore
dotnet restore
dotnet build --no-restore
gitleaks dir --redact --config .gitleaks.toml .
```

Если эти команды проходят, рабочее место готово для обычной разработки.
