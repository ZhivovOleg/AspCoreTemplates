using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace SALT.WebApi.Template.AppCore;

/// <summary>
/// Дополнения к healthCheck'ам
/// </summary>
public class HealthCheckHelpers
{
    /// <summary>
    /// Вывод результата в виде json
    /// </summary>
    public static Task WriteJsonResponse(HttpContext context, HealthReport healthReport)
    {
        context.Response.ContentType = "application/json; charset=utf-8";
        JsonWriterOptions options = new() { Indented = true };
        using (MemoryStream ms = new())
        {
            using (Utf8JsonWriter jsonWriter = new(ms, options))
            {
                jsonWriter.WriteStartObject();
                jsonWriter.WriteString("status", healthReport.Status.ToString());
                jsonWriter.WriteStartObject("results");

                foreach (KeyValuePair<string, HealthReportEntry> hcr in healthReport.Entries)
                {
                    jsonWriter.WriteStartObject(hcr.Key);
                    jsonWriter.WriteString("status", hcr.Value.Status.ToString());
                    jsonWriter.WriteString("description", hcr.Value.Description);
                    jsonWriter.WriteStartObject("data");

                    foreach (KeyValuePair<string, object> item in hcr.Value.Data)
                    {
                        jsonWriter.WritePropertyName(item.Key);
                        JsonSerializer.Serialize(jsonWriter, item.Value, item.Value?.GetType() ?? typeof(object));
                    }

                    jsonWriter.WriteEndObject();
                    jsonWriter.WriteEndObject();
                }

                jsonWriter.WriteEndObject();
                jsonWriter.WriteEndObject();
            }

            return context.Response.WriteAsync(Encoding.UTF8.GetString(ms.ToArray()));
        }
    }
}