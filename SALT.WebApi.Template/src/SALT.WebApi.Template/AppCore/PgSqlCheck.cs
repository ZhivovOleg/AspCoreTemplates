using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Npgsql;

namespace SALT.WebApi.Template.AppCore;

/// <summary>
/// Кастомная проверка доступности БД postgres
/// </summary>
/// <remarks>
/// DI ctor
/// </remarks>
public class PgSqlCheck(string connString) : IHealthCheck
{
    private readonly string _connString = connString;

    /// <inheritdoc/>
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            NpgsqlConnection conn = new(_connString);
            await using (ConfiguredAsyncDisposable _ = conn.ConfigureAwait(false))
            {
                NpgsqlCommand cmd = new("SELECT version()", conn);
                await using (ConfiguredAsyncDisposable __ = cmd.ConfigureAwait(false))
                {
                    Stopwatch sw = new();
                    sw.Start();
                    await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
                    string result = (await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false)).ToString();
                    sw.Stop();
                    return new HealthCheckResult(
                        HealthStatus.Healthy,
                        $"Version: {result}; Connection and request timeout: {sw.ElapsedMilliseconds.ToString(CultureInfo.InvariantCulture)}ms",
                        exception: null);
                }
            }
        }
        catch (Exception exc)
        {
            return new HealthCheckResult(HealthStatus.Unhealthy, exc.Message, exc);
        }
    }
}
