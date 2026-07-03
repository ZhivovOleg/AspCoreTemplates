namespace SALT.WebApi.Template.Dto;

/// <summary>
/// Example response DTO
/// </summary>
public sealed record ExampleResponseDto
{
    /// <summary>
    /// ID
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// string result
    /// </summary>
    public required string Result { get; init; }
}
