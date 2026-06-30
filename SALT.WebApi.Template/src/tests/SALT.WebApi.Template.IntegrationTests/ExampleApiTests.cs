using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SALT.WebApi.Template.Data;
using SALT.WebApi.Template.Data.Models;
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
    public async Task GetExampleData_ReturnsDataFromPostgres()
    {
        using HttpClient client = _factory.CreateClient();

        using HttpResponseMessage response = await client.GetAsync("/api/v1/example/1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        string body = await response.Content.ReadAsStringAsync();
        Assert.Contains("hello from postgres", body);
    }
}
