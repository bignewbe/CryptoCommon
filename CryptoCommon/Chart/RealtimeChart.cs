using System;
using System.Collections.Generic;
using System.Linq;
using CommonCSharpLibary.CustomExtensions;
using PortableCSharpLib.TechnicalAnalysis;
using PortableCSharpLib;
using System.Windows.Forms.DataVisualization.Charting;
using System.Drawing;
using CommonCSharpLibary.Facility;

namespace CryptoCommon.Chart
{
    //used for display multiple real time quote streams from ClientQuoteServer
    public class RealtimeChart : CustomChart
    {
        class SeriesInfo
        {
            static public double GetPipFactor(string symbol)
            {
                return symbol.ToUpper().Contains("JPY") ? 100 : 10000;
            }

            public double PipFactor { get { return GetPipFactor(Symbol); } }
            public string Symbol { get; set; }
            public string SeriesName { get; set; }
            public long MinTimeData { get; set; }
            public long MaxTimeData { get; set; }
            public double MinPrice { get; set; }
            public double MaxPrice { get; set; }

            public SeriesInfo()
            {
                Symbol = string.Empty;
                SeriesName = string.Empty;
                MinTimeData = int.MaxValue;
                MaxTimeData = int.MinValue;
                MinPrice = double.MaxValue;
                MaxPrice = double.MinValue;
            }
            public SeriesInfo(string symbol, string seriesName)
                : this()
            {
                Symbol = symbol;
                SeriesName = seriesName;
            }
        }

        List<double> _steps = new List<double> { 1, 2, 5, 10, 20, 50, 100, 200, 500, 1000 };

        const int _maxNumbars = 200;
        Dictionary<string, int> _chartAreaToInterval = new Dictionary<string, int>();
        SortedDictionary<string, List<SeriesInfo>> _dicChartAreaToSeriesInfo;    //each chart area (symbol) has an associated list of series info for determining min/max of axis
        public RealtimeChart(string title)
            : base(title)
        {
            _dicChartAreaToSeriesInfo = new SortedDictionary<string, List<SeriesInfo>>();
        }

