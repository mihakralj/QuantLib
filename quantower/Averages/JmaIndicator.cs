using System.Drawing;
using TradingPlatform.BusinessLayer;

namespace QuanTAlib;

public class JmaIndicator : Indicator, IWatchlistIndicator
{
    [InputParameter("Periods", sortIndex: 1, 1, 1000, 1, 0)]
    public int Periods { get; set; } = 14;

    [InputParameter("Phase", sortIndex: 2, -100, 100, 1, 0)]
    public double Phase { get; set; } = 0;

    [InputParameter("VShort", sortIndex: 3, 1, 100, 1, 0)]
    public int VShort { get; set; } = 10;

    [InputParameter("Data source", sortIndex: 4, variants: [
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

    private Jma? ma;
    protected LineSeries? Series;
    protected string? SourceName;
    public int MinHistoryDepths => Periods * 2;
    int IWatchlistIndicator.MinHistoryDepths => MinHistoryDepths;

    public JmaIndicator()
    {
        OnBackGround = true;
        SeparateWindow = false;
        SourceName = Source.ToString();
        Name = "JMA - Jurik Moving Average";
        Description = "Jurik Moving Average (Note: This indicator may have consistency issues)";
        Series = new(name: $"JMA {Periods}", color: Color.Yellow, width: 2, style: LineStyle.Solid);
        AddLineSeries(Series);
    }

    protected override void OnInit()
    {
        ma = new Jma(Periods, Phase, VShort);
        SourceName = Source.ToString();
        base.OnInit();
    }

    protected override void OnUpdate(UpdateArgs args)
    {
        TValue input = this.GetInputValue(args, Source);
        TValue result = ma!.Calc(input);

        Series!.SetValue(result.Value);
    }

    public override string ShortName => $"JMA {Periods}:{Phase}:{VShort}:{SourceName}";
}
