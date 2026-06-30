using System.Globalization;
using SALT.WebApi.Template.Services;

namespace SALT.WebApi.Template.UnitTests.Services;

/// <summary>
/// Unit tests for <see cref="ExampleLogicService"/>.
/// </summary>
public class ExampleLogicServiceTests
{
    private readonly ExampleLogicService _service = new();

    [Fact]
    public void SumAnyNumericValues_ReturnsSum_WhenBothValuesAreIntegers()
    {
        double result = _service.SumAnyNumericValues(2, 3);

        Assert.Equal(5, result);
    }

    [Theory]
    [InlineData(1, 2, 3)]
    [InlineData(-1, 1, 0)]
    [InlineData(1.5, 2.25, 3.75)]
    [InlineData("10", "15", 25)]
    [InlineData("10.5", "0.5", 11)]
    public void SumAnyNumericValues_ReturnsSum_WhenBothValuesAreNumeric(object first, object second, double expected)
    {
        double result = _service.SumAnyNumericValues(first, second);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("not-a-number", 5, 5)]
    [InlineData(5, "not-a-number", 5)]
    [InlineData(null, 5, 5)]
    [InlineData(5, null, 5)]
    public void SumAnyNumericValues_IgnoresValue_WhenOnlyOneValueIsNotNumeric(object? first, object? second, double expected)
    {
        double result = _service.SumAnyNumericValues(first, second);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("not-a-number", "also-not-a-number")]
    [InlineData(null, null)]
    [InlineData("", " ")]
    public void SumAnyNumericValues_ReturnsZero_WhenBothValuesAreNotNumeric(object? first, object? second)
    {
        double result = _service.SumAnyNumericValues(first, second);

        Assert.Equal(0, result);
    }

    [Fact]
    public void SumAnyNumericValues_UsesInvariantCulture_WhenParsingDecimalStrings()
    {
        CultureInfo originalCulture = CultureInfo.CurrentCulture;

        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ru-RU");

            double result = _service.SumAnyNumericValues("1.5", "2.5");

            Assert.Equal(4, result);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }
}