        public void AddSeriesForChartArea(string chartAreaName, int interval, params string[] seriesNames)
        {
            if (seriesNames == null || seriesNames.Length <= 0)
            {
                Console.WriteLine("No series has been added. Please specify a series name.");
                return;
            }

            //if (_chart.ChartAreas.Any(a => a.Name == chartAreaName)) return;
            if (_chartAreaToInterval.ContainsKey(chartAreaName)) return;
            _chartAreaToInterval.Add(chartAreaName, interval);
            //add series info for the chart area
            _dicChartAreaToSeriesInfo.Add(chartAreaName, new List<SeriesInfo>());

            //add the chart area if not yet existed
            _chart.InvokeIfRequired(c => c.ChartAreas.Add(chartAreaName));
            _chart.InvokeIfRequired(c => c.ChartAreas[chartAreaName].AxisX.Title = chartAreaName);

            //set Y2 axis
            _chart.InvokeIfRequired(c => c.ChartAreas[chartAreaName].AxisY2.LabelStyle.Format = "#.00##");
            _chart.InvokeIfRequired(c => c.ChartAreas[chartAreaName].AxisY2.LabelStyle.IsEndLabelVisible = true);
            _chart.InvokeIfRequired(c => c.ChartAreas[chartAreaName].AxisY2.IsLabelAutoFit = true);
            _chart.InvokeIfRequired(c => c.ChartAreas[chartAreaName].AxisY2.IsMarksNextToAxis = true);
            _chart.InvokeIfRequired(c => c.ChartAreas[chartAreaName].AxisY2.MajorGrid.LineDashStyle = ChartDashStyle.Dash);
            _chart.InvokeIfRequired(c => c.ChartAreas[chartAreaName].AxisY2.MajorGrid.LineWidth = 1);
            _chart.InvokeIfRequired(c => c.ChartAreas[chartAreaName].AxisY2.MajorGrid.LineColor = Color.Aqua);

            //set X axis
            //_chart.InvokeIfRequired(c => c.ChartAreas[0].AxisX.Minimum = dates[min_index].ToOADate());
            //_chart.InvokeIfRequired(c => c.ChartAreas[0].AxisX.Maximum = dates[max_index].ToOADate());
            _chart.InvokeIfRequired(c => c.ChartAreas[chartAreaName].AxisX.IntervalType = DateTimeIntervalType.Auto);
            _chart.InvokeIfRequired(c => c.ChartAreas[chartAreaName].AxisX.MajorGrid.LineWidth = 1);
            _chart.InvokeIfRequired(c => c.ChartAreas[chartAreaName].AxisX.MajorGrid.LineColor = Color.Aqua);

            //_chart.ChartAreas[0].AxisX.IntervalType = DateTimeIntervalType.Auto;
            //_chart.ChartAreas[0].AxisX.IntervalAutoMode = IntervalAutoMode.VariableCount;
            _chart.InvokeIfRequired(c => c.ChartAreas[chartAreaName].AxisX.IsLabelAutoFit = true);
            _chart.InvokeIfRequired(c => c.ChartAreas[chartAreaName].AxisX.IsMarksNextToAxis = true);

            //set label style
            //_chart.InvokeIfRequired(c => c.ChartAreas[0].AxisX.LabelStyle.IntervalOffsetType = DateTimeIntervalType.Seconds;
            //_chart.InvokeIfRequired(c => c.ChartAreas[0].AxisX.LabelStyle.Interval = 5;
            _chart.InvokeIfRequired(c => c.ChartAreas[chartAreaName].AxisX.LabelStyle.Format = "HH:mm:ss");
            _chart.InvokeIfRequired(c => c.ChartAreas[chartAreaName].AxisX.LabelStyle.Enabled = true);
            //_chart.InvokeIfRequired(c => c.ChartAreas[0].AxisX.LabelStyle.TruncatedLabels = true;

            //set major and minor grid
            _chart.InvokeIfRequired(c => c.ChartAreas[chartAreaName].AxisX.MajorGrid.IntervalType = DateTimeIntervalType.Minutes);
            _chart.InvokeIfRequired(c => c.ChartAreas[chartAreaName].AxisX.MajorGrid.Interval = 5);
            _chart.InvokeIfRequired(c => c.ChartAreas[chartAreaName].AxisX.MajorGrid.LineWidth = 1);
            _chart.InvokeIfRequired(c => c.ChartAreas[chartAreaName].AxisX.MajorGrid.LineColor = Color.Aqua);
            _chart.InvokeIfRequired(c => c.ChartAreas[chartAreaName].AxisX.MinorGrid.IntervalType = DateTimeIntervalType.Minutes);
            _chart.InvokeIfRequired(c => c.ChartAreas[chartAreaName].AxisX.MinorGrid.Interval = 1);
            _chart.InvokeIfRequired(c => c.ChartAreas[chartAreaName].AxisX.MinorGrid.LineWidth = 1);
            _chart.InvokeIfRequired(c => c.ChartAreas[chartAreaName].AxisX.MinorGrid.LineColor = Color.Aqua);
            _chart.InvokeIfRequired(c => c.ChartAreas[chartAreaName].AxisX.MinorGrid.LineDashStyle = ChartDashStyle.Dash);

            //////////////////////////////////////////////////////////////////////////////////////////////////////////////
            foreach (var seriesName in seriesNames)
            {
                var series1 = new Series(seriesName);
                series1.ChartType = SeriesChartType.Candlestick;
                series1.XValueType = ChartValueType.DateTime;
                series1.YValuesPerPoint = 4;
                series1.XValueMember = "Time";
                series1.YValueMembers = "HighPrice, LowPrice, OpenPrice, ClosePrice";
                series1.XAxisType = AxisType.Primary;
                series1.YAxisType = AxisType.Secondary;
                series1.Enabled = true;
                series1.ChartArea = chartAreaName;
                series1.IsVisibleInLegend = true;

                _chart.InvokeIfRequired(c => c.Series.Add(series1));
                //add series info for this chart area
                _dicChartAreaToSeriesInfo[chartAreaName].Add(new SeriesInfo(chartAreaName, seriesName));
            }
        }

