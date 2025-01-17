using System.Drawing;
using TradingPlatform.BusinessLayer;

namespace QuanTAlib;

public class SmaIndicator : Indicator, IWatchlistIndicator
{
    [InputParameter("Period", sortIndex: 1, 1, 1000, 1, 0)]
    public int Period { get; set; } = 14;

    [IndicatorExtensions.DataSourceInput]
    public SourceType Source { get; set; } = SourceType.Close;

    [InputParameter("Show cold values", sortIndex: 21)]
    public bool ShowColdValues { get; set; } = true;

    private Sma? ma;
    private Mape? error;
    protected LineSeries? Series;
    protected string? SourceName;
    public int MinHistoryDepths => Period;
    int IWatchlistIndicator.MinHistoryDepths => MinHistoryDepths;

    public SmaIndicator()
    {
        OnBackGround = true;
        SeparateWindow = false;
        SourceName = Source.ToString();
        Name = "SMA - Simple Moving Average";
        Description = "Simple Moving Average";
        Series = new(name: $"SMA {Period}", color: IndicatorExtensions.Averages, width: 2, style: LineStyle.Solid);
        AddLineSeries(Series);
    }

    protected override void OnInit()
    {
        ma = new Sma(Period);
        error = new(Period);
        SourceName = Source.ToString();
        base.OnInit();
    }

    protected override void OnUpdate(UpdateArgs args)
    {
        TValue input = this.GetInputValue(args, Source);
        TValue result = ma!.Calc(input);
        error!.Calc(input, result);

        Series!.SetMarker(0, Color.Transparent); //OnPaintChart draws the line, hidden here
        Series!.SetValue(result.Value);
    }

    public override string ShortName => $"SMA {Period}:{SourceName}";

    public override void OnPaintChart(PaintChartEventArgs args)
    {
        base.OnPaintChart(args);
        this.PaintSmoothCurve(args, Series!, ma!.WarmupPeriod, showColdValues: ShowColdValues, tension: 0.2);
    }
}
