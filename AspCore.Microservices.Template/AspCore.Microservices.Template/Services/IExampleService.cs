using System;
using System.Threading.Tasks;
using AspCore.Microservices.Template.Dto;

namespace AspCore.Microservices.Template.Services;

/// <summary>
/// Example service interface
/// </summary>
public interface IExampleService
{
    /// <summary>
    /// Fare method
    /// </summary>
    Task<ExampleResponseDto> GetData(int id, Guid metaId);
}