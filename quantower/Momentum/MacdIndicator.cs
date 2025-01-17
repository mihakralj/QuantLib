using System.Diagnostics.Metrics;
using System.Drawing;
using System.Drawing.Drawing2D;
using TradingPlatform.BusinessLayer;

namespace QuanTAlib;

public class MacdIndicator : Indicator, IWatchlistIndicator
{
    [InputParameter("Slow EMA", sortIndex: 1, 1, 1000, 1, 0)]
    public int Slow { get; set; } = 26;

    [InputParameter("Fast EMA", sortIndex: 2, 1, 2000, 1, 0)]
    public int Fast { get; set; } = 12;

    [InputParameter("Signal line", sortIndex: 3, 1, 2000, 1, 0)]
    public int Signal { get; set; } = 9;

    [InputParameter("Use SMA for warmup period", sortIndex: 2)]
    public bool UseSMA { get; set; } = false;

    [IndicatorExtensions.DataSourceInput]
    public SourceType Source { get; set; } = SourceType.Close;

    [InputParameter("Show cold values", sortIndex: 21)]
    public bool ShowColdValues { get; set; } = true;

    private Macd? macd;
    private Slope? histSlope;
    protected LineSeries? MainSeries;
    protected LineSeries? SignalSeries;
    protected LineSeries? HistogramSeries;
    protected LineSeries? HistSlopeSeries;

    protected string? SourceName;
    public int MinHistoryDepths => Slow;
    int IWatchlistIndicator.MinHistoryDepths => MinHistoryDepths;

    public override string ShortName => $"MACD {Slow}:{Fast}:{Signal}";

    public MacdIndicator()
    {
        OnBackGround = true;
        SeparateWindow = true;
        SourceName = Source.ToString();
        Name = "MACD - Moving Average Convergence Divergence";
        Description = "MACD";
        MainSeries = new(name: $"MAIN", color: Color.RoyalBlue, width: 2, style: LineStyle.Solid);
        SignalSeries = new(name: $"SIGNAL", color: Color.Red, width: 2, style: LineStyle.Solid);
        HistogramSeries = new(name: $"HISTOGRAM", color: Color.White, width: 2, style: LineStyle.Solid);
        HistSlopeSeries = new(name: $"SLOPE", color: Color.Transparent, width: 2, style: LineStyle.Solid);
        HistSlopeSeries.Visible = false;

        AddLineSeries(MainSeries);
        AddLineSeries(SignalSeries);
        AddLineSeries(HistogramSeries);
        AddLineSeries(HistSlopeSeries);
    }

    protected override void OnInit()
    {
        macd = new(fastPeriod: Fast, slowPeriod: Slow, signalPeriod: Signal);
        histSlope = new(2);
        SourceName = Source.ToString();
        base.OnInit();
    }

    protected override void OnUpdate(UpdateArgs args)
    {
        TValue input = this.GetInputValue(args, Source);
        macd!.Calc(input);

        double main = macd.MacdLine;
        double signal = macd.SignalLine;
        double histogram = macd.Value;
        histSlope!.Calc(histogram);

        MainSeries!.SetValue(main);
        MainSeries!.SetMarker(0, Color.Transparent); //OnPaintChart draws the line, hidden here

        SignalSeries!.SetValue(signal);
        SignalSeries!.SetMarker(0, Color.Transparent); //OnPaintChart draws the line, hidden here

        HistogramSeries!.SetValue(histogram);
        HistogramSeries!.SetMarker(0, Color.Transparent); //OnPaintChart draws the line, hidden here

        HistSlopeSeries!.SetValue(histSlope.Value);
        HistSlopeSeries!.SetMarker(0, Color.Transparent); //OnPaintChart draws the line, hidden here
    }
#pragma warning disable CA1416 // Validate platform compatibility

    public override void OnPaintChart(PaintChartEventArgs args)
    {
        Graphics gr = args.Graphics;
        gr.SmoothingMode = SmoothingMode.AntiAlias;
        var mainWindow = this.CurrentChart.Windows[args.WindowIndex];
        var converter = mainWindow.CoordinatesConverter;
        var clientRect = mainWindow.ClientRectangle;

        gr.SetClip(clientRect);
        DateTime leftTime = new[] { converter.GetTime(clientRect.Left), this.HistoricalData.Time(this!.Count - 1) }.Max();
        DateTime rightTime = new[] { converter.GetTime(clientRect.Right), this.HistoricalData.Time(0) }.Min();
        int leftIndex = (int)this.HistoricalData.GetIndexByTime(leftTime.Ticks) + 1;
        int rightIndex = (int)this.HistoricalData.GetIndexByTime(rightTime.Ticks);

        for (int i = rightIndex; i < leftIndex; i++)
        {
            int barX = (int)converter.GetChartX(this.HistoricalData.Time(i));
            int barY = (int)converter.GetChartY(HistogramSeries![i] * 2.0);
            int barY0 = (int)converter.GetChartY(0);
            int HistBarWidth = this.CurrentChart.BarsWidth - 2;

            Brush lowGreen = new SolidBrush(Color.FromArgb(255, 0, 100, 0));
            Brush highGreen = new SolidBrush(Color.FromArgb(255, 50, 255, 50));
            Brush lowRed = new SolidBrush(Color.FromArgb(255, 100, 0, 0));
            Brush highRed = new SolidBrush(Color.FromArgb(255, 255, 50, 50));

            if (HistogramSeries[i] > 0)
            {
                Brush col = HistSlopeSeries![i] > 0 ? highGreen : lowGreen;
                gr.FillRectangle(col, barX, barY, HistBarWidth, Math.Abs(barY - barY0));
            }
            else
            {
                Brush col = HistSlopeSeries![i] < 0 ? highRed : lowRed;
                gr.FillRectangle(col, barX, barY0, HistBarWidth, Math.Abs(barY0 - barY));
            }
        }

        this.PaintSmoothCurve(args, MainSeries!, macd!.WarmupPeriod, showColdValues: ShowColdValues, tension: 0.3);
        this.PaintSmoothCurve(args, SignalSeries!, macd!.WarmupPeriod, showColdValues: ShowColdValues, tension: 0.2);
        base.OnPaintChart(args);
    }
}

