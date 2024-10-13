using System.Drawing;
using TradingPlatform.BusinessLayer;

namespace QuanTAlib;

public class PwmaIndicator : Indicator, IWatchlistIndicator
{
    [InputParameter("Periods", sortIndex: 1, 1, 1000, 1, 0)]
    public int Periods { get; set; } = 14;

    [InputParameter("Data source", sortIndex: 2, variants: [
        "Open", SourceType.Open,
        "High", SourceType.High,
        "Low", SourceType.Low,
        "Close", SourceType.Close,
        "HL/2 (Median)", SourceType.HL2,
        "OC/2 (Midpoint)", SourceType.OC2,
        "OHL/3 (Mean)", SourceType.OHL3,
        "HLC/3 (Typical)", SourceType.HLC3,
        "OHLC/4 (Average)", SourceType.OHLC4,
        "HLCC/4 (Weighted)", SourceType.HLCC4
    ])]
    public SourceType Source { get; set; } = SourceType.Close;

    private Pwma? ma;
    protected LineSeries? Series;
    protected string? SourceName;
    public int MinHistoryDepths => Periods;
    int IWatchlistIndicator.MinHistoryDepths => MinHistoryDepths;

    public PwmaIndicator()
    {
        OnBackGround = true;
        SeparateWindow = false;
        SourceName = Source.ToString();
        Name = "PWMA - Pascal's Weighted Moving Average";
        Description = "Pascal's Weighted Moving Average";
        Series = new(name: $"PWMA {Periods}", color: Color.Yellow, width: 2, style: LineStyle.Solid);
        AddLineSeries(Series);
    }

    protected override void OnInit()
    {
        ma = new Pwma(Periods);
        SourceName = Source.ToString();
        base.OnInit();
    }

    protected override void OnUpdate(UpdateArgs args)
    {
        TValue input = this.GetInputValue(args, Source);
        TValue result = ma!.Calc(input);

        Series!.SetValue(result.Value);
    }

    public override string ShortName => $"PWMA {Periods}:{SourceName}";
}
