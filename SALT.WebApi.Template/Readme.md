# Title

Краткое описание сервиса.

# Description

Подробное описание функционала сервиса: назначение, основные сценарии, внешние зависимости и границы ответственности.

# Documentation

- OpenAPI document: `https://localhost:11001/openapi/v1.json`
- API reference UI: `https://localhost:11001/scalar/v1`
- Confluence/Jira/Postman/ADR links: добавить ссылки проекта.

# Requirements

- .NET SDK 10
- PostgreSQL
- Docker, если сервис запускается или публикуется в контейнере
- Доступ к корпоративному NuGet/Sonar/GitLab, если они используются в проекте

# Project Layout

Основной код сервиса расположен в `src/<ServiceName>`.

Корень репозитория оставлен для файлов решения, настроек шаблона, NuGet, IDE, CI/CD и общих build-настроек:

- `src/<ServiceName>` — Web API project, Dockerfile и runtime-конфигурация сервиса;
- `.DevOps` — локальные HTTP-проверки, database scripts и publish-скрипты;
- `.vscode` — рекомендуемые настройки запуска, задач и расширений;
- `Directory.Build.props` и `Directory.Packages.props` — общие правила сборки и версии NuGet-пакетов;
- `nuget.config` — источники пакетов.

Такая структура оставляет место для будущего каталога `tests`, не смешивая тестовые проекты с production-кодом.

# Setup

1. Восстановить зависимости:

```bash
dotnet restore
```

2. Настроить строку подключения. Для локальной разработки используйте User Secrets, см. раздел `Development`.

3. Запустить сервис:

```bash
dotnet run --project src/SALT.WebApi.Template/SALT.WebApi.Template.csproj
```

# Configuration

Пароли, API-ключи и строки подключения с учетными данными не должны храниться в `appsettings*.json`.

В production строка подключения передается через secret store оркестратора или переменную среды:

```bash
ConnectionStrings__SharedDb="Host=db;Port=5432;Database=service;Username=service;Password=<password>"
```

Двойное подчеркивание `__` соответствует разделителю `:` в конфигурации ASP.NET Core.

Настройки CORS задаются в секции:

```json
{
  "Cors": {
    "AllowedOrigins": [
      "https://app.example.com"
    ]
  }
}
```

# Development

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

Перед началом работы рекомендуется выполнить в корне репозитория команду

```bash
dotnet tool restore
```
для установки необходимых утилит dotnet

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

## Build

Локальная сборка должна проходить без warning/error. Анализаторы Roslyn и Sonar подключены к build pipeline, поэтому предупреждения анализаторов считаются ошибками.

```bash
dotnet restore
dotnet build --no-restore
```

## Local Tools

Локальные .NET CLI tools зафиксированы в `.config/dotnet-tools.json`.

После создания сервиса из шаблона восстановите инструменты:

```bash
dotnet tool restore
```

В manifest входят:

- `dotnet-sonarscanner` — запуск SonarQube/SonarCloud анализа из CLI и CI;
- `dotnet-reportgenerator-globaltool` — генерация HTML/Markdown/Cobertura-отчетов по coverage.

Локальный tool manifest позволяет не ставить эти инструменты глобально и держать одну версию tooling в разработке и CI.

## Run

Обычный запуск:

```bash
dotnet run --project src/SALT.WebApi.Template/SALT.WebApi.Template.csproj
```

Запуск с авто-перезапуском:

```bash
dotnet watch run --project src/SALT.WebApi.Template/SALT.WebApi.Template.csproj
```

## HTTP Checks

OpenAPI document:

```text
https://localhost:11001/openapi/v1.json
```

API reference UI:

```text
https://localhost:11001/scalar/v1
```

Если используется VS Code REST Client, проектные запросы можно хранить в `.DevOps/test.http`.

## Observability

Сервис использует OpenTelemetry как vendor-neutral слой для traces, metrics и, при необходимости, logs.

Для локальной разработки можно поднять OpenTelemetry Collector, Grafana Alloy, Aspire Dashboard или другой OTLP-compatible backend и передать endpoint через переменные среды:

```bash
OTEL_SERVICE_NAME=SALT.WebApi.Template
OTEL_EXPORTER_OTLP_ENDPOINT=http://localhost:4317
OTEL_RESOURCE_ATTRIBUTES=deployment.environment=Development,service.namespace=SALT
```

Если backend принимает OTLP over HTTP вместо gRPC, дополнительно задайте protocol:

