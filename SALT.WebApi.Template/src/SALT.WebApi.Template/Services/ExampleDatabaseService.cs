using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SALT.WebApi.Template.Data;
using SALT.WebApi.Template.Data.Models;
using SALT.WebApi.Template.Dto;

namespace SALT.WebApi.Template.Services;

/// <summary>
/// Example service implementation
/// </summary>
/// <remarks>
/// DI ctor
/// </remarks>
internal sealed partial class ExampleDatabaseService(ILogger<ExampleDatabaseService> logger, SharedDbContext sharedDbContext) : IExampleDatabaseService
{
    private readonly ILogger<ExampleDatabaseService> _logger = logger;
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
            ExampleModel? model = await _sharedDbContext.ExampleModels
                .SingleOrDefaultAsync(m => m.Id == id);
            return model == null
                ? new ExampleResponseDto { Id = 0, Result = "No model found in shared database." }
                : new ExampleResponseDto { Id = model.Id, Result = model.Bytes };
        }
        catch (Exception exc)
        {
            LogGetModelFailed(_logger, id, exc);
            throw;
        }
    }

}
