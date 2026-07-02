# CI/CD

Этот раздел описывает рекомендуемый flow сборки и деплоя сервиса. Конкретная CI/CD-платформа может отличаться синтаксисом, но порядок проверок должен оставаться одинаковым.

## Common Flow

Pipeline должен идти от дешевых и критичных проверок к дорогим и publish/deploy шагам:

1. Restore: восстановить NuGet-пакеты и локальные .NET tools.
2. Security gates: проверить секреты, уязвимые зависимости и SAST/OWASP-правила.
3. Quality gates: собрать проект с анализаторами и treat warnings as errors.
4. Tests: запустить unit, contract и integration tests.
5. Coverage gate: собрать coverage report и проверить минимальный порог.
6. Sonar analysis: отправить build/test/coverage данные в SonarQube или SonarCloud.
7. Publish: собрать publish output и упаковать artifact.
8. Deploy: выложить artifact в целевую среду.

Артефакт нельзя собирать, если упали security gates, analyzers, tests или coverage gate. Deploy в production должен быть ручным, ограниченным по правам и доступным только из protected branch.

## Build Agent

Build-agent должен иметь:

- .NET SDK 10;
- доступ к NuGet sources из `nuget.config`;
- восстановленные local tools через `dotnet tool restore`;
- Gitleaks или Docker image `zricethezav/gitleaks`;
- Docker daemon, если запускаются integration tests с Testcontainers;
- права на запуск контейнеров и доступ к Docker socket;
- доступ к SonarQube/SonarCloud, если включен Sonar analysis;
- secret variables для `SONAR_TOKEN`, NuGet/API keys, container registry credentials и других внешних систем;
- возможность публиковать build artifacts: publish archive, `TestResults`, coverage reports;
- Bash для Linux agent или PowerShell для Windows agent.

Для Linux agent с Testcontainers обычно достаточно Docker executor или shell executor с доступом к `/var/run/docker.sock`. Если используется Docker-in-Docker, настройте privileged mode и registry/DNS/proxy параметры согласно политике CI/CD платформы.

## Security Gates

Обязательные проверки:

```bash
gitleaks dir --redact --config .gitleaks.toml .
dotnet list package --vulnerable --include-transitive
```

Рекомендуемые дополнительные проверки:

- `dotnet list package --deprecated`;
- `dotnet list package --outdated`;
- Semgrep с правилами OWASP Top 10 и secrets;
- OWASP Dependency-Check с fail threshold по CVSS;
- SonarQube/SonarCloud Quality Gate.

Gitleaks должен блокировать pipeline, если найдены пароли, токены, API keys или inline connection string credentials.

## Quality Gates

Основная quality-проверка:

```bash
dotnet restore
dotnet build -c Release --no-restore
```

Roslyn analyzers, Meziantou Analyzer и SonarAnalyzer.CSharp подключены к build pipeline. Предупреждения анализаторов считаются ошибками, поэтому отдельный "analyze" command для локальных Roslyn-правил не нужен.

## Tests And Coverage

Запуск тестов с coverage в формате OpenCover:

```bash
dotnet test \
  -c Release \
  --no-restore \
  --logger "trx" \
  --collect:"XPlat Code Coverage" \
  --results-directory TestResults \
  -- \
  DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover
```

Проверка coverage gate:

```bash
.DevOps/Scripts/check_coverage.sh TestResults 70
```

В CI нужно публиковать `TestResults` как artifact, чтобы после падения pipeline можно было скачать `.trx` и coverage reports.

## Sonar Analysis

Полноценный SonarQube/SonarCloud анализ запускается через SonarScanner for .NET:

```bash
dotnet tool restore

dotnet sonarscanner begin \
  /k:"<project-key>" \
  /d:sonar.host.url="<sonar-url>" \
  /d:sonar.token="<sonar-token>" \
  /d:sonar.cs.opencover.reportsPaths="TestResults/**/coverage.opencover.xml"

dotnet build -c Release --no-restore

dotnet sonarscanner end \
  /d:sonar.token="<sonar-token>"
```

Токен Sonar должен храниться в secret store CI/CD, а не в репозитории.

## Manual Build

Ручной flow полезен для проверки release-кандидата без CI/CD:

```bash
dotnet tool restore
dotnet restore
gitleaks dir --redact --config .gitleaks.toml .
dotnet list package --vulnerable --include-transitive
dotnet build -c Release --no-restore
dotnet test -c Release --no-restore --collect:"XPlat Code Coverage" --results-directory TestResults -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover
.DevOps/Scripts/check_coverage.sh TestResults 70
dotnet publish src/SALT.WebApi.Template/SALT.WebApi.Template.csproj -c Release -o publish --no-restore
```

После publish можно упаковать каталог:

```bash
tar -czf service.tar.gz -C publish .
```

## Scripted Build

Для локальной проверки перед merge request используйте:

```bash
.DevOps/Scripts/check_before_mr.sh
```

Windows PowerShell:

```powershell
.\.DevOps\Scripts\check_before_mr.ps1
```

Для сборки publish artifact используйте publish-скрипты.

Linux:

```bash
bash .DevOps/Scripts/publish_for_centos.sh
```

Windows:

```powershell
.\.DevOps\Scripts\publish_for_win.ps1
```

Оба publish-скрипта повторяют базовый CI/CD flow и останавливаются на первой ошибке. Если тестовые проекты не найдены, тесты падают, coverage reports не созданы или coverage ниже порога, артефакт не создается.

## GitLab CI/CD

В шаблоне есть готовый GitLab pipeline: `.gitlab-ci.yml`.

Он использует stages:

```text
restore -> security -> quality -> test -> sonar -> publish -> deploy
```

GitLab-specific проверки:

- `secret-scan` запускает Gitleaks в Docker image `zricethezav/gitleaks`;
- `semgrep-sast` запускается при `RUN_SEMGREP=true`;
- `owasp-dependency-check` запускается при `RUN_OWASP_DEPENDENCY_CHECK=true`;
- `dependency-audit` выполняет NuGet audit commands;
- `test-and-coverage` запускает Docker-in-Docker для Testcontainers;
- `sonar` запускается только если заданы `SONAR_HOST_URL` и `SONAR_TOKEN`;
- `publish` сохраняет tar.gz artifact;
- `deploy` ручной и доступен только из ветки `master`.

Рекомендуемые GitLab variables:

```text
SONAR_HOST_URL
SONAR_TOKEN
COVERAGE_LINE_THRESHOLD
RUN_SEMGREP
RUN_OWASP_DEPENDENCY_CHECK
```

Для production deployment настройте protected environment в GitLab и разрешите deploy только Maintainers. Сам `.gitlab-ci.yml` уже содержит manual deploy из `master`, но ограничение по роли настраивается в GitLab UI: `Settings -> CI/CD -> Protected environments`.

Deployment job в шаблоне намеренно содержит placeholder. Замените его на команду вашей платформы:

- `kubectl apply` или `helm upgrade`;
- push/pull Docker image;
- copy artifact на сервер;
- systemd/Windows Service rollout;
- вызов корпоративного deployment API.

## GitHub Actions

В шаблоне нет готового GitHub workflow-файла, но flow переносится напрямую в `.github/workflows/ci.yml`.

Рекомендуемые jobs:

```text
restore
security
quality
test
sonar
publish
deploy
```

GitHub-specific настройки:

- хранить токены в `Settings -> Secrets and variables -> Actions`;
- использовать `permissions` с минимальными правами;
- публиковать test/coverage artifacts через `actions/upload-artifact`;
- запускать Testcontainers на `ubuntu-latest`, где доступен Docker;
- ограничить production deploy через GitHub Environments и Required reviewers;
- запускать production deploy только из `master`.

Минимальный контур workflow:

```yaml
name: ci

on:
  pull_request:
  push:
    branches:
      - master

jobs:
  build-test-publish:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v7
      - uses: actions/setup-dotnet@v5
        with:
          dotnet-version: '10.0.x'

      - name: Restore tools
        run: dotnet tool restore

      - name: Restore
        run: dotnet restore

      - name: Secret scan
        uses: gitleaks/gitleaks-action@v3
        env:
          GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}

      - name: Dependency audit
        run: dotnet list package --vulnerable --include-transitive

      - name: Build
        run: dotnet build -c Release --no-restore

      - name: Test with coverage
        run: >
          dotnet test
          -c Release
          --no-restore
          --logger "trx"
          --collect:"XPlat Code Coverage"
          --results-directory TestResults
          --
          DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover

      - name: Coverage gate
        run: .DevOps/Scripts/check_coverage.sh TestResults 70

      - name: Publish
        run: dotnet publish src/SALT.WebApi.Template/SALT.WebApi.Template.csproj -c Release -o publish --no-restore

      - name: Upload artifacts
        uses: actions/upload-artifact@v7
        with:
          name: publish
          path: publish/
```

Production deploy лучше вынести в отдельный job:

- `needs: build-test-publish`;
- `if: github.ref == 'refs/heads/master'`;
- `environment: production`;
- required reviewers в настройках GitHub Environment.

## Deployment Observability

Pipeline или deployment manifest должен передавать runtime-настройки observability через переменные среды или secret/config store.

Минимальный набор:

```bash
OTEL_SERVICE_NAME=<service-name>
OTEL_EXPORTER_OTLP_ENDPOINT=http://otel-collector:4317
OTEL_RESOURCE_ATTRIBUTES=deployment.environment=<environment>,service.namespace=<namespace>,service.version=<version>
```

Эксплуатационная модель observability описана в [Operations](operations.md).

## Windows Service

Для регистрации опубликованного Windows build как Windows Service используйте:

```powershell
.\RegisterWinService.ps1
```

Скрипт нужно запускать из каталога опубликованного приложения, где находится `<ServiceName>.exe`.

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