```bash
OTEL_EXPORTER_OTLP_PROTOCOL=http/protobuf
OTEL_EXPORTER_OTLP_ENDPOINT=http://localhost:4318
```

Минимальная локальная проверка:

1. запустить OTLP backend;
2. запустить сервис с `ASPNETCORE_ENVIRONMENT=Development`;
3. выполнить запрос к API или открыть Scalar UI;
4. убедиться, что появились ASP.NET Core traces, HTTP client traces и runtime metrics.

Если в проект добавлен Prometheus exporter, endpoint метрик обычно доступен по адресу:

```text
/metrics
```

Prometheus endpoint не должен быть публично доступен без сетевого ограничения или авторизации.

## Structured Logging

Для часто используемых лог-событий используйте source-generated logging через `[LoggerMessage]`.

Такой подход:

- избегает лишних allocations при выключенном уровне логирования;
- сохраняет structured properties для поиска в Loki/Elastic/Application Insights;
- делает `EventId`, уровень и текст события явными;
- помогает держать формат логов стабильным между релизами.

Пример:

```csharp
[LoggerMessage(
    EventId = 1001,
    Level = LogLevel.Error,
    Message = "Failed to load example model {ExampleId} from shared database")]
private static partial void LogGetModelFailed(
    ILogger logger,
    int exampleId,
    Exception exception);
```

Класс, содержащий такой метод, должен быть `partial`:

```csharp
internal sealed partial class ExampleDatabaseService
```

Рекомендации:

- используйте уникальные `EventId` в пределах компонента;
- добавляйте в message template только полезные атрибуты: идентификаторы, route, method, status, dependency name;
- не логируйте пароли, токены, connection strings и персональные данные;
- передавайте `Exception` отдельным параметром, а не через `{Exception}`;
- для необработанных HTTP-ошибок включайте `TraceId`, чтобы связать лог с trace и `ProblemDetails`.

## Static Analysis

Проверить устаревшие, deprecated и уязвимые пакеты:

```bash
dotnet list package --outdated
dotnet list package --deprecated
dotnet list package --vulnerable --include-transitive
```

## Tests And Coverage

Тестовые проекты лежат в `src/tests` и разделяются по назначению:

- `SALT.WebApi.Template.UnitTests` — быстрые unit-тесты чистой бизнес-логики без запуска ASP.NET Core host и внешних зависимостей;
- `SALT.WebApi.Template.ContractTests` — проверка публичных контрактов сервиса: OpenAPI сейчас, gRPC/AsyncAPI в будущем;
- `SALT.WebApi.Template.IntegrationTests` — интеграционные тесты с реальными зависимостями, например PostgreSQL через Testcontainers.

Обычный запуск всех тестов:

```bash
dotnet test
```

### Contract Tests

Contract tests защищают публичный API от случайных изменений. OpenAPI-тест запускает приложение через `WebApplicationFactory`, получает `/openapi/v1.json`, нормализует JSON и сравнивает его со snapshot-файлом.

Snapshot OpenAPI хранится в проекте contract-тестов:

```text
src/tests/SALT.WebApi.Template.ContractTests/Contracts/OpenApi/Snapshots/openapi.v1.json
```

Обычный запуск contract-тестов:

```bash
dotnet test src/tests/SALT.WebApi.Template.ContractTests/SALT.WebApi.Template.ContractTests.csproj
```

Если изменение OpenAPI ожидаемое, обновите snapshot специальным режимом:

```bash
UPDATE_OPENAPI_SNAPSHOT=true dotnet test src/tests/SALT.WebApi.Template.ContractTests/SALT.WebApi.Template.ContractTests.csproj
```

Для Windows PowerShell:

```powershell
$env:UPDATE_OPENAPI_SNAPSHOT = "true"
dotnet test .\src\tests\SALT.WebApi.Template.ContractTests\SALT.WebApi.Template.ContractTests.csproj
Remove-Item Env:\UPDATE_OPENAPI_SNAPSHOT
```

После обновления snapshot обязательно проверьте diff `openapi.v1.json`. Если изменение контракта ожидаемое, коммитьте snapshot вместе с изменением API. Если diff неожиданный, исправьте route, versioning, DTO или OpenAPI metadata.

### Coverage

Для сбора покрытия в тестовых проектах используется `coverlet.collector`.

Запуск тестов с покрытием:

```bash
dotnet test \
  --collect:"XPlat Code Coverage" \
  --results-directory TestResults
```

Для SonarQube/SonarCloud удобнее формат OpenCover:

```bash
dotnet test \
  --collect:"XPlat Code Coverage" \
  --results-directory TestResults \
  -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover
```

