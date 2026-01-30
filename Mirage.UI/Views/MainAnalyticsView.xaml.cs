using System.Windows;
using System.Windows.Controls;
using Mirage.UI.ViewModels;
using PortalMirage.Core.Dtos;
using ScottPlot;

namespace Mirage.UI.Views;

public partial class MainAnalyticsView : UserControl
{
    public MainAnalyticsView()
    {
        InitializeComponent();
        // Hook into DataContext changes to listen to the ViewModel
        this.DataContextChanged += (s, e) =>
        {
            if (e.NewValue is MainAnalyticsViewModel vm)
            {
                vm.RequestChartUpdate += UpdateChart;
            }
        };
    }

    private void UpdateChart(AnalyticsReportDto data)
    {
        MainPlot.Plot.Clear();
        if (data.ChartData == null || !data.ChartData.Any())
        {
            MainPlot.Refresh();
            return;
        }

        double[] values = data.ChartData.Select(x => (double)x.Value).ToArray();

        // 1. Add Bars
        var bars = MainPlot.Plot.Add.Bars(values);
        bars.Color = ScottPlot.Color.FromHex("#2196F3"); // Professional blue

        // 2. Fix Overlapping X-Axis Labels
        ScottPlot.Tick[] ticks = data.ChartData
            .Select((x, i) => new ScottPlot.Tick(i, x.Label)).ToArray();
        MainPlot.Plot.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.NumericManual(ticks);

        // ROTATION: This is the specific fix for your overlapping labels
        MainPlot.Plot.Axes.Bottom.TickLabelStyle.Rotation = 45;
        MainPlot.Plot.Axes.Bottom.TickLabelStyle.Alignment = ScottPlot.Alignment.MiddleLeft;

        // 3. Dynamic Axis Labels based on Selection
        MainPlot.Plot.Axes.Left.Label.Text = SelectedLogbook == "Machine Breakdown"
            ? "Downtime (Hours)"
            : "Completion (%)";
        MainPlot.Plot.Axes.Bottom.Label.Text = SelectedLogbook == "Machine Breakdown"
            ? "Equipment / Machine Name"
            : "Date";

        // 4. Scale Fix: Force Y-Axis to start at 0 
        double maxValue = values.Any() ? values.Max() * 1.2 : 10;
        MainPlot.Plot.Axes.SetLimitsY(0, maxValue);

        // 5. Final Polish (REPLACED MinimumTickSpacing)
        MainPlot.Plot.Title(data.ChartTitle);

        // This tells ScottPlot to always show at least one tick for every bar
        MainPlot.Plot.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.NumericManual(ticks);

        // Add a line at Y=0 to act as a floor for the bars
        MainPlot.Plot.Add.HorizontalLine(0, 1, ScottPlot.Colors.Black);

        MainPlot.Refresh();
    }

    // Property to track selected logbook type (add this if not already present)
    private string SelectedLogbook
    {
        get
        {
            if (DataContext is MainAnalyticsViewModel vm)
            {
                return vm.SelectedLogbook;
            }
            return string.Empty;
        }
    }
}