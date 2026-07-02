# Operations

Health checks говорят "можно ли слать трафик".
Observability говорит "что происходит внутри".

В проекте внедрен observability как стандартный набор сигналов:

- Logs: что произошло.
- Metrics: насколько часто, быстро и тяжело работает сервис.
- Traces: где конкретный запрос провел время между сервисами, БД, HTTP-клиентами.

## Observability

Сервис использует OpenTelemetry как vendor-neutral слой для traces, metrics и, при необходимости, logs.

Рекомендуемый production-путь: экспортировать telemetry по OTLP в OpenTelemetry Collector или совместимый агент, а уже из него отправлять данные в Grafana/Tempo/Prometheus/Loki, Azure Monitor, Datadog, Elastic или другую платформу.

Минимальные переменные окружения для deployment:

```bash
OTEL_SERVICE_NAME=<service-name>
OTEL_EXPORTER_OTLP_ENDPOINT=http://otel-collector:4317
OTEL_RESOURCE_ATTRIBUTES=deployment.environment=<environment>,service.namespace=<namespace>,service.version=<version>
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

## Logs

Логи пишутся в stdout/stderr в structured формате. Дальше их забирает инфраструктурный агент: Promtail, Grafana Alloy, Fluent Bit, Vector или аналогичный компонент.

Ожидаемая эксплуатационная схема:

```text
App console logs -> node/sidecar agent -> Loki / Elastic / другой log backend
```

В логах нельзя хранить пароли, токены, connection strings, cookie/session values и персональные данные.

## Metrics

Метрики можно отдавать двумя способами:

- через OTLP exporter в OpenTelemetry Collector;
- через `/metrics`, если включен Prometheus exporter.

Endpoint `/metrics` не должен быть публичным. Его нужно ограничивать сетевой политикой, ingress rules, service mesh policy или отдельной internal-only схемой публикации.

## Traces

Traces отправляются по OTLP в collector или совместимый backend. Они помогают видеть весь путь запроса: ASP.NET Core pipeline, исходящие HTTP-вызовы, PostgreSQL и другие зависимости.

В span names, attributes и baggage нельзя добавлять секреты и чувствительные пользовательские данные.

## Observability Schemes

### General

```text
App
 ├─ logs    -> console stdout/stderr -> Promtail/Alloy/Fluent Bit -> Loki
 ├─ metrics -> /metrics или OTLP -> Prometheus / OpenTelemetry Collector
 └─ traces  -> OTLP -> OpenTelemetry Collector -> Tempo / Jaeger / Zipkin / APM
```

### Grafana Stack

```text
App
 ├─ Console logs -> Grafana Alloy -> Loki
 ├─ OTLP metrics -> Grafana Alloy/Otel Collector -> Prometheus/Mimir
 └─ OTLP traces  -> Grafana Alloy/Otel Collector -> Tempo
```

### Prometheus Exporter

```text
App /metrics <- Prometheus scrape
App OTLP traces -> Collector/Alloy -> Tempo
App console logs -> Alloy/Promtail -> Loki
```

## Health Checks

Основной endpoint:

```text
/healthz
```

Health checks используются для readiness/liveness и автоматических проверок после rollout. Они не заменяют logs, metrics и traces.

## Runtime Checklist

После выкладки сервиса проверьте:

- `/healthz` возвращает healthy status;
- сервис пишет structured logs в stdout/stderr;
- в telemetry backend появились traces для входящих HTTP-запросов;
- runtime и ASP.NET Core metrics поступают в metrics backend;
- `OTEL_SERVICE_NAME`, `deployment.environment`, `service.namespace` и `service.version` заданы корректно;
- `/metrics` недоступен из публичной сети, если включен Prometheus exporter;
- в logs, metrics labels, traces attributes и baggage нет секретов и персональных данных.

Сборка, публикация artifact и platform-specific deploy описаны в [CI/CD](ci-cd.md).
