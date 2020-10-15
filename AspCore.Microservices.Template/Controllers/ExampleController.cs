using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AspCore.Microservices.Template.Dto;
using AspCore.Microservices.Template.Services;

namespace AspCore.Microservices.Template.Controllers
{
	/// <summary>
	/// Example controller
	/// </summary>
	[ApiController]
	[Route(nameof(ExampleController))]
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
		public async Task<IActionResult> GetExample(ExampleRequestDto requestDto)
		{
			ExampleResponseDto result = await _exampleService.GetData(requestDto.Id, requestDto.Guid);
			return Ok(result);
		}
	}
}