        //since we have multiple chart areas, we need to arrange them in proper positions
        public void SetChartAreaPosition()
        {
            int width = -1, height = -1;
            if (_chart.ChartAreas.Count == 1)
                width = height = 100;
            else if (_chart.ChartAreas.Count <= 4)
                width = height = 50;
            else if (_chart.ChartAreas.Count <= 9)
                width = height = 33;
            else if (_chart.ChartAreas.Count <= 12)
            {
                width = 25;
                height = 33;
            }

            if (width == -1) return;

            switch (_chart.ChartAreas.Count)
            {
                case 1:
                    _chart.InvokeIfRequired(c => c.ChartAreas[0].Position.X = 0);
                    _chart.InvokeIfRequired(c => c.ChartAreas[0].Position.Y = 0);
                    break;
                case 2:
                    _chart.InvokeIfRequired(c => c.ChartAreas[0].Position.X = 0);
                    _chart.InvokeIfRequired(c => c.ChartAreas[1].Position.X = 50);
                    _chart.InvokeIfRequired(c => c.ChartAreas[0].Position.Y = 0);
                    _chart.InvokeIfRequired(c => c.ChartAreas[1].Position.Y = 0);
                    break;
                case 3:
                    _chart.InvokeIfRequired(c => c.ChartAreas[0].Position.X = 0);
                    _chart.InvokeIfRequired(c => c.ChartAreas[1].Position.X = 50);
                    _chart.InvokeIfRequired(c => c.ChartAreas[0].Position.Y = 0);
                    _chart.InvokeIfRequired(c => c.ChartAreas[1].Position.Y = 0);

                    _chart.InvokeIfRequired(c => c.ChartAreas[2].Position.X = 0);
                    _chart.InvokeIfRequired(c => c.ChartAreas[2].Position.Y = 50);
                    break;
                case 4:
                    _chart.InvokeIfRequired(c => c.ChartAreas[0].Position.X = 0);
                    _chart.InvokeIfRequired(c => c.ChartAreas[1].Position.X = 50);
                    _chart.InvokeIfRequired(c => c.ChartAreas[0].Position.Y = 0);
                    _chart.InvokeIfRequired(c => c.ChartAreas[1].Position.Y = 0);

                    _chart.InvokeIfRequired(c => c.ChartAreas[2].Position.X = 0);
                    _chart.InvokeIfRequired(c => c.ChartAreas[3].Position.X = 50);
                    _chart.InvokeIfRequired(c => c.ChartAreas[2].Position.Y = 50);
                    _chart.InvokeIfRequired(c => c.ChartAreas[3].Position.Y = 50);
                    break;
                case 5:
                    _chart.InvokeIfRequired(c => c.ChartAreas[0].Position.X = 0);
                    _chart.InvokeIfRequired(c => c.ChartAreas[1].Position.X = 33);
                    _chart.InvokeIfRequired(c => c.ChartAreas[2].Position.X = 66);
                    _chart.InvokeIfRequired(c => c.ChartAreas[0].Position.Y = 0);
                    _chart.InvokeIfRequired(c => c.ChartAreas[1].Position.Y = 0);
                    _chart.InvokeIfRequired(c => c.ChartAreas[2].Position.Y = 0);

                    _chart.InvokeIfRequired(c => c.ChartAreas[3].Position.X = 0);
                    _chart.InvokeIfRequired(c => c.ChartAreas[4].Position.X = 33);
                    _chart.InvokeIfRequired(c => c.ChartAreas[3].Position.Y = 33);
                    _chart.InvokeIfRequired(c => c.ChartAreas[4].Position.Y = 33);

                    break;
                case 6:
                    _chart.InvokeIfRequired(c => c.ChartAreas[0].Position.X = 0);
                    _chart.InvokeIfRequired(c => c.ChartAreas[1].Position.X = 33);
                    _chart.InvokeIfRequired(c => c.ChartAreas[2].Position.X = 66);
                    _chart.InvokeIfRequired(c => c.ChartAreas[0].Position.Y = 0);
                    _chart.InvokeIfRequired(c => c.ChartAreas[1].Position.Y = 0);
                    _chart.InvokeIfRequired(c => c.ChartAreas[2].Position.Y = 0);

                    _chart.InvokeIfRequired(c => c.ChartAreas[3].Position.X = 0);
                    _chart.InvokeIfRequired(c => c.ChartAreas[4].Position.X = 33);
                    _chart.InvokeIfRequired(c => c.ChartAreas[5].Position.X = 66);
                    _chart.InvokeIfRequired(c => c.ChartAreas[3].Position.Y = 33);
                    _chart.InvokeIfRequired(c => c.ChartAreas[4].Position.Y = 33);
                    _chart.InvokeIfRequired(c => c.ChartAreas[5].Position.Y = 33);

                    break;
                case 7:
                    _chart.InvokeIfRequired(c => c.ChartAreas[0].Position.X = 0);
                    _chart.InvokeIfRequired(c => c.ChartAreas[1].Position.X = 33);
                    _chart.InvokeIfRequired(c => c.ChartAreas[2].Position.X = 66);
                    _chart.InvokeIfRequired(c => c.ChartAreas[0].Position.Y = 0);
                    _chart.InvokeIfRequired(c => c.ChartAreas[1].Position.Y = 0);
                    _chart.InvokeIfRequired(c => c.ChartAreas[2].Position.Y = 0);

                    _chart.InvokeIfRequired(c => c.ChartAreas[3].Position.X = 0);
                    _chart.InvokeIfRequired(c => c.ChartAreas[4].Position.X = 33);
                    _chart.InvokeIfRequired(c => c.ChartAreas[5].Position.X = 66);
                    _chart.InvokeIfRequired(c => c.ChartAreas[3].Position.Y = 33);
                    _chart.InvokeIfRequired(c => c.ChartAreas[4].Position.Y = 33);
                    _chart.InvokeIfRequired(c => c.ChartAreas[5].Position.Y = 33);

                    _chart.InvokeIfRequired(c => c.ChartAreas[6].Position.X = 0);
                    _chart.InvokeIfRequired(c => c.ChartAreas[6].Position.Y = 66);
                    break;
                case 8:
                    _chart.InvokeIfRequired(c => c.ChartAreas[0].Position.X = 0);
                    _chart.InvokeIfRequired(c => c.ChartAreas[1].Position.X = 33);
                    _chart.InvokeIfRequired(c => c.ChartAreas[2].Position.X = 66);
                    _chart.InvokeIfRequired(c => c.ChartAreas[0].Position.Y = 0);
                    _chart.InvokeIfRequired(c => c.ChartAreas[1].Position.Y = 0);
                    _chart.InvokeIfRequired(c => c.ChartAreas[2].Position.Y = 0);

                    _chart.InvokeIfRequired(c => c.ChartAreas[3].Position.X = 0);
                    _chart.InvokeIfRequired(c => c.ChartAreas[4].Position.X = 33);
                    _chart.InvokeIfRequired(c => c.ChartAreas[5].Position.X = 66);
                    _chart.InvokeIfRequired(c => c.ChartAreas[3].Position.Y = 33);
                    _chart.InvokeIfRequired(c => c.ChartAreas[4].Position.Y = 33);
                    _chart.InvokeIfRequired(c => c.ChartAreas[5].Position.Y = 33);

                    _chart.InvokeIfRequired(c => c.ChartAreas[6].Position.X = 0);
                    _chart.InvokeIfRequired(c => c.ChartAreas[7].Position.X = 33);
                    _chart.InvokeIfRequired(c => c.ChartAreas[6].Position.Y = 66);
                    _chart.InvokeIfRequired(c => c.ChartAreas[7].Position.Y = 66);
                    break;

                case 9:
                    _chart.InvokeIfRequired(c => c.ChartAreas[0].Position.X = 0);
                    _chart.InvokeIfRequired(c => c.ChartAreas[1].Position.X = 33);
                    _chart.InvokeIfRequired(c => c.ChartAreas[2].Position.X = 66);
                    _chart.InvokeIfRequired(c => c.ChartAreas[0].Position.Y = 0);
                    _chart.InvokeIfRequired(c => c.ChartAreas[1].Position.Y = 0);
                    _chart.InvokeIfRequired(c => c.ChartAreas[2].Position.Y = 0);

                    _chart.InvokeIfRequired(c => c.ChartAreas[3].Position.X = 0);
                    _chart.InvokeIfRequired(c => c.ChartAreas[4].Position.X = 33);
                    _chart.InvokeIfRequired(c => c.ChartAreas[5].Position.X = 66);
                    _chart.InvokeIfRequired(c => c.ChartAreas[3].Position.Y = 33);
                    _chart.InvokeIfRequired(c => c.ChartAreas[4].Position.Y = 33);
                    _chart.InvokeIfRequired(c => c.ChartAreas[5].Position.Y = 33);

                    _chart.InvokeIfRequired(c => c.ChartAreas[6].Position.X = 0);
                    _chart.InvokeIfRequired(c => c.ChartAreas[7].Position.X = 33);
                    _chart.InvokeIfRequired(c => c.ChartAreas[8].Position.X = 66);
                    _chart.InvokeIfRequired(c => c.ChartAreas[6].Position.Y = 66);
                    _chart.InvokeIfRequired(c => c.ChartAreas[7].Position.Y = 66);
                    _chart.InvokeIfRequired(c => c.ChartAreas[8].Position.Y = 66);
                    break;

                case 10:
                    _chart.InvokeIfRequired(c => c.ChartAreas[0].Position.X = 0);
                    _chart.InvokeIfRequired(c => c.ChartAreas[1].Position.X = 25);
                    _chart.InvokeIfRequired(c => c.ChartAreas[2].Position.X = 50);
                    _chart.InvokeIfRequired(c => c.ChartAreas[3].Position.X = 75);
                    _chart.InvokeIfRequired(c => c.ChartAreas[0].Position.Y = 0);
                    _chart.InvokeIfRequired(c => c.ChartAreas[1].Position.Y = 0);
                    _chart.InvokeIfRequired(c => c.ChartAreas[2].Position.Y = 0);
                    _chart.InvokeIfRequired(c => c.ChartAreas[3].Position.Y = 0);

                    _chart.InvokeIfRequired(c => c.ChartAreas[4].Position.X = 0);
                    _chart.InvokeIfRequired(c => c.ChartAreas[5].Position.X = 25);
                    _chart.InvokeIfRequired(c => c.ChartAreas[6].Position.X = 50);
                    _chart.InvokeIfRequired(c => c.ChartAreas[7].Position.X = 75);
                    _chart.InvokeIfRequired(c => c.ChartAreas[4].Position.Y = 33);
                    _chart.InvokeIfRequired(c => c.ChartAreas[5].Position.Y = 33);
                    _chart.InvokeIfRequired(c => c.ChartAreas[6].Position.Y = 33);
                    _chart.InvokeIfRequired(c => c.ChartAreas[7].Position.Y = 33);

                    _chart.InvokeIfRequired(c => c.ChartAreas[8].Position.X = 0);
                    _chart.InvokeIfRequired(c => c.ChartAreas[9].Position.X = 25);
                    _chart.InvokeIfRequired(c => c.ChartAreas[8].Position.Y = 66);
                    _chart.InvokeIfRequired(c => c.ChartAreas[9].Position.Y = 66);
                    break;
                default:
                    break;
            }

            for (int i = 0; i < _chart.ChartAreas.Count; i++)
            {
                _chart.InvokeIfRequired(c => c.ChartAreas[i].Position.Width = width);
                _chart.InvokeIfRequired(c => c.ChartAreas[i].Position.Height = height);
            }
        }

        public void InitializeChart(string chartAreaName, string seriesName, IQuoteBasic quote)
        {
            if (quote == null || quote.Count <= 1) return;

            var series = ChartSeries.FindByName(seriesName);
            if (series == null) return;

            var seriesInfo = _dicChartAreaToSeriesInfo[chartAreaName].Find(s => s.SeriesName == seriesName);
            if (seriesInfo == null) return;

            var max_index = quote.Count - 1;
            var min_index = Math.Max(0, max_index - _maxNumbars);
            var times = quote.Time.GetRange(min_index, max_index - min_index + 1).Select(t => t.GetUTCFromUnixTime().ToOADate()).ToList();
            var high = quote.High.GetRange(min_index, max_index - min_index + 1).ToList();
            var low = quote.Low.GetRange(min_index, max_index - min_index + 1).ToList();
            var open = quote.Open.GetRange(min_index, max_index - min_index + 1).ToList();
            var close = quote.Close.GetRange(min_index, max_index - min_index + 1).ToList();

            _chart.InvokeIfRequired(c => series.Points.Clear());
            _chart.InvokeIfRequired(c => series.Points.DataBindXY(times, high, low, open, close));

            seriesInfo.MinTimeData = DateTime.FromOADate(times.FirstOrDefault()).GetUnixTimeFromUTC();
            seriesInfo.MaxTimeData = DateTime.FromOADate(times.LastOrDefault()).GetUnixTimeFromUTC();

            this.SetAxisRangeForChartArea(chartAreaName, series, seriesInfo);
        }

