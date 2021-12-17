using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AspCore.Microservices.Template.Dto;
using AspCore.Microservices.Template.Services;
using Microsoft.AspNetCore.Http;

namespace AspCore.Microservices.Template.Controllers
{
	/// <summary>
	/// Example controller
	/// </summary>
	[ApiController]
	[ApiVersion("1.0")]
	[Route("api/v{version:apiVersion}/[controller]")]
	public class ExampleController : ControllerBase
	{
		private readonly IExampleService _exampleService;

		/// <summary>
		/// DI ctor
		/// </summary>
		public ExampleController(IExampleService exampleService) => 
			_exampleService = exampleService;

		/// <summary>
		/// Generate example value
		/// </summary>
		[HttpPost]
		[Produces("application/json")]
		[Route(nameof(GetExample))]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> GetExample(ExampleRequestDto requestDto)
		{
			ExampleResponseDto result = await _exampleService.GetData(requestDto.Id, requestDto.Guid);
			return Ok(result);
		}
	}
}