Проверить coverage gate:

```bash
.DevOps/Scripts/check_coverage.sh TestResults 60
```

Для Windows PowerShell:

```powershell
.\.DevOps\Scripts\check_coverage.ps1 -ResultsDirectory .\TestResults -LineThreshold 60
```

Порог по умолчанию в publish-скриптах: `1%` line coverage. Это bootstrap-порог для шаблона: он запрещает публиковать сервис совсем без тестов и coverage report. В реальном сервисе порог нужно явно поднять в CI/CD.

Linux/macOS:

```bash
COVERAGE_LINE_THRESHOLD=70 bash .DevOps/Scripts/publish_for_centos.sh
```

Windows:

```powershell
.\.DevOps\Scripts\publish_for_win.ps1 -CoverageLineThreshold 70
```

Publish-скрипты не создают артефакт, если тестовых проектов нет, тесты падают, coverage reports не созданы или line coverage ниже порога.

## Prerelease Packages

В шаблоне может использоваться prerelease-версия пакета `Asp.Versioning.OpenApi`.

Если prerelease-пакеты разрешены корпоративной политикой, оставьте соответствующее правило в `renovate.json`. Если prerelease-пакеты запрещены, замените пакет на стабильную версию, когда она станет доступна, или удалите правило для prerelease.

# CI/CD

## Build Agent

Build-agent должен иметь:

- .NET SDK 10;
- доступ к NuGet sources из `nuget.config`;
- Docker daemon, если запускаются integration tests с Testcontainers;
- права на запуск контейнеров и доступ к Docker socket;
- доступ к SonarQube/SonarCloud, если включен Sonar analysis;
- secret variables для `sonar.token`, NuGet/API keys, container registry credentials и других внешних систем;
- возможность публиковать build artifacts: publish archive, `TestResults`, coverage reports;
- shell, совместимый с выбранным publish-скриптом: Bash для Linux agent или PowerShell для Windows agent.

Для Linux agent с Testcontainers обычно достаточно Docker executor или shell executor с доступом к `/var/run/docker.sock`. Если используется Docker-in-Docker, настройте privileged mode и registry/DNS/proxy параметры согласно политике CI/CD платформы.

## Quality Gate

Перед сборкой артефакта pipeline должен выполнить те же проверки, что и локальная предрелизная сборка:

1. `dotnet restore`
2. `dotnet list package --outdated`
3. `dotnet list package --deprecated`
4. `dotnet list package --vulnerable --include-transitive`
5. `dotnet build -c Release --no-restore`
6. `dotnet test` с coverage
7. coverage gate
8. `dotnet publish`
9. упаковка publish output в артефакт

## Publish Scripts

В `.DevOps/Scripts` лежат локальные скрипты, повторяющие базовое поведение CI/CD перед созданием publish-артефакта.

Linux:

```bash
bash .DevOps/Scripts/publish_for_centos.sh
```

По умолчанию архив создается в `/tmp/release`. Директорию можно передать первым аргументом:

```bash
bash .DevOps/Scripts/publish_for_centos.sh ./Release
```

Windows:

```powershell
.\.DevOps\Scripts\publish_for_win.ps1
```

По умолчанию архив создается в `c:/Release/`. Директорию можно переопределить параметром:

```powershell
.\.DevOps\Scripts\publish_for_win.ps1 -PublishDirectory ".\Release"
```

Оба publish-скрипта останавливаются на первой ошибке. Если тестовые проекты не найдены, публикация считается ошибкой.

## Windows Service

Для регистрации опубликованного Windows build как Windows Service используйте:

```powershell
.\RegisterWinService.ps1
```

Скрипт нужно запускать из каталога опубликованного приложения, где находится `<ServiceName>.exe`.

## Static analysis

Полноценный SonarQube/SonarCloud анализ в CI запускается через SonarScanner for .NET:

```bash
dotnet tool install --global dotnet-sonarscanner

dotnet sonarscanner begin \
  /k:"<project-key>" \
  /d:sonar.host.url="<sonar-url>" \
  /d:sonar.token="<sonar-token>" \
  /d:sonar.cs.opencover.reportsPaths="TestResults/**/coverage.opencover.xml"

dotnet restore
dotnet build --no-restore
dotnet test \
  --no-build \
  --collect:"XPlat Code Coverage" \
  --results-directory TestResults \
  -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover

dotnet sonarscanner end \
  /d:sonar.token="<sonar-token>"
```

Токен Sonar должен храниться в secret store CI/CD, а не в репозитории.

