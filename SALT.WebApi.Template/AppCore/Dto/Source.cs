namespace SALT.WebApi.Template.AppCore.Dto;

/// <summary>
/// Подробные сведения об ошибке
/// </summary>
public class Source
{
    /// <summary>
    /// Детальное объяснение возникшей проблемы
    /// </summary>
    public string Detail { get; set; }
    /// <summary>
    /// Метод или действие, которое привело к ошибке
    /// </summary>
    public string Action { get; set; }
    /// <summary>
    /// Набор параметров и их значений, вызвавшим ошибку в виде JSON
    /// </summary>
    public string Parameters { get; set; }
}
