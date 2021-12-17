using System;

namespace AspCore.Microservices.Template.Dto;

/// <summary>
/// Example request DTO
/// </summary>
public class ExampleRequestDto
{
    /// <summary>
    /// ID
    /// </summary>
    public int Id { get; set; }
    /// <summary>
    /// MetaId
    /// </summary>
    public Guid MetaId { get; set; }
}