В CI стоит публиковать `TestResults` как artifact, чтобы можно было скачать `.trx` и coverage-отчеты после падения pipeline.

## Observability

DevOps-инженер должен предоставить сервису backend для OpenTelemetry-сигналов. Рекомендуемый production-путь: экспортировать telemetry по OTLP в OpenTelemetry Collector или совместимый агент, а уже из него отправлять данные в Grafana/Tempo/Prometheus/Loki, Azure Monitor, Datadog, Elastic или другую платформу.

Минимальные переменные окружения для deployment:

```bash
OTEL_SERVICE_NAME=<service-name>
OTEL_EXPORTER_OTLP_ENDPOINT=http://otel-collector:4317
OTEL_RESOURCE_ATTRIBUTES=deployment.environment=<environment>,service.namespace=<namespace>
```

Для OTLP over HTTP:

```bash
OTEL_EXPORTER_OTLP_PROTOCOL=http/protobuf
OTEL_EXPORTER_OTLP_ENDPOINT=http://otel-collector:4318
```

Рекомендации для production:

- задавать `OTEL_SERVICE_NAME` явно, чтобы сервисы не смешивались в APM;
- добавлять `deployment.environment`, `service.namespace`, `service.version`;
- не отправлять персональные данные и секреты в tags, baggage, span names и log scopes;
- публиковать `/metrics` только внутри trusted-сети, если включен Prometheus scraping;
- health checks использовать для readiness/liveness, а не как замену metrics/traces;
- хранить настройки collector/agent в инфраструктурном репозитории или secret/config store.

### Схемы observability

#### В общем случае:

App
 ├─ logs    -> console stdout/stderr -> Promtail/Alloy/Fluent Bit -> Loki
 ├─ metrics -> /metrics или OTLP -> Prometheus / OpenTelemetry Collector
 └─ traces  -> OTLP -> OpenTelemetry Collector -> Tempo / Jaeger / Zipkin / APM

,либо если стек Grafana
#### Grafana stack:

App
 ├─ Console logs -> Grafana Alloy -> Loki
 ├─ OTLP metrics -> Grafana Alloy/Otel Collector -> Prometheus/Mimir
 └─ OTLP traces  -> Grafana Alloy/Otel Collector -> Tempo

,или если включен Prometheus exporter:
#### Prometheus exporter:

App ./metrics <- Prometheus scrape
App OTLP traces -> Collector/Alloy -> Tempo
App console logs -> Alloy/Promtail -> Loki

## Dependency Updates

Шаблон содержит базовый `renovate.json` для автоматического обновления зависимостей.

Renovate проверяет:

- NuGet-пакеты;
- Docker base images;
- GitLab CI images/includes, если они используются в проекте.

Конфиг сам по себе не запускает Renovate. Для работы нужен один из вариантов:

- централизованный Renovate bot, настроенный на GitLab group;
- scheduled GitLab pipeline, запускающий Renovate для проекта;
- корпоративная инсталляция Renovate Community/Enterprise.

Рекомендуемый подход для компании: настроить Renovate централизованно на уровне GitLab group, а в сервисах хранить только `renovate.json`.

# Operations

Health checks говорят “можно ли слать трафик”. 
Observability говорит “что происходит внутри”.

В проекте внедрен Observability как стандартный набор сигналов:
- Logs: что произошло.
- Metrics: насколько часто, быстро и тяжело работает сервис.
- Traces: где конкретный запрос провел время между сервисами, БД, HTTP-клиентами.

## Health Checks

Основной endpoint:

```text
/healthz
```

## Deployment

Описать способ публикации сервиса: Docker image, Kubernetes, systemd, Windows Service или другой принятый в проекте вариант.

# Road Map

## Planned Additions

- Добавить проект тестов.
- Добавить интеграционные тесты с Testcontainers.
- Добавить production-ready observability через OpenTelemetry.

## Current Issues

Список известных проблем.

## Changelog

30 july 2026

- move to .NET 10
- add static analysis
- cleanup settings
- move to User Secrets
- use `Directory.Packages.props`
- add Renovate for GitLab

03 october 2025

- add package [Microsoft.Extensions.Http.Resilience](https://www.nuget.org/packages/Microsoft.Extensions.Http.Resilience)
- [Создание устойчивых HTTP-приложений: ключевые шаблоны разработки](https://learn.microsoft.com/ru-ru/dotnet/core/resilience/http-resilience?tabs=dotnet-cli)

# Owners

- product owner:
- tech lead:
- backend owner:
- DevOps owner:
