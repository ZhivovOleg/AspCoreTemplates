using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SALT.WebApi.Template.Data;
using SALT.WebApi.Template.Data.Models;
using SALT.WebApi.Template.Dto;
using SALT.WebApi.Template.Dto.AppSettings;

namespace SALT.WebApi.Template.Services;

/// <summary>
/// Example service implementation
/// </summary>
/// <remarks>
/// DI ctor
/// </remarks>
internal sealed partial class ExampleDatabaseService(IOptions<ApplicationSettings> appSettings, ILogger<ExampleDatabaseService> logger, SharedDbContext sharedDbContext) : IExampleDatabaseService
{
    private readonly ILogger<ExampleDatabaseService> _logger = logger;
    private readonly ApplicationSettings _appSettings = appSettings.Value;
    private readonly SharedDbContext _sharedDbContext = sharedDbContext;

    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Error,
        Message = "Failed to load example model {ExampleId} from shared database")]
    private static partial void LogGetModelFailed(ILogger logger, int exampleId, Exception exception);

    /// <summary>
    /// Fake method implementation
    /// </summary>
    public async Task<ExampleResponseDto> GetData(int id)
    {
        try
        {
            ExampleModel model = await _sharedDbContext.ExampleModels
                .SingleOrDefaultAsync(m => m.Id == id)
                .ConfigureAwait(false);
            return model == null
                ? new ExampleResponseDto { Id = 0, Result = $"no model in db {_appSettings.ConnectionStrings["SharedDb"]}" }
                : new ExampleResponseDto { Id = model.Id, Result = model.Bytes };
        }
        catch (Exception exc)
        {
            LogGetModelFailed(_logger, id, exc);
            throw;
        }
    }

}