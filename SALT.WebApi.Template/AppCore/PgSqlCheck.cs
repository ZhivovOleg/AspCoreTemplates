using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Npgsql;

namespace SALT.WebApi.Template.AppCore;

/// <summary>
/// Кастомная проверка доступности БД postgres
/// </summary>
public class PgSqlCheck : IHealthCheck
{
    private readonly string _connString;

    /// <summary>
    /// DI ctor
    /// </summary>
    public PgSqlCheck(string connString) => _connString = connString;

    /// <inheritdoc/>
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await using NpgsqlConnection conn = new(_connString);
            {
                await using (NpgsqlCommand cmd = new("SELECT version()", conn))
                {
                    Stopwatch sw = new();
                    sw.Start();
                    await conn.OpenAsync(cancellationToken);
                    string result = (await cmd.ExecuteScalarAsync(cancellationToken)).ToString();
                    sw.Stop();
                    return new HealthCheckResult(
                        HealthStatus.Healthy,
                        $"Version: {result}; Connection and request timeout: {sw.ElapsedMilliseconds}ms",
                        null);
                }
            }
        }
        catch (Exception exc)
        {
            return new HealthCheckResult(HealthStatus.Unhealthy, exc.Message, exc);
        }
    }
}