        public void UpdateChart(string chartAreaName, string seriesName, long timestamp, double open, double high, double low, double close)
        {
            var series = ChartSeries.FindByName(seriesName);
            if (series == null) return;

            var seriesInfo = _dicChartAreaToSeriesInfo[chartAreaName].Find(s => s.SeriesName == seriesName);
            if (seriesInfo == null) return;

            //if the new data already shown in chart, we skip
            if (series.Points.Count > 0 && timestamp <= seriesInfo.MaxTimeData)
                return;

            //remove first elements if gets too much data
            if (series.Points.Count >= _maxNumbars * 10)
            {
                var points = series.Points.ToList();
                points.RemoveRange(0, 9 * _maxNumbars);
                var times = points.Select(p => p.XValue).ToList();
                var highs = points.Select(p => p.YValues[0]).ToList();
                var lows = points.Select(p => p.YValues[1]).ToList();
                var opens = points.Select(p => p.YValues[2]).ToList();
                var closes = points.Select(p => p.YValues[3]).ToList();
                _chart.InvokeIfRequired(c => series.Points.Clear());
                _chart.InvokeIfRequired(c => series.Points.DataBindXY(times, highs, lows, opens, closes));
            }

            //add the new data to series
            _chart.InvokeIfRequired(c => series.Points.AddXY(timestamp.GetUTCFromUnixTime().ToOADate(), high, low, open, close));
            seriesInfo.MinTimeData = DateTime.FromOADate(series.Points.FirstOrDefault().XValue).GetUnixTimeFromUTC();
            seriesInfo.MaxTimeData = timestamp;

            //seriesInfo.MaxPrice = Math.Max(seriesInfo.MaxPrice, high);
            //seriesInfo.MinPrice = Math.Min(seriesInfo.MinPrice, low);            
            this.SetAxisRangeForChartArea(chartAreaName, series, seriesInfo);
        }

        public void UpdateChart(string chartAreaName, string seriesName, IQuoteBasic quote)
        {
            var series = ChartSeries.FindByName(seriesName);
            if (series == null) return;

            var seriesInfo = _dicChartAreaToSeriesInfo[chartAreaName].Find(s => s.SeriesName == seriesName);
            if (seriesInfo == null) return;

            var sindex = quote.Count - 1;
            while (sindex >= 0 && quote.Time[sindex] > seriesInfo.MaxTimeData) --sindex; //index must be -1 or quote.Time[index] <= maxTimeData
            ++sindex;

            var num = quote.Count - sindex;
            if (num <= 0) return;

            if (num > 1)
            {
                var points = series.Points.ToList();
                var lst = Enumerable.Range(sindex, num).Select(i => new DataPoint(quote.Time[i].GetUTCFromUnixTime().ToOADate(), new List<double> { quote.High[i], quote.Low[i], quote.Open[i], quote.Close[i] }.ToArray()));
                points.AddRange(lst);
                _chart.InvokeIfRequired(c => series.Points.DataBindXY(
                                             points.Select(l => l.XValue).ToList(),
                                             points.Select(l => l.YValues[0]).ToList(),
                                             points.Select(l => l.YValues[1]).ToList(),
                                             points.Select(l => l.YValues[2]).ToList(),
                                             points.Select(l => l.YValues[3]).ToList()));
            }
            else //num == 1
            {
                _chart.InvokeIfRequired(c => series.Points.AddXY(quote.LastTime.GetUTCFromUnixTime().ToOADate(), quote.High.Last(), quote.Low.Last(), quote.Open.Last(), quote.Close.Last()));
            }

            //remove first elements if gets too much data
            if (series.Points.Count >= _maxNumbars * 10)
            {
                var points = series.Points.ToList();
                points.RemoveRange(0, 9 * _maxNumbars);
                _chart.InvokeIfRequired(c => series.Points.DataBindXY(
                                             points.Select(l => l.XValue).ToList(),
                                             points.Select(l => l.YValues[0]).ToList(),
                                             points.Select(l => l.YValues[1]).ToList(),
                                             points.Select(l => l.YValues[2]).ToList(),
                                             points.Select(l => l.YValues[3]).ToList()));
            }

            //add the new data to series
            seriesInfo.MinTimeData = DateTime.FromOADate(series.Points.FirstOrDefault().XValue).GetUnixTimeFromUTC();
            seriesInfo.MaxTimeData = DateTime.FromOADate(series.Points.LastOrDefault().XValue).GetUnixTimeFromUTC();

            this.SetAxisRangeForChartArea(chartAreaName, series, seriesInfo);
        }


        void SetAxisRangeForChartArea(string chartAreaName, Series series, SeriesInfo seriesInfo)
        {
            //////////////////////////////////////////////////////////////////////////////////////////
            //set X axis
            var maxTimeData = _dicChartAreaToSeriesInfo[chartAreaName].Select(s => s.MaxTimeData).Max();
            var maxTimeAxis = maxTimeData + 5 * _chartAreaToInterval[chartAreaName];
            var minTimeAxis = maxTimeAxis - _maxNumbars * _chartAreaToInterval[chartAreaName];
            var maxAxisX = maxTimeAxis.GetUTCFromUnixTime().ToOADate();
            var minAxisX = minTimeAxis.GetUTCFromUnixTime().ToOADate();

            _chart.InvokeIfRequired(c => c.ChartAreas[chartAreaName].AxisX.Maximum = maxAxisX);
            _chart.InvokeIfRequired(c => c.ChartAreas[chartAreaName].AxisX.Minimum = minAxisX);

            ////////////////////////////////////////////////////////////
            //set Y2 range
            var price = series.Points.Where(p => CFacility.WithinRange(p.XValue, minAxisX, maxAxisX)).Select(p => p.YValues[0]).ToList();
            seriesInfo.MaxPrice = price.Max();
            seriesInfo.MinPrice = price.Min();

            var max = _dicChartAreaToSeriesInfo[chartAreaName].Select(s => s.MaxPrice).Max();
            var min = _dicChartAreaToSeriesInfo[chartAreaName].Select(s => s.MinPrice).Min();

            //var listSteps = new List<double>() { 1, 2, 5, 10, 20, 50, 100, 200, 500, 1000 };
            var steps = _steps.Select(l => l / seriesInfo.PipFactor).ToList();
            var step = (max - min) / 5;
            step = (step <= steps.First()) ? steps.First() : steps.Last(item => item <= step);
            min = (int)(min / step) * step;
            max = min + ((int)((max - min) / step) + 1) * step;

            //set range of Y axis
            if (_chart.ChartAreas[chartAreaName].AxisY2.Minimum != min || _chart.ChartAreas[chartAreaName].AxisY2.Maximum != max)
            {
                //the normal Y2 axis
                _chart.InvokeIfRequired(c => c.ChartAreas[chartAreaName].AxisY2.Minimum = min);
                _chart.InvokeIfRequired(c => c.ChartAreas[chartAreaName].AxisY2.Maximum = max);
                _chart.InvokeIfRequired(c => c.ChartAreas[chartAreaName].AxisY2.Interval = step);
                _chart.InvokeIfRequired(c => c.ChartAreas[chartAreaName].AxisY2.LabelStyle.Interval = step);
                _chart.InvokeIfRequired(c => c.ChartAreas[chartAreaName].AxisY2.MajorGrid.Interval = step);
            }
        }
    }
}
