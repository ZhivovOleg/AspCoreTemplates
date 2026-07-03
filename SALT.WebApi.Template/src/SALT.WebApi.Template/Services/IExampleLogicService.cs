namespace SALT.WebApi.Template.Services;

/// <summary>
/// Example logic service.
/// </summary>
public interface IExampleLogicService
{
    /// <summary>
    /// Sum any values of any numeric types if possible.
    /// </summary>
    double SumAnyNumericValues(object? a, object? b);
}
