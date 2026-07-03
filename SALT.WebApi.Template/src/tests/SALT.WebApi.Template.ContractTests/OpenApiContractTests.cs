using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace SALT.WebApi.Template.ContractTests;

public class OpenApiContractTests
{
    private const string _updateSnapshotVariable = "UPDATE_OPENAPI_SNAPSHOT";

    private static readonly JsonSerializerOptions _snapshotJsonOptions = new()
    {
        WriteIndented = true
    };

    [Fact]
    public async Task OpenApiV1_ShouldMatchSnapshot()
    {
        await using WebApplicationFactory<Program> factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder => builder.UseEnvironment("Development"));

        using HttpClient client = factory.CreateClient();

        string actualJson = await client.GetStringAsync("/openapi/v1.json");
        string actual = NormalizeJson(actualJson);

        string snapshotPath = GetSnapshotPath();

        if (ShouldUpdateSnapshot())
        {
            _ = Directory.CreateDirectory(Path.GetDirectoryName(snapshotPath)!);
            await File.WriteAllTextAsync(snapshotPath, actual);
            return;
        }

        string expected = await File.ReadAllTextAsync(snapshotPath);

        Assert.Equal(expected, actual);
    }

    private static bool ShouldUpdateSnapshot() =>
        string.Equals(
            Environment.GetEnvironmentVariable(_updateSnapshotVariable),
            "true",
            StringComparison.OrdinalIgnoreCase);

    private static string NormalizeJson(string json)
    {
        using JsonDocument document = JsonDocument.Parse(json);

        return JsonSerializer.Serialize(
            document.RootElement,
            _snapshotJsonOptions);
    }

    private static string GetSnapshotPath()
    {
        string projectDirectory = GetProjectDirectory();

        return Path.Combine(
            projectDirectory,
            "Contracts",
            "Snapshots",
            "openapi.v1.json");
    }

    private static string GetProjectDirectory()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);

        while (directory is not null)
        {
            string projectFile = Path.Combine(
                directory.FullName,
                "SALT.WebApi.Template.ContractTests.csproj");

            if (File.Exists(projectFile))
                return directory.FullName;

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Unit test project directory was not found.");
    }
}
