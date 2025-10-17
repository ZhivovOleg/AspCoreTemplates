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
public class ExampleDatabaseService(IOptions<AppSettings> appSettings, ILogger<ExampleDatabaseService> logger, SharedDbContext sharedDbContext) : IExampleDatabaseService
{
    private static readonly Action<ILogger, Exception> _errorOnGetModel = LoggerMessage.Define(
        LogLevel.Error,
        new EventId(1, nameof(ExampleDatabaseService)),
        "Error on get model");

    private readonly ILogger<ExampleDatabaseService> _logger = logger;
    private readonly AppSettings _appSettings = appSettings.Value;
    private readonly SharedDbContext _sharedDbContext = sharedDbContext;

    /// <summary>
    /// Fake method implementation
    /// </summary>
    public async Task<ExampleResponseDto> GetData(int id)
    {
        try
        {
            ExampleModel model = await _sharedDbContext.ExampleModels.SingleOrDefaultAsync(m => m.Id == id);
            return model == null
                ? new ExampleResponseDto { Id = 0, Result = $"no model in db {_appSettings.Connections["SharedDb"]}" }
                : new ExampleResponseDto { Id = model.Id, Result = model.Bytes };
        }
        catch (Exception exc)
        {
            _errorOnGetModel(_logger, exc);
            throw;
        }
    }
}