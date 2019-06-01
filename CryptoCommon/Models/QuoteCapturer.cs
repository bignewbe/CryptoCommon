using CryptoCommon.DataTypes;
using CryptoCommon.Interfaces;
using PortableCSharpLib.TechnicalAnalysis;
using Serialization.Serialize;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CryptoCommon.Models
{
    public class QuoteCapturer : IQuoteCapturer
    {
        string _dumpfile = "QuoteCapturer.dump";
        public List<int> Intervals { get; private set; } = new List<int> { 30, 60, 300, 900, 1800, 3600, 86400 };

        IHistoricalQuote _hist;

        public string DataFolder { get { return _hist.DataFolder; } }
        public int MaxNumTicker { get; private set; }
        public int MaxNumBars { get; private set; }
        public bool IsStarted { get; private set; }

        public string Exchange { get { return TikerSocket.Exchange; } }
        public ISocketTicker TikerSocket { get; private set; }

        ConcurrentDictionary<string, Ticker> _tickers = new ConcurrentDictionary<string, Ticker>();
        ConcurrentDictionary<string, IQuoteCapture> _quoteCaptures = new ConcurrentDictionary<string, IQuoteCapture>();
        ConcurrentDictionary<string, ConcurrentDictionary<int, IQuoteBasic>> _quotes = new ConcurrentDictionary<string, ConcurrentDictionary<int, IQuoteBasic>>();

        bool _isProcessingTickerBusy = false;

        public event EventHandlers.TickerReceivedEventHandlerList OnTickerListReceived;
        public event EventHandlers.TickerReceivedEventHandler OnTickerReceived;
        public event EventHandlers.ExceptionOccuredEventHandler OnExceptionOccured;
        public event EventHandlers.DataAddedOrUpdatedEventHandler OnQuoteBasicDataAddedOrUpated;
        public event EventHandlers.QuoteSavedEventHandler OnQuoteSaved;

        public QuoteCapturer(IHistoricalQuote hist, ISocketTicker socket, int maxNumTicker, int maxNumBars, List<int> intervals=null)
        {
            _hist = hist;
            this.TikerSocket = socket;
            this.MaxNumTicker = maxNumTicker;
            this.MaxNumBars = maxNumBars;
            this.TikerSocket.OnExceptionOccured += (object sender, string exchange, System.Exception ex) 
                => OnExceptionOccured?.Invoke(sender, exchange, ex);
            _hist.OnQuoteSaved += (object sender, string exch, string filename) => OnQuoteSaved?.Invoke(sender, exch, filename);

            if (intervals != null) Intervals = new List<int>(intervals);
            //if (File.Exists(_dumpfile))
            //{
            //    var l = BinarySerializer.DeSerializeObject(_dumpfile, typeof(List<IQuoteCapture>)) as List<IQuoteCapture>;
            //    _quoteCaptures = new ConcurrentDictionary<string, IQuoteCapture>(l.ToDictionary(q => q.Symbol, q => q));
            //}
        }

        public void Start()
        {
            if (!this.IsStarted)
            {
                this.IsStarted = !this.IsStarted;
                //TikerSocket.OnTickerReceived += _market_OnTickerReceived;
                TikerSocket.OnTickerListReceived += TikerSocket_OnTickerListReceived;
                TikerSocket.Start();
            }
        }

        public void Stop()
        {
            if (this.IsStarted)
            {
                this.IsStarted = !this.IsStarted;
                //TikerSocket.OnTickerReceived -= _market_OnTickerReceived;
                TikerSocket.Stop();
            }
        }

        //private void _market_OnTickerReceived(object sender, string exchange, CryptoCommon.DataTypes.Ticker ticker)
        //{
        //    OnTickerReceived?.Invoke(this, exchange, ticker);
        //    if (_isProcessingTickerBusy) return;
        //    try
        //    {
        //        _isProcessingTickerBusy = !_isProcessingTickerBusy;
        //        this.AddTicker(ticker);
        //    }
        //    catch (Exception ex)
        //    {
        //        OnExceptionOccured?.Invoke(this, this.Exchange, ex);
        //    }
        //    finally
        //    {
        //        _isProcessingTickerBusy = !_isProcessingTickerBusy;
        //    }
        //}

        private void TikerSocket_OnTickerListReceived(object sender, string exchange, List<DataTypes.Ticker> ticker)
        {
            OnTickerListReceived?.Invoke(this, exchange, ticker);

            if (_isProcessingTickerBusy) return;

            try
            {
                _isProcessingTickerBusy = !_isProcessingTickerBusy;

                //var symbols = ticker.Select(t => t.Symbol).Distinct().ToList();
                foreach (var t in ticker)
                {
                    var symbol = t.Symbol;
                    if (!_tickers.ContainsKey(symbol)) _tickers.TryAdd(symbol, new Ticker(t));

                    ////////////////////////////////////////////////////////////////////////////
                    //qc
                    if (!_quoteCaptures.ContainsKey(symbol)) _quoteCaptures.TryAdd(symbol, new QuoteCapture(symbol));

                    _quoteCaptures[symbol].Add(t.Timestamp, (t.Bid + t.Ask) / 2, t.Volume - _tickers[symbol].Volume);
                    //save quote capture to buffer
                    _tickers[symbol].Copy(t);

                    //qb
                    if (!_quotes.ContainsKey(symbol))
                    {
                        _quotes.TryAdd(symbol, new ConcurrentDictionary<int, IQuoteBasic>());
                        foreach (var interval in Intervals)
                        {
                            var qb = _hist.LoadHistoricalData(symbol, interval, 0, this.MaxNumBars) ?? new QuoteBasic(symbol, interval);
                            //var q1 = _hist.DownloadHistoricalData(symbol, interval) as IQuoteBasic;
                            //if (q1 != null) qb.Append(q1);
                            qb.OnDataAddedOrUpdated += Qb_OnDataAddedOrUpdated;
                            _quotes[symbol].TryAdd(interval, qb);
                        }
                    }
                    else
                    {
                        foreach (var interval in Intervals)
                            _quotes[symbol][interval].Append(_quoteCaptures[symbol], -1, false, true);
                    }

                    if (_quoteCaptures[symbol].Count > this.MaxNumTicker)
                    {
                        foreach (var interval in Intervals)
                            _quotes[symbol][interval].Append(_quoteCaptures[symbol]);

                        _quoteCaptures[symbol].Time.RemoveRange(0, MaxNumTicker / 2);
                        _quoteCaptures[symbol].Price.RemoveRange(0, MaxNumTicker / 2);
                        _quoteCaptures[symbol].Volume.RemoveRange(0, MaxNumTicker / 2);
                    }

                    //var count = 0;
                    //var symbol = t.Symbol;
                    //if (!_tickers.ContainsKey(symbol))
                    //{
                    //    _tickers.TryAdd(symbol, new Ticker(t));
                    //    ++count;
                    //}
                    //else
                    //    Console.WriteLine($"{symbol} not added");
                    //this.AddTicker(t);
                }

            }
            //catch (Exception ex)
            //{
            //    OnExceptionOccured?.Invoke(this, this.Exchange, ex);
            //}
            finally
            {
                _isProcessingTickerBusy = !_isProcessingTickerBusy;
            }
        }

        //private void AddTicker(DataTypes.Ticker ticker)
        //{
        //    var symbol = ticker.Symbol;
        //    if (!_tickers.ContainsKey(symbol)) _tickers.TryAdd(symbol, new Ticker(ticker));

        //    ////////////////////////////////////////////////////////////////////////////
        //    //qc
        //    if (!_quoteCaptures.ContainsKey(symbol)) _quoteCaptures.TryAdd(symbol, new QuoteCapture(symbol));
        //    _quoteCaptures[symbol].Add(ticker.Timestamp, (ticker.Bid + ticker.Ask) / 2, ticker.Volume - _tickers[symbol].Volume);
        //    //save quote capture to buffer
        //    _tickers[symbol].Copy(ticker); 

        //    //qb
        //    if (!_quotes.ContainsKey(symbol))
        //    {
        //        _quotes.TryAdd(symbol, new ConcurrentDictionary<int, IQuoteBasic>());
        //        foreach (var interval in Intervals)
        //        {                    
        //            var qb = _hist.LoadHistoricalData(symbol, interval, 0, this.MaxNumBars) ?? new QuoteBasic(symbol, interval);
        //            //var q1 = _hist.DownloadHistoricalData(symbol, interval) as IQuoteBasic;
        //            //if (q1 != null) qb.Append(q1);
        //            qb.OnDataAddedOrUpdated += Qb_OnDataAddedOrUpdated;
        //            _quotes[symbol].TryAdd(interval, qb);
        //        }
        //    }
        //    else
        //    {
        //        foreach (var interval in Intervals)
        //            _quotes[symbol][interval].Append(_quoteCaptures[symbol], -1, false, true);
        //    }

        //    if (_quoteCaptures[symbol].Count > this.MaxNumTicker)
        //    {
        //        foreach (var interval in Intervals)
        //            _quotes[symbol][interval].Append(_quoteCaptures[symbol], -1, false, true);

        //        _quoteCaptures[symbol].Time.RemoveRange(0, MaxNumTicker / 2);
        //        _quoteCaptures[symbol].Price.RemoveRange(0, MaxNumTicker / 2);
        //        _quoteCaptures[symbol].Volume.RemoveRange(0, MaxNumTicker / 2);
        //    }
        //}

        private void Qb_OnDataAddedOrUpdated(object sender, IQuoteBasic quote, int numAppended)
        {
            OnQuoteBasicDataAddedOrUpated?.Invoke(sender, this.Exchange, quote, numAppended);
            if (quote.Count > this.MaxNumBars)
            {
                //save to file
                _hist.SaveHistoricalData(quote);
                quote.Clear(0, this.MaxNumBars / 2);
            }
        }

        public List<string> GetAvaliableSymbols()
        {
            return _quoteCaptures.Keys.ToList();
        }

        public IQuoteCapture GetInMemoryQuoteCapture(string symbol)
        {
            if (!_quoteCaptures.ContainsKey(symbol)) return null;
            return _quoteCaptures[symbol];
        }

        public IQuoteBasic GetInMemoryQuoteBasic(string symbol, int interval) //, long stimeUtc = -1, long etimeUtc = -1, int maxCount = -1)
        {
            return _quotes.ContainsKey(symbol) && _quotes[symbol].ContainsKey(interval) ? _quotes[symbol][interval] : null;
        }

        ~QuoteCapturer()
        {
            //BinarySerializer.SerializeObject<List<IQuoteCapture>>(_dumpfile, _quoteCaptures.Values.ToList());            
            foreach (var symbol in _quoteCaptures.Keys)
            {
                var interval = this.Intervals[0];
                _quotes[symbol][interval].Append(_quoteCaptures[symbol]);
                _hist.SaveHistoricalData(_quotes[symbol][interval]);

                for (int i = 1; i < this.Intervals.Count; i++)
                {
                    _quotes[symbol][this.Intervals[i]].Append(_quotes[symbol][this.Intervals[i - 1]]);
                    _hist.SaveHistoricalData(_quotes[symbol][this.Intervals[i]]);
                }
            }
        }
    }
}
