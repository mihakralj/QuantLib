using System.Drawing;
using TradingPlatform.BusinessLayer;

namespace QuanTAlib;

public class MaafIndicator : Indicator, IWatchlistIndicator
{
    [InputParameter("Period", sortIndex: 1, 3, 1000, 1, 0)]
    public int Period { get; set; } = 10;

    [InputParameter("Threshold", sortIndex: 2, 0.0001, 0.1, 0.0001, 4)]
    public double Threshold { get; set; } = 0.002;

    [IndicatorExtensions.DataSourceInput]
    public SourceType Source { get; set; } = SourceType.Close;

    [InputParameter("Show cold values", sortIndex: 21)]
    public bool ShowColdValues { get; set; } = true;

    private Maaf? ma;
    protected LineSeries? Series;
    protected string? SourceName;
    public int MinHistoryDepths => Period;
    int IWatchlistIndicator.MinHistoryDepths => MinHistoryDepths;

    public override string ShortName => $"MAAF {Period}:{Threshold}:{SourceName}";

    public MaafIndicator()
    {
        OnBackGround = true;
        SeparateWindow = false;
        SourceName = Source.ToString();
        Name = "MAAF - Median Adaptive Averaging Filter";
        Description = "Median Adaptive Averaging Filter (Note: This indicator may have consistency issues)";
        Series = new(name: $"MAAF {Period}", color: IndicatorExtensions.Averages, width: 2, style: LineStyle.Solid);
        AddLineSeries(Series);
    }

    protected override void OnInit()
    {
        ma = new Maaf(Period, Threshold);
        SourceName = Source.ToString();
        base.OnInit();
    }

    protected override void OnUpdate(UpdateArgs args)
    {
        TValue input = this.GetInputValue(args, Source);
        TValue result = ma!.Calc(input);

        Series!.SetValue(result.Value);
        Series!.SetMarker(0, Color.Transparent); //OnPaintChart draws the line, hidden here
    }

    public override void OnPaintChart(PaintChartEventArgs args)
    {
        base.OnPaintChart(args);
        this.PaintSmoothCurve(args, Series!, ma!.WarmupPeriod, showColdValues: ShowColdValues, tension: 0.2);
    }
}
