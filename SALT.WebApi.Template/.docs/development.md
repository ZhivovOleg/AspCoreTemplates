# Development

Перед началом работы подготовьте окружение по инструкции [Developer Environment](environment.md).

## Documentation

- OpenAPI document: `https://localhost:11001/openapi/v1.json`
- API reference UI: `https://localhost:11001/scalar/v1`
- Confluence/Jira/Postman/ADR links: добавить ссылки проекта.

## Configuration

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

## Build

Локальная сборка должна проходить без warning/error. Анализаторы Roslyn и Sonar подключены к build pipeline, поэтому предупреждения анализаторов считаются ошибками.

```bash
dotnet restore
dotnet build --no-restore
```

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

## Pre-Merge Request Checks

Перед созданием merge request разработчик должен запустить локальный pre-MR скрипт.

Linux/macOS:

```bash
.DevOps/Scripts/check_before_mr.sh
```

Windows PowerShell:

```powershell
.\.DevOps\Scripts\check_before_mr.ps1
```

Скрипт выполняет:

1. `dotnet restore`;
2. `dotnet build`;
3. проверку уязвимых NuGet-пакетов;
4. `dotnet test` с coverage;
5. coverage gate;
6. `gitleaks protect --staged`;
7. `gitleaks dir` для текущего рабочего дерева.

Если нужно временно поднять локальный порог покрытия:

```bash
COVERAGE_LINE_THRESHOLD=70 .DevOps/Scripts/check_before_mr.sh
```

PowerShell:

```powershell
.\.DevOps\Scripts\check_before_mr.ps1 -CoverageLineThreshold 70
```

Локальные проверки не заменяют CI/CD. Pipeline остается источником истины и обязан повторять критичные проверки.

## Secret Scanning

В репозитории есть `.gitleaks.toml` с базовыми правилами:

- стандартные правила Gitleaks;
- запрет inline-паролей в connection strings;
- запрет секретоподобных значений в `appsettings*.json`;
- запрет hardcoded `Authorization: Bearer ...`;
- allowlist для безопасных placeholders вроде `<password>` и `<token>`.

Нельзя хранить реальные пароли, токены и API keys в:

- `appsettings*.json`;
- `.http`;
- `.env`;
- исходном коде;
- README/документации;
- тестовых snapshots.

Используйте User Secrets локально, переменные среды в CI/CD и secret store платформы в production.

## Tests And Coverage

Тестовые проекты лежат в `src/tests` и разделяются по назначению:

- `SALT.WebApi.Template.UnitTests` - быстрые unit-тесты чистой бизнес-логики без запуска ASP.NET Core host и внешних зависимостей;
- `SALT.WebApi.Template.ContractTests` - проверка публичных контрактов сервиса: OpenAPI сейчас, gRPC/AsyncAPI в будущем;
- `SALT.WebApi.Template.IntegrationTests` - интеграционные тесты с реальными зависимостями, например PostgreSQL через Testcontainers.

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
