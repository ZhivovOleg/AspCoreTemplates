using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AspCore.Microservices.Template.Data;
using AspCore.Microservices.Template.Data.Models;
using AspCore.Microservices.Template.Dto;
using AspCore.Microservices.Template.Dto.AppSettings;

namespace AspCore.Microservices.Template.Services
{
	/// <summary>
	/// Example service implementation
	/// </summary>
	public class ExampleService : IExampleService
	{
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
		public async Task<ExampleResponseDto> GetData(int id, Guid guid)
		{
			try
			{
				ExampleModel model = await _sharedDbContext.ExampleModels.SingleOrDefaultAsync(m => m.Name == guid.ToString());
				return model == null
					? new ExampleResponseDto {Id = 0, Result = $"no model in db {_appSettings.Connections["SharedDb"]}"}
					: new ExampleResponseDto {Id = model.Id, Result = model.Bytes};
			}
			catch (Exception exc)
			{
				_logger.LogError("Error on get model", exc);
				throw;
			}
		}
	}
}