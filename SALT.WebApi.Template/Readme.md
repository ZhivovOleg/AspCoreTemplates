# Title

Краткое описание сервиса.

# Description

Подробное описание функционала сервиса: назначение, основные сценарии, внешние зависимости и границы ответственности.

# Project Layout

Основной код сервиса расположен в `src/<ServiceName>`.

Корень репозитория оставлен для файлов решения, настроек шаблона, NuGet, IDE, CI/CD и общих build-настроек:

- `src/<ServiceName>` - Web API project, Dockerfile и runtime-конфигурация сервиса;
- `src/tests` - тестовые проекты;
- `.DevOps` - локальные HTTP-проверки, database scripts и publish-скрипты;
- `.vscode` - рекомендуемые настройки запуска, задач и расширений;
- `.docs` - подробная документация для разработки, CI/CD и эксплуатации;
- `Directory.Build.props` и `Directory.Packages.props` - общие правила сборки и версии NuGet-пакетов;
- `nuget.config` - источники пакетов.

# Guides

## Environment

- [Developer Environment](.docs/environment.md) - подготовка рабочего места.
- [Prerequisites](.docs/environment.md#prerequisites) - .NET SDK, Docker, PostgreSQL, Gitleaks.
- [Setup](.docs/environment.md#setup) - restore, secrets и первый запуск сервиса.
- [IDE](.docs/environment.md#ide) - рекомендуемая конфигурация VS Code.
- [Local Tools](.docs/environment.md#local-tools) - восстановление `.config/dotnet-tools.json`.
- [Gitleaks](.docs/environment.md#gitleaks) - установка и локальная проверка секретов.
- [Local Secrets](.docs/environment.md#local-secrets) - настройка User Secrets.

## Development

- [Development](.docs/development.md) - ежедневная разработка сервиса.
- [Documentation](.docs/development.md#documentation) - OpenAPI, Scalar и проектные ссылки.
- [Configuration](.docs/development.md#configuration) - connection strings, secrets и CORS.
- [Build](.docs/development.md#build) - restore/build.
- [Run](.docs/development.md#run) - запуск сервиса.
- [HTTP Checks](.docs/development.md#http-checks) - OpenAPI, Scalar и `.http` запросы.
- [Observability](.docs/development.md#observability) - локальная проверка OpenTelemetry.
- [Structured Logging](.docs/development.md#structured-logging) - `[LoggerMessage]` и structured properties.
- [Static Analysis](.docs/development.md#static-analysis) - проверки пакетов и анализаторов.
- [Pre-Merge Request Checks](.docs/development.md#pre-merge-request-checks) - локальный pre-MR скрипт.
- [Secret Scanning](.docs/development.md#secret-scanning) - правила работы с секретами.
- [Tests And Coverage](.docs/development.md#tests-and-coverage) - unit, contract, integration tests и coverage gate.

## CI/CD

- [CI/CD](.docs/ci-cd.md) - общий pipeline flow, publish и deploy.
- [Common Flow](.docs/ci-cd.md#common-flow) - security gates, quality gates, tests, coverage, Sonar, publish, deploy.
- [Build Agent](.docs/ci-cd.md#build-agent) - требования к агенту.
- [Security Gates](.docs/ci-cd.md#security-gates) - Gitleaks, NuGet audit, SAST/OWASP.
- [Quality Gates](.docs/ci-cd.md#quality-gates) - build с анализаторами.
- [Tests And Coverage](.docs/ci-cd.md#tests-and-coverage) - CI coverage collection.
- [Sonar Analysis](.docs/ci-cd.md#sonar-analysis) - SonarScanner for .NET.
- [Manual Build](.docs/ci-cd.md#manual-build) - ручной release flow.
- [Scripted Build](.docs/ci-cd.md#scripted-build) - `.DevOps/Scripts`.
- [GitLab CI/CD](.docs/ci-cd.md#gitlab-cicd) - готовый `.gitlab-ci.yml` и GitLab-specific checks.
- [GitHub Actions](.docs/ci-cd.md#github-actions) - пример workflow.
- [Deployment Observability](.docs/ci-cd.md#deployment-observability) - env vars для telemetry при deployment.

## Operations

- [Operations](.docs/operations.md) - эксплуатация сервиса.
- [Observability](.docs/operations.md#observability) - production telemetry model.
- [Logs](.docs/operations.md#logs) - сбор structured logs.
- [Metrics](.docs/operations.md#metrics) - OTLP и Prometheus metrics.
- [Traces](.docs/operations.md#traces) - distributed tracing.
- [Observability Schemes](.docs/operations.md#observability-schemes) - типовые схемы доставки сигналов.
- [Health Checks](.docs/operations.md#health-checks) - `/healthz`.
- [Runtime Checklist](.docs/operations.md#runtime-checklist) - проверки после выкладки.

# Road Map

## Planned Additions

- Адаптировать deployment-раздел под конкретную инфраструктуру проекта.
- Поднять coverage gate до командного стандарта после появления реальной бизнес-логики.
- Дополнить contract tests новыми типами контрактов, если появятся gRPC или AsyncAPI.

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
