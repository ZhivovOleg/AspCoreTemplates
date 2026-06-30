using System.Globalization;

namespace SALT.WebApi.Template.Services;

/// <inheritdoc />
public class ExampleLogicService : IExampleLogicService
{
    /// <inheritdoc />
    public double SumAnyNumericValues(object a, object b)
    {
        double ad = TryParseDouble(a);
        double bd = TryParseDouble(b);

        return (ad, bd) switch
        {
            (double.NaN, double.NaN) => 0,
            (double.NaN, _) => bd,
            (_, double.NaN) => ad,
            _ => ad + bd,
        };
    }

    private static double TryParseDouble(object val) =>
        val switch
        {
            byte value => value,
            sbyte value => value,
            short value => value,
            ushort value => value,
            int value => value,
            uint value => value,
            long value => value,
            ulong value => value,
            float value => value,
            double value => value,
            decimal value => (double)value,
            _ => double.TryParse(val?.ToString(), CultureInfo.InvariantCulture, out double res)
                ? res
                : double.NaN,
        };
}
