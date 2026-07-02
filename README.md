# SALT Microservice Templates

Modern `dotnet new` templates for production-ready ASP.NET Core microservices.

## Included Templates

- `saltwebapi` - an ASP.NET Core Web API microservice template for .NET 10.

The template includes:

- OpenAPI generation and Scalar API reference UI;
- API versioning;
- PostgreSQL and EF Core wiring;
- health checks;
- OpenTelemetry-based observability;
- structured logging examples;
- static analyzers and Sonar-friendly configuration;
- unit, contract, and integration test project layout;
- Testcontainers-ready integration tests;
- coverage collection and coverage gate scripts;
- Gitleaks-based secret scanning;
- GitLab CI/CD example;
- Dockerfiles and publish scripts.

## Install

From NuGet:

```bash
dotnet new install Salt.Microservices.Templates
```

From a local package:

```bash
dotnet new install ./Salt.Microservices.Templates.1.0.0.nupkg
```

## Create A Service

```bash
mkdir MyService
cd MyService
dotnet new saltwebapi
```

## Build This Template Package

```bash
dotnet pack --configuration Release
```

With an explicit package version:

```bash
dotnet pack --configuration Release -p:PackageVersion=2026.0702.1
```

## Publish

```bash
dotnet nuget push ./bin/Release/Salt.Microservices.Templates.<version>.nupkg \
  --source https://api.nuget.org/v3/index.json \
  --api-key <NUGET_API_KEY>
```

## Uninstall

```bash
dotnet new uninstall Salt.Microservices.Templates
```
