using CryptoCommon.Interfaces;
using PortableCSharpLib.TechnicalAnalysis;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace CryptoCommon.Models
{
    public class QuoteCaptureMemStore : IQuoteCaptureMemStore
    {
        public int MaxNumTicks { get; private set; }
        public ConcurrentDictionary<string, IQuoteCapture> QuoteCaptures { get; private set; } = new ConcurrentDictionary<string, IQuoteCapture>();
        
        public string Exchange { get; private set; }
        public event EventHandlers.QuoteCaptureDataAddedOrUpdatedEventHandler OnQuoteCaptureDataAdded;

        public QuoteCaptureMemStore(string exchange, int maxNumTicks)
        {
            MaxNumTicks = maxNumTicks;
            this.Exchange = exchange;
        }

        public void Add(string symbol, long timestamp, double price, double volume)
        {
            lock (this)
            {
                //add to quote capture
                if (!QuoteCaptures.ContainsKey(symbol))
                {
                    var qc = new QuoteCapture(symbol);
                    qc.DataAdded += Qc_DataAdded;
                    QuoteCaptures.TryAdd(symbol, qc);
                }

                if (timestamp > QuoteCaptures[symbol].LastTime)
                    QuoteCaptures[symbol].Add(timestamp, price, volume);
            }
        }

        private void Qc_DataAdded(object sender, string symbol, long time, double price, double volume)
        {
            //Task.Run(() => OnQuoteCaptureDataAdded?.Invoke(sender, this.Exchange, new QuoteCapture(_quoteCaptures[symbol]), 1));
            //Task.Run(() => OnQuoteCaptureDataAdded?.Invoke(sender, this.Exchange, _quoteCaptures[symbol], 1));
            OnQuoteCaptureDataAdded?.Invoke(sender, this.Exchange, QuoteCaptures[symbol], 1);

            if (QuoteCaptures[symbol].Count > this.MaxNumTicks * 2)
            {
                QuoteCaptures[symbol].Time.RemoveRange(0, MaxNumTicks);
                QuoteCaptures[symbol].Price.RemoveRange(0, MaxNumTicks);
                QuoteCaptures[symbol].Volume.RemoveRange(0, MaxNumTicks);
            }
        }

        public IQuoteCapture GetInMemoryQuoteCapture(string symbol)
        {
            return QuoteCaptures.ContainsKey(symbol) ? QuoteCaptures[symbol] : null;
        }

        //public IQuoteCapture AddTicker(Ticker t)
        //{
        //    var symbol = t.Symbol;
        //    if (!_tickers.ContainsKey(symbol)) _tickers.TryAdd(symbol, new Ticker(t));

        //    ////////////////////////////////////////////////////////////////////////////
        //    //add to quote capture
        //    if (!_quoteCaptures.ContainsKey(symbol))
        //    {
        //        var qc = new QuoteCapture(symbol);
        //        _quoteCaptures.TryAdd(symbol, qc);
        //    }

        //    _quoteCaptures[symbol].Add(t.Timestamp, (t.Bid + t.Ask) / 2, t.Volume - _tickers[symbol].Volume);

        //    //raise price updated event
        //    if (_tickers[symbol].Bid != t.Bid || _tickers[symbol].Ask != t.Ask)
        //        OnPriceUpdated?.Invoke(this, this.Exchange, symbol, t);

        //    //save ticker
        //    _tickers[symbol].Copy(t);

        //    if (_quoteCaptures[symbol].Count > this._maxNumTicks * 2)
        //    {
        //        _quoteCaptures[symbol].Time.RemoveRange(0, _maxNumTicks);
        //        _quoteCaptures[symbol].Price.RemoveRange(0, _maxNumTicks);
        //        _quoteCaptures[symbol].Volume.RemoveRange(0, _maxNumTicks);
        //    }

        //    return _quoteCaptures[symbol];
        //}

        //public Ticker GetInMemoryTicker(string symbol)
        //{
        //    return _tickers.ContainsKey(symbol) ? _tickers[symbol] : null;
        //}

        //public List<Ticker> GetInMemoryTickers()
        //{
        //    return new List<Ticker>(_tickers.Values);
        //}
    }
}
