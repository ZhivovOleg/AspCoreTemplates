using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SALT.WebApi.Template.Data;
using SALT.WebApi.Template.Data.Models;
using SALT.WebApi.Template.Dto;
using Testcontainers.PostgreSql;

namespace SALT.WebApi.Template.IntegrationTests;

/// <summary>
/// Схема с Testcontainers такая:
/// - Тест создает временный PostgreSQL в Docker.
/// - Получает у контейнера connection string.
/// - Запускает твой Web API через WebApplicationFactory.
/// - Подменяет ConnectionStrings:SharedDb на connection string контейнера.
/// - Создает схему/seed data.
/// - Делает обычный HTTP-запрос к приложению.
/// </summary>

public class ExampleApiTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:17-alpine")
    .WithDatabase("integration_tests")
    .WithUsername("postgres")
    .WithPassword("postgres")
    .Build();

    private WebApplicationFactory<Program> _factory = null!;

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            _ = builder.ConfigureAppConfiguration((_, configuration) =>
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:SharedDb"] = _postgres.GetConnectionString()
                })));

        using IServiceScope scope = _factory.Services.CreateScope();
        SharedDbContext dbContext = scope.ServiceProvider.GetRequiredService<SharedDbContext>();

        _ = await dbContext.Database.EnsureCreatedAsync();

        _ = dbContext.ExampleModels.Add(new ExampleModel
        {
            Name = "test",
            Bytes = "hello from postgres"
        });

        _ = await dbContext.SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        await _factory.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    [Fact]
    public async Task ControllerGetExampleData_ReturnsDataFromPostgres()
    {
        using HttpClient client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("api-version", "1.0");

        using HttpResponseMessage response = await client.GetAsync("/api/example/1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        string body = await response.Content.ReadAsStringAsync();
        Assert.Contains("hello from postgres", body);
    }

    [Fact]
    public async Task ControllerPostExampleData_ReturnsPhantomProcessingResult()
    {
        using HttpClient client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("api-version", "1.0");

        using HttpResponseMessage response = await client.PostAsJsonAsync(
            "/api/example",
            new ExampleRequestDto { Id = 1 });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        ExampleResponseDto? result = await response.Content.ReadFromJsonAsync<ExampleResponseDto>();

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Example request was processed. No data was persisted.", result.Result);
    }

    [Fact]
    public async Task MinimalGetExampleData_ReturnsDataFromPostgres()
    {
        using HttpClient client = _factory.CreateClient();

        using HttpResponseMessage response = await client.GetAsync("/api/v1/minimal-example/1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        string body = await response.Content.ReadAsStringAsync();
        Assert.Contains("hello from postgres", body);
    }

    [Fact]
    public async Task MinimalPostExampleData_ReturnsPhantomProcessingResult()
    {
        using HttpClient client = _factory.CreateClient();

        using HttpResponseMessage response = await client.PostAsJsonAsync(
            "/api/v1/minimal-example",
            new ExampleRequestDto { Id = 1 });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        ExampleResponseDto? result = await response.Content.ReadFromJsonAsync<ExampleResponseDto>();

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Example request was processed. No data was persisted.", result.Result);
    }

    [Theory]
    [InlineData("/api/example/0")]
    [InlineData("/api/v1/minimal-example/0")]
    public async Task GetExampleData_WithInvalidId_ReturnsUnprocessableEntity(string url)
    {
        using HttpClient client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("api-version", "1.0");

        using HttpResponseMessage response = await client.GetAsync(url);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }
}