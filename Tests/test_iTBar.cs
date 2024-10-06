using Xunit;
using System.Reflection;
using System.Diagnostics.CodeAnalysis;

namespace QuanTAlib;

[SuppressMessage("Security", "SCS0005:Weak random number generator.", Justification = "Acceptable for tests")]

public class BarIndicatorTests
{
    private readonly Random rnd;
    private const int SeriesLen = 1000;
    private const int Corrections = 100;

    public BarIndicatorTests()
    {
        rnd = new Random((int)DateTime.Now.Ticks);
    }

    private static readonly ITValue[] indicators = new ITValue[]
    {
            new Atr(period: 14),
    };

    [Theory]
    [MemberData(nameof(GetIndicators))]
    public void IndicatorIsNew(ITValue indicator)
    {
        var indicator1 = indicator;
        var indicator2 = indicator;

        MethodInfo calcMethod = indicator.GetType().GetMethod("Calc")!;
        if (calcMethod == null)
        {
            throw new InvalidOperationException($"Calc method not found for indicator type: {indicator.GetType().Name}");
        }

        for (int i = 0; i < SeriesLen; i++)
        {
            TBar item1 = new(Time: DateTime.Now, Open: rnd.Next(-100, 100), High: rnd.Next(-100, 100), Low: rnd.Next(-100, 100), Close: rnd.Next(-100, 100), Volume: rnd.Next(-1000, 1000), IsNew: true);
            calcMethod.Invoke(indicator1, new object[] { item1 });

            for (int j = 0; j < Corrections; j++)
            {
                item1 = new(Time: DateTime.Now, Open: rnd.Next(-100, 100), High: rnd.Next(-100, 100), Low: rnd.Next(-100, 100), Close: rnd.Next(-100, 100), Volume: rnd.Next(-1000, 1000), IsNew: false);
                calcMethod.Invoke(indicator1, new object[] { item1 });
            }

            var item2 = new TBar(item1.Time, item1.Open, item1.High, item1.Low, item1.Close, item1.Volume, IsNew: true);
            calcMethod.Invoke(indicator2, new object[] { item2 });

            Assert.Equal(indicator1.Value, indicator2.Value);
        }
    }

    public static IEnumerable<object[]> GetIndicators()
    {
        return indicators.Select(indicator => new object[] { indicator });
    }
}
