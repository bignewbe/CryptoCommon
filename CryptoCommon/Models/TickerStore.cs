using System.Collections.Generic;
using System.Linq;
using CryptoCommon.DataTypes;
using CryptoCommon.Interfaces;
using System.Collections.Concurrent;
using CommonCSharpLibary.CustomExtensions;
using System.Threading.Tasks;
using PortableCSharpLib.DataType;

namespace CryptoCommon.Models
{
    public class TickerStore : ITickerStore
    {
        ConcurrentDictionary<string, ConcurrentDictionary<string, Ticker>> _tickers = new ConcurrentDictionary<string, ConcurrentDictionary<string, Ticker>>();

        public event CryptoCommon.EventHandlers.TickerUpdatedEventHandler OnTickerUpdated;
        //public event EventHandlers.TickerReceivedEventHandler OnTickerReceived;

        public double? ComputeRatioBetweenExchanges(string exchange1, string fiat1, string exchange2, string fiat2, string currency)
        {
            var standardSymbol1 = string.Format("{0}_{1}", fiat1, currency);
            var standardSymbol2 = string.Format("{0}_{1}", fiat2, currency);
            return this.ComputeRatioBetweenExchanges(exchange1, standardSymbol1, exchange2, standardSymbol2);
        }

        public double? ComputeRatioBetweenExchanges(string exchange1, string standardSymbol1, string exchange2, string standardSymbol2)
        {
            if (!_tickers.ContainsKey(exchange1) || !_tickers.ContainsKey(exchange2)) return null;
            if (!_tickers[exchange1].ContainsKey(standardSymbol1) || !_tickers[exchange2].ContainsKey(standardSymbol2)) return null;

            return _tickers[exchange1][standardSymbol1].Bid / _tickers[exchange2][standardSymbol2].Ask;
        }

        public void AddTickers(string exchange, List<Ticker> tickers)
        {
            foreach(var ticker in tickers)
            {
                var symbol = ticker.Symbol;

                if (!_tickers.ContainsKey(exchange)) _tickers.TryAdd(exchange, new ConcurrentDictionary<string, Ticker>());
                if (!_tickers[exchange].ContainsKey(symbol)) _tickers[exchange].TryAdd(symbol, new Ticker());

                var old = _tickers[exchange][symbol];
                if (old.Timestamp >= ticker.Timestamp) return;

                if (ticker.Bid != old.Bid || ticker.Ask != old.Ask)
                {
                    var volume = ticker.Volume - _tickers[exchange][symbol].Volume;
                    _tickers[exchange][symbol].Copy(ticker);
                    OnTickerUpdated?.Invoke(this, exchange, ticker, volume);
                }
                //OnTickerReceived?.Invoke(this, exchange, ticker);
            }
        }

        public Ticker GetTicker(string exchange, string symbol)
        {
            if (!_tickers.ContainsKey(exchange) || !_tickers[exchange].ContainsKey(symbol))
                return null;

            return _tickers[exchange][symbol];
        }

        public List<Ticker> GetTickers(string exchange)
        {
            if (!_tickers.ContainsKey(exchange)) return null;
            return _tickers[exchange].Values.ToList();
        }
    }
}
