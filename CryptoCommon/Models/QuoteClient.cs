using CommonCSharpLibary.CustomExtensions;
using CryptoCommon.Interfaces;
using PortableCSharpLib.TechnicalAnalysis;
using StockChart.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace CryptoCommon.Models
{
    public class QuoteClient : IQuoteClient
    {
        Label _label = new Label();

        IProductMeta _meta;
        IPriceStore _store;
        ConcurrentDictionary<string, IMarket> _markets;

        QuoteCaptureChart _quoteCaptureChart = new QuoteCaptureChart("quote capture chart");
        QuoteBasicChart _quoteBasicChart = new QuoteBasicChart("realtime chart");
        AutoResetEvent _eventRealtimeQuote = new AutoResetEvent(true);

        ConcurrentDictionary<string, IQuoteBasic> _temp = new ConcurrentDictionary<string, IQuoteBasic>();
        ConcurrentDictionary<string, bool> _isStreamingStarted = new ConcurrentDictionary<string, bool>();

        bool _isStarted { get { return _markets.Values.All(m => m.IsStarted); } }
        bool _isStoped { get { return _markets.Values.All(m => !m.IsStarted); } }
        
        bool _IsShowChart = true;
        public bool IsShowChart
        {
            get { return _IsShowChart; }
            set
            {
                if (_IsShowChart != value)
                {
                    _IsShowChart = value;
                    _quoteBasicChart.ShowChart(value);
                    _quoteCaptureChart.ShowChart(value);
                }
            }
        }

        public int NumBar { get; private set; }
        public int Interval { get; private set; }
        public HashSet<string> SubscribedSymbols{ get; set; }

        public event EventHandlers.DataBarReceivedEventHandler OnRealtimeBar;
        public ConcurrentDictionary<string, IQuoteBasic> Quotes { get; private set; } = new ConcurrentDictionary<string, IQuoteBasic>();
        ConcurrentDictionary<string, IQuoteCapture> _quoteCaptures = new ConcurrentDictionary<string, IQuoteCapture>();

        public QuoteClient(int numBar, int interval, bool isShowChart, List<IMarket> markets, params string[] symbols)
        {
            _IsShowChart = isShowChart;
            this.NumBar = numBar;
            this.Interval = interval;           
            this.SubscribedSymbols = new HashSet<string>(symbols);
            _markets = new ConcurrentDictionary<string, IMarket>();
            foreach(var m in markets)
            {
                if (!_markets.ContainsKey(m.Exchange))
                    _markets.TryAdd(m.Exchange, m);
            }
            //_quoteBasicChart.ShowChart(true);
            //_quoteCaptureChart.ShowChart(true);
        }

        public void Start ()
        {
            lock (this)
            {
                if (_isStarted) return;

                foreach (var m in _markets.Values)
                {
                    if (!m.IsStarted)
                    {
                        m.OnTickerReceived += M_OnTickerReceived;
                        m.Start();
                    }
                }
            }
        }

        public void Stop()
        {
            lock (this)
            {
                if (!_isStoped) return;
                foreach (var m in _markets.Values)
                {
                    if (m.IsStarted) m.Stop();
                }
            }
        }

        private void M_OnTickerReceived(object sender, string exchange, CryptoCommon.DataTypes.Ticker ticker)
        {
            var symbol = ProductMeta.GetUniqueSymbolFromStandardSymbol(exchange, ticker.Symbol);
            if (!this.SubscribedSymbols.Contains(symbol)) return;

            if (!_quoteCaptures.ContainsKey(symbol))
            {
                _quoteCaptures.TryAdd(symbol, new QuoteCapture(symbol));
                //_quoteCaptures[symbol].DataAdded += QuoteClient_DataAdded;
                Quotes.TryAdd(symbol, new QuoteBasic(symbol,this.Interval));
                Quotes[symbol].OnDataAddedOrUpdated += QuoteClient_OnDataAddedOrUpdated;

                if (_IsShowChart)
                {
                    Console.WriteLine("Initialze chart {0}...", symbol);

                    _label.InvokeIfRequired(c =>
                    {
                        //_quoteBasicChart.AddSeriesForChartArea(symbol, Quotes[symbol].Interval, symbol);
                        _quoteBasicChart.SetChartAreaPosition();
                        _quoteBasicChart.ShowChart(true);

                        //_quoteCaptureChart.AddSeriesForChartArea(symbol, new List<string> { symbol+"_bid", symbol+"_ask" });
                        _quoteCaptureChart.SetChartAreaPosition();
                        _quoteCaptureChart.ShowChart(true);
                    });
                }
            }

            _quoteCaptures[symbol].Add(ticker.Timestamp, (ticker.Bid + ticker.Ask) / 2);
            //_label.InvokeIfRequired(c => _quoteCaptureChart.UpdateChart(symbol + "_bid", symbol, ticker.Timestamp, ticker.Bid));
            //_label.InvokeIfRequired(c => _quoteCaptureChart.UpdateChart(symbol + "_ask", symbol, ticker.Timestamp, ticker.Ask));
            
            var num = Quotes[symbol].Append(_quoteCaptures[symbol]);

            if (_quoteCaptures[symbol].Count > 5000)
            {
                var index = _quoteCaptures[symbol].Time.FindLastIndex(t => t < Quotes[symbol].LastTime);
                if (index > 0)
                {
                    _quoteCaptures[symbol].Time.RemoveRange(0, index + 1);
                    _quoteCaptures[symbol].Price.RemoveRange(0, index + 1);
                }
            }
        }

        private void QuoteClient_OnDataAddedOrUpdated(object sender, IQuoteBasic quote, int numAppended)
        {
            if (this.IsShowChart)
            {
                //_label.InvokeIfRequired(c => _quoteBasicChart.UpdateChart(quote.Symbol, quote.Symbol, quote));
            }
        }

        //private void QuoteClient_QuoteBasicDataAdded(object sender, string symbol, long time, double open, double close, double high, double low, double volume)
        //{
        //    this.OnRealtimeBar?.Invoke(this, symbol, time, open, close, high, low, volume);
        //    if (this.IsShowChart)
        //    {
        //        Console.WriteLine("Update chart {0}...", symbol);
        //        _label.InvokeIfRequired(c=> _quoteBasicChart.UpdateChart(symbol, symbol, this.Quotes[symbol]));
        //    }
        //}
    }
}
