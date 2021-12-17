using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AspCore.Microservices.Template.Data;
using AspCore.Microservices.Template.Data.Models;
using AspCore.Microservices.Template.Dto;
using AspCore.Microservices.Template.Dto.AppSettings;

namespace AspCore.Microservices.Template.Services;

/// <summary>
/// Example service implementation
/// </summary>
public class ExampleService : IExampleService
{
    private static readonly Action<ILogger, Exception> _errorOnGetModel = LoggerMessage.Define(
        LogLevel.Error,
        new EventId(1, nameof(ExampleService)),
        "Error on get model");

    private readonly ILogger<ExampleService> _logger;
    private readonly AppSettings _appSettings;
    private readonly SharedDbContext _sharedDbContext;

    /// <summary>
    /// DI ctor
    /// </summary>
    public ExampleService(IOptions<AppSettings> appSettings, ILogger<ExampleService> logger, SharedDbContext sharedDbContext)
    {
        _logger = logger;
        _sharedDbContext = sharedDbContext;
        _appSettings = appSettings.Value;
    }

    /// <summary>
    /// Fake method implementation
    /// </summary>
    public async Task<ExampleResponseDto> GetData(int id, Guid metaId)
    {
        try
        {
            ExampleModel model = await _sharedDbContext.ExampleModels.SingleOrDefaultAsync(m => m.Name == metaId.ToString());
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