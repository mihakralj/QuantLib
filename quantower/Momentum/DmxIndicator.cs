using System.Drawing;
using TradingPlatform.BusinessLayer;

namespace QuanTAlib;

public class DmxIndicator : Indicator, IWatchlistIndicator
{
    [InputParameter("DMI Periods", sortIndex: 1, 1, 2000, 1, 0)]
    public int DmiPeriods { get; set; } = 14;

    [InputParameter("JMA Smoothing Periods", sortIndex: 2, 1, 2000, 1, 0)]
    public int JmaPeriods { get; set; } = 12;

    [InputParameter("JMA Phase", sortIndex: 3, -100, 100, 1, 0)]
    public int JmaPhase { get; set; } = 100;

    [InputParameter("JMA Factor", sortIndex: 4, 0.01, 1, 0.01, 2)]
    public double JmaFactor { get; set; } = 0.3;

    [InputParameter("Show cold values", sortIndex: 21)]
    public bool ShowColdValues { get; set; } = true;

    private Dmx? dmx;
    protected LineSeries? PlusDiSeries;
    protected LineSeries? MinusDiSeries;
    public int MinHistoryDepths => Math.Max(5, (DmiPeriods + JmaPeriods) * 2);
    int IWatchlistIndicator.MinHistoryDepths => MinHistoryDepths;

    public DmxIndicator()
    {
        Name = "DMX - Enhanced Directional Movement Index";
        Description = "An enhanced version of DMI using JMA smoothing for better noise reduction and responsiveness.";
        SeparateWindow = true;

        PlusDiSeries = new($"+DI {DmiPeriods}", color: Color.Red, 2, LineStyle.Solid);
        MinusDiSeries = new($"-DI {DmiPeriods}", color: Color.Blue, 2, LineStyle.Solid);
        AddLineSeries(PlusDiSeries);
        AddLineSeries(MinusDiSeries);
    }

    protected override void OnInit()
    {
        dmx = new Dmx(DmiPeriods, JmaPeriods, JmaPhase, JmaFactor);
        base.OnInit();
    }

    protected override void OnUpdate(UpdateArgs args)
    {
        TBar input = IndicatorExtensions.GetInputBar(this, args);
        var result = dmx!.Calc(input);

        PlusDiSeries!.SetValue(dmx.PlusDI);
        MinusDiSeries!.SetValue(dmx.MinusDI);
        PlusDiSeries!.SetMarker(0, Color.Transparent);
        MinusDiSeries!.SetMarker(0, Color.Transparent);
    }

#pragma warning disable CA1416 // Validate platform compatibility

    public override string ShortName => $"DMX ({DmiPeriods})";

    public override void OnPaintChart(PaintChartEventArgs args)
    {
        base.OnPaintChart(args);
        this.PaintSmoothCurve(args, PlusDiSeries!, dmx!.WarmupPeriod, showColdValues: ShowColdValues, tension: 0.2);
        this.PaintSmoothCurve(args, MinusDiSeries!, dmx!.WarmupPeriod, showColdValues: ShowColdValues, tension: 0.2);
    }
}