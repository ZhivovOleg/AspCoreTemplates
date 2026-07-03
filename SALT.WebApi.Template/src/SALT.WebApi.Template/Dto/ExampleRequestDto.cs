using System.Text.Json.Serialization;

namespace SALT.WebApi.Template.Dto;

/// <summary>
/// Example request DTO
/// </summary>
public sealed record ExampleRequestDto
{
    /// <summary>
    /// ID
    /// </summary>
    [JsonRequired]
    public int Id { get; init; }
}
