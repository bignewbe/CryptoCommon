using CryptoCommon.DataTypes;
using CryptoCommon.Interfaces;
using PortableCSharpLib.TechnicalAnalysis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace CryptoCommon.Models
{
    public class QuoteQuery : IQuoteQuery
    {
        static readonly List<int> _intervals = new List<int> { 60, 300, 1800, 3600, 86400 };

        public string Exchange => throw new NotImplementedException();

        IQuoteCapturer _cryptoClient;
        IHistoricalQuote _hist;

        public int MaxNumBars { get; private set; }

        ConcurrentDictionary<string, IQuoteCapture> _quoteCaptures = new ConcurrentDictionary<string, IQuoteCapture>();
        ConcurrentDictionary<string, ConcurrentDictionary<int, IQuoteBasic>> _quotes = new ConcurrentDictionary<string, ConcurrentDictionary<int, IQuoteBasic>>();

        bool _isProcessingTickerBusy = false;
        bool _isDownLoadInProgress = false;

        public QuoteQuery()
        {
            _cryptoClient.OnTickerReceived += _cryptoClient_OnTickerReceived;
            _cryptoClient.OnConnectionChanged += _cryptoClient_OnConnectionChanged;
        }

        private async void _cryptoClient_OnTickerReceived(object sender, string exchange, Ticker ticker)
        {
            if (_isProcessingTickerBusy) return;

            try
            {
                _isProcessingTickerBusy = !_isProcessingTickerBusy;

                //var symbol = this.GetUniqueSymbol(exchange, ticker.Symbol);
                var symbol = ticker.Symbol;
                if (symbol == "LTC_BTC")
                    Console.WriteLine();

                if (!_quotes.ContainsKey(symbol))
                {
                    _quoteCaptures.TryAdd(symbol, new QuoteCapture(symbol));
                    _quotes.TryAdd(symbol, new ConcurrentDictionary<int, IQuoteBasic>());

                    //1. intialize from files
                    foreach (var interval in _intervals)
                    {
                        var q = _hist.LoadHistoricalData(symbol, interval, 0, this.MaxNumBars / 2) ?? new QuoteBasic(symbol, interval);
                        var qc = await _cryptoClient.GetInMemoryQuoteCapture(symbol);
                        if (qc != null)
                        {
                            q.Append(qc);
                            _quoteCaptures[symbol].Append(qc);
                        }
                        _quotes[symbol].TryAdd(interval, q);
                    }
                }

                _quoteCaptures[symbol].Add(ticker.Timestamp, ticker.Last);
                foreach (var interval in _intervals)
                {
                    var q = _quotes[symbol][interval];
                    if (q.Append(_quoteCaptures[symbol]) > 0)
                    {
                        //1. remove half data
                        //2. save to file for interval = 5s
                        if (q.Count > this.MaxNumBars)
                        {
                            //save to file
                            _hist.SaveHistoricalData(q);
                            q.Clear(0, this.MaxNumBars / 2);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                _isProcessingTickerBusy = !_isProcessingTickerBusy;
            }
        }

        public List<string> GetAvaliableSymbols(int timeout = 10000)
        {
            return _quotes.Keys.ToList();
        }

        public IQuoteBasic GetQuoteAsync(string symbol, int interval, long stimeUtc, long etimeUtc, int maxCount, int timeout = 15000)
        {
            return _quotes.ContainsKey(symbol) && _quotes[symbol].ContainsKey(interval) ? _quotes[symbol][interval] : null;
        }
    }

}
