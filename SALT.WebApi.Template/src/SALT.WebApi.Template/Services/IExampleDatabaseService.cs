using System.Threading.Tasks;
using SALT.WebApi.Template.Dto;

namespace SALT.WebApi.Template.Services;

/// <summary>
/// Example service interface
/// </summary>
public interface IExampleDatabaseService
{
    /// <summary>
    /// Fare method
    /// </summary>
    Task<ExampleResponseDto> GetData(int id);
}