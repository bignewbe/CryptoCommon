using CryptoCommon.DataTypes;
using CryptoCommon.Interfaces;
using PortableCSharpLib.DataType;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace CryptoCommon.Models
{
    /// <summary>
    /// 1. one way coin to currency: 
    ///    a. sell crypto@exchange1 to fiat1, 
    ///    b. buy crypto@exchange2 using fiat2 
    ///    c. take profit of price delta and fiat exchange rates
    /// 2. one way coin to coin c1_c2: 
    ///    a. sell symbol@exchange1: c1 -> c2
    ///    b. buy symbol@exchange2:  c2 -> c1 
    ///    c. vice versa 
    ///    d. take profit of price delta between two exchs
    ///    e. end result: c1: exch1 -> exch2, c2: exch2 -> exch1
    /// 3. two way coin to currency: 
    ///    1) buy  crypto1 in fiat1
    ///    2) sell crypto1 in fiat2
    ///    3) buy  crypto2 in fiat2
    ///    4) sell crypto2 in fiat1
    ///    5) end result:
    ///       a. crypto1: exchange2 -> exchange1, sum not changed
    ///       b. crypto2: exchange2 -> exchange1, sun not changed 
    ///       c. fiat2: no change; 
    ///       d. fiat1 should increase. 
    /// 4. two way coin to coin
    ///    symbol1 = c1_c2, symbol2 = c3_c4
    ///    1. buy  c1 using c2 at exch1
    ///    2. sell c1 for   c2 at exch2
    ///    3. buy  c3 using c4 at exch2
    ///    4. sell c3 for   c4 at exch1
    ///    5. end result
    ///       a. c1: exch2 -> exch1
    ///       b. c2: exch1 -> exch2
    ///       c. c3: exch1 -> exch2
    ///       d. c4: exch2 -> exch1
    public class ProfitCalculator : IProfitCalculator
    {
        ITickerStore _store;
        IProductMeta _meta;
        Dictionary<string, IMarket> _markets;
        //double _tradefee = 0.002;
        //Dictionary<string, Dictionary<string, double>> _exchangeRates;

        //ConcurrentDictionary<string, ConcurrentDictionary<string, ConcurrentDictionary<string, double>>> _profitOneWayCoinToCoin;                                   //_profitOneWayCoinToCoin[exch1][exch2][symbol]: buy symbol@exch1 and sell@exch2
        //ConcurrentDictionary<string, ConcurrentDictionary<string, ConcurrentDictionary<string, double>>> _profitOneWayCoinToCurrency;                               //_profitCoinToCurrency[exch1][exch2][crypto]: buy crypto@exch1 and sell@exchn2
        //ConcurrentDictionary<string, ConcurrentDictionary<string, ConcurrentDictionary<string, ConcurrentDictionary<string, double>>>> _profitTwowayCoinToCurrency; //_profitTwowayCoinToCurrency[exch1][exch2][crypt1[crypt2]: buy crypt1@exch1 and buy crypt2@exch2
        //ConcurrentDictionary<string, ConcurrentDictionary<string, ConcurrentDictionary<string, ConcurrentDictionary<string, double>>>> _profitTwowayCoinToCoin;
        private bool _isComputeProfitBusy;

        private List<(List<string> symbols, string srcExch, string dstExch)> _onewayCoinToCoin;
        public event CryptoCommon.EventHandlers.OneWayCoinToCurrencyProfitCalculatedEventHandler OnOneWayCoinToCurrencyProfitCalculated;
        public event CryptoCommon.EventHandlers.OneWayCoinToCoinProfitCalculatedEventHandler OnOneWayCoinToCoinProfitCalculated;
        public event CryptoCommon.EventHandlers.TwoWayCoinToCurrencyProfitCalculatedEventHandler OnTwoWayCoinToCurrencyProfitCalculated;
        public event CryptoCommon.EventHandlers.TwoWayCoinToCoinProfitCalculatedEventHandler OnTwoWayCoinToCoinProfitCalculated;

        public ProfitCalculator(ITickerStore store, IProductMeta meta, Dictionary<string, IMarket> markets,
            Dictionary<string, Dictionary<string, List<string>>> toComputeOnewaycoinToCoin = null)
            //Dictionary<string, Dictionary<string, double>> exchangeRate,
            //Dictionary<string, Dictionary<string, List<string>>> toComputeOnewayCoinToCurrency,
            //Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>> toComputeTwowayCoinToCurrency,
            //Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>> toComputeTwowayCoinToCoin = null)
        {
            _store = store;
            _meta = meta;
            _markets = markets;

            foreach (var exch in _markets.Keys)
            {
                //_apiMarketSeconds[exch].OnTickerReceived += ProfitCalculator_OnTickerReceived;
                _markets[exch].OnTickerListReceived += ProfitCalculator_OnTickerListReceived;
            }

            /////////////////////////////////////////////////////////////////////////////////////////////////
            /////one way coin to currency
            //_profitOneWayCoinToCurrency = new ConcurrentDictionary<string, ConcurrentDictionary<string, ConcurrentDictionary<string, double>>>();
            //foreach (var exch1 in toComputeOnewayCoinToCurrency.Keys)
            //{
            //    _profitOneWayCoinToCurrency.TryAdd(exch1, new ConcurrentDictionary<string, ConcurrentDictionary<string, double>>());
            //    foreach (var exch2 in toComputeOnewayCoinToCurrency[exch1].Keys)
            //    {
            //        _profitOneWayCoinToCurrency[exch1].TryAdd(exch2, new ConcurrentDictionary<string, double>());
            //        foreach (var crypto in toComputeOnewayCoinToCurrency[exch1][exch2])
            //            _profitOneWayCoinToCurrency[exch1][exch2].TryAdd(crypto, -1);
            //    }
            //}

            /////////////////////////////////////////////////////////////////////////////////////////////////
            /////two way coin to currency
            //_profitTwowayCoinToCurrency = new ConcurrentDictionary<string, ConcurrentDictionary<string, ConcurrentDictionary<string, ConcurrentDictionary<string, double>>>>();
            //foreach (var exch1 in toComputeTwowayCoinToCurrency.Keys)
            //{
            //    _profitTwowayCoinToCurrency.TryAdd(exch1, new ConcurrentDictionary<string, ConcurrentDictionary<string, ConcurrentDictionary<string, double>>>());
            //    foreach (var exch2 in toComputeTwowayCoinToCurrency[exch1].Keys)
            //    {
            //        _profitTwowayCoinToCurrency[exch1].TryAdd(exch2, new ConcurrentDictionary<string, ConcurrentDictionary<string, double>>());
            //        foreach (var crypto1 in toComputeTwowayCoinToCurrency[exch1][exch2].Keys)
            //        {
            //            _profitTwowayCoinToCurrency[exch1][exch2].TryAdd(crypto1, new ConcurrentDictionary<string, double>());
            //            foreach (var crypto2 in toComputeTwowayCoinToCurrency[exch1][exch2][crypto1])
            //                _profitTwowayCoinToCurrency[exch1][exch2][crypto1].TryAdd(crypto2, -1);
            //        }
            //    }
            //}

            /////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //////two way coin to coin
            //_profitTwowayCoinToCoin = new ConcurrentDictionary<string, ConcurrentDictionary<string, ConcurrentDictionary<string, ConcurrentDictionary<string, double>>>>();
            //if (toComputeTwowayCoinToCoin != null)
            //{
            //    foreach (var exch1 in toComputeTwowayCoinToCoin.Keys)
            //    {
            //        _profitTwowayCoinToCoin.TryAdd(exch1, new ConcurrentDictionary<string, ConcurrentDictionary<string, ConcurrentDictionary<string, double>>>());
            //        foreach (var exch2 in toComputeTwowayCoinToCoin[exch1].Keys)
            //        {
            //            _profitTwowayCoinToCoin[exch1].TryAdd(exch2, new ConcurrentDictionary<string, ConcurrentDictionary<string, double>>());
            //            foreach (var crypto1 in toComputeTwowayCoinToCoin[exch1][exch2].Keys)
            //            {
            //                _profitTwowayCoinToCoin[exch1][exch2].TryAdd(crypto1, new ConcurrentDictionary<string, double>());
            //                foreach (var crypto2 in toComputeTwowayCoinToCoin[exch1][exch2][crypto1])
            //                    _profitTwowayCoinToCoin[exch1][exch2][crypto1].TryAdd(crypto2, -1);
            //            }
            //        }
            //    }
            //}

            ///////////////////////////////////////////////////////////////////////////////////////////////////////////
            ///One way coin to coin
            if (toComputeOnewaycoinToCoin == null)
            {
                _onewayCoinToCoin = new List<(List<string> symbols, string srcExch, string dstExch)>();

                //toComputeOnewaycoinToCoin = new Dictionary<string, Dictionary<string, List<string>>>();
                var list = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("Okex", "Bittrex"),
                    new KeyValuePair<string, string>("Okex", "Kraken" ),
                    new KeyValuePair<string, string>("Bittrex", "Kraken")
                };

                foreach (var k in list)
                {
                    var exch1 = k.Key;
                    var exch2 = k.Value;
                    if (!_markets.ContainsKey(exch1) || !_markets.ContainsKey(exch2))
                        continue;

                    //toComputeOnewaycoinToCoin.Add(exch1, new Dictionary<string, List<string>>());
                    //toComputeOnewaycoinToCoin[exch1].Add(exch2, new List<string>());

                    var lst1 = _meta.GetStandardSymbolsForExchange(exch1);
                    var lst2 = _meta.GetStandardSymbolsForExchange(exch2);
                    var symbols = lst1.Where(s => lst2.Contains(s)).ToList();

                    //toComputeOnewaycoinToCoin[exch1][exch2] = symbols;
                    _onewayCoinToCoin.Add((symbols, exch1, exch2));
                }
            }

            //_profitOneWayCoinToCoin = new ConcurrentDictionary<string, ConcurrentDictionary<string, ConcurrentDictionary<string, double>>>();
            //if (toComputeOnewaycoinToCoin != null)
            //{
            //    foreach (var exch1 in toComputeOnewaycoinToCoin.Keys)
            //    {
            //        _profitOneWayCoinToCoin.TryAdd(exch1, new ConcurrentDictionary<string, ConcurrentDictionary<string, double>>());
            //        foreach (var exch2 in toComputeOnewaycoinToCoin[exch1].Keys)
            //        {
            //            _profitOneWayCoinToCoin[exch1].TryAdd(exch2, new ConcurrentDictionary<string, double>());
            //            foreach (var symbol in toComputeOnewaycoinToCoin[exch1][exch2])
            //            {
            //                _profitOneWayCoinToCoin[exch1][exch2].TryAdd(symbol, -1);
            //            }
            //        }
            //    }
            //}

            _store.OnTickerUpdated += _store_OnPriceUpdated;
        }

        private void _store_OnPriceUpdated(object sender, string exchange, Ticker oldtick, Ticker newtick)
        {
            lock (this)
            {
                if (_isComputeProfitBusy) return;
                _isComputeProfitBusy = true;
            }

            var symbol = oldtick.Symbol;

            try
            {
                //one way coin to coin
                if (_onewayCoinToCoin != null)
                {
                    foreach (var t in _onewayCoinToCoin)
                    {
                        if (t.srcExch != exchange && t.dstExch != exchange) continue;
                        foreach (var s in t.symbols)
                        {
                            if (s == symbol)
                            {
                                var p = this.ComputeOneWayCoinToCoinProfit(t.srcExch, t.dstExch, s, _meta.TradingFee[t.srcExch], _meta.TradingFee[t.dstExch]);
                                if (p.profitBuyAtExch1.HasValue)
                                {
                                    //_profitOneWayCoinToCoin[exch1][exch2][symbol] = p.Value;
                                    this.OnOneWayCoinToCoinProfitCalculated?.Invoke(this, t.srcExch, t.dstExch, s, p.profitBuyAtExch1.Value, p.profitSellAtExch1.Value, p.tick1, p.tick2);
                                }
                            }
                        }
                    }
                }

                ////two way coin to currency
                //var c = ProductMeta.GetCurrenciesFromStandardSymbol(symbol);
                //foreach (var exch1 in _profitTwowayCoinToCurrency.Keys)
                //{
                //    foreach (var exch2 in _profitTwowayCoinToCurrency[exch1].Keys)
                //    {
                //        if (exch1 != exchange && exch2 != exchange) continue;
                //        foreach (var crypto1 in _profitTwowayCoinToCurrency[exch1][exch2].Keys)
                //        {
                //            foreach (var crypto2 in _profitTwowayCoinToCurrency[exch1][exch2][crypto1].Keys)
                //            {
                //                if (crypto1 != c.crypto && crypto2 != c.crypto) continue;
                //                var p = this.ComputeTwowayCoinToCurrencyProfit(exch1, crypto1, exch2, crypto2);

                //                if (p.HasValue)
                //                {
                //                    _profitTwowayCoinToCurrency[exch1][exch2][crypto1][crypto2] = p.Value;
                //                    this.OnTwoWayCoinToCurrencyProfitCalculated?.Invoke(this, exch1, crypto1, exch2, crypto2, p.Value);
                //                }
                //            }
                //        }
                //    }
                //}

                ////two way coin to coin
                //foreach (var exch1 in _profitTwowayCoinToCoin.Keys)
                //{
                //    foreach (var exch2 in _profitTwowayCoinToCoin[exch1].Keys)
                //    {
                //        if (exch1 != exchange && exch2 != exchange) continue;
                //        foreach (var symbol1 in _profitTwowayCoinToCoin[exch1][exch2].Keys)
                //        {
                //            foreach (var symbol2 in _profitTwowayCoinToCoin[exch1][exch2][symbol1].Keys)
                //            {
                //                if (symbol1 != symbol && symbol2 != symbol) continue;
                //                var p = this.ComputeTwowayCoinToCoinProfit(exch1, symbol1, exch2, symbol2);

                //                if (p.HasValue)
                //                {
                //                    _profitTwowayCoinToCoin[exch1][exch2][symbol1][symbol2] = p.Value;
                //                    this.OnTwoWayCoinToCoinProfitCalculated?.Invoke(this, exch1, symbol1, exch2, symbol2, p.Value);
                //                }
                //            }
                //        }
                //    }
                //}

                ////one way coin to currency
                //foreach (var exch1 in _profitOneWayCoinToCurrency.Keys)
                //{
                //    foreach (var exch2 in _profitOneWayCoinToCurrency[exch1].Keys)
                //    {
                //        if (exch1 != exchange && exch2 != exchange) continue;
                //        if (!_profitOneWayCoinToCurrency[exch1][exch2].ContainsKey(c.crypto)) continue;
                //        var p = this.ComputeOneWayCoinToCurrencyProfit(exch1, exch2, c.crypto, _tradefee);

                //        if (p.HasValue)
                //        {
                //            _profitOneWayCoinToCurrency[exch1][exch2][c.crypto] = p.Value;
                //            this.OnOneWayCoinToCurrencyProfitCalculated?.Invoke(this, exch1, exch2, c.crypto, p.Value);
                //        }
                //    }
                //}

                //foreach (var exch1 in _profitOneWayCoinToCoin.Keys)
                //{
                //    foreach (var exch2 in _profitOneWayCoinToCoin[exch1].Keys)
                //    {
                //        if (exch1 != exchange && exch2 != exchange) continue;
                //        if (!_profitOneWayCoinToCoin[exch1][exch2].ContainsKey(symbol)) continue;

                //        var p = this.ComputeOneWayCoinToCoinProfit(exch1, exch2, symbol, _tradefee);
                //        if (p.profitBuyAtExch1.HasValue)
                //        {
                //            //_profitOneWayCoinToCoin[exch1][exch2][symbol] = p.Value;
                //            this.OnOneWayCoinToCoinProfitCalculated?.Invoke(this, exch1, exch2, symbol, p.profitBuyAtExch1.Value, p.profitSellAtExch1.Value, p.tick1, p.tick2);
                //        }
                //    }
                //}
            }
            finally
            {
                _isComputeProfitBusy = false;
            }
        }

        /// <summary>
        /// buy symbol@exchange1 and sell@exchange2 or sell symbol@exchange1 and buy@exchange2
        /// </summary>
        /// <param name="exchange1"></param>
        /// <param name="exchange2"></param>
        /// <param name="symbol"></param>
        /// <returns></returns>
        private (double? profitBuyAtExch1, double? profitSellAtExch1, Ticker tick1, Ticker tick2) ComputeOneWayCoinToCoinProfit(string exchange1, string exchange2, string symbol, double fee1, double fee2)
        {
            var ticker_exch1 = _store.GetTicker(exchange1, symbol);
            var ticker_exch2 = _store.GetTicker(exchange2, symbol);
            if (ticker_exch1 == null || ticker_exch2 == null) return (null, null, ticker_exch1, ticker_exch2);

            return ComputeOneWayCoinToCoinProfit(ticker_exch1, ticker_exch2, fee1, fee2);
        }

        public static (double? profitBuyAtExch1, double? profitSellAtExch1, Ticker tick1, Ticker tick2) ComputeOneWayCoinToCoinProfit(Ticker ticker_exch1, Ticker ticker_exch2, double tradefee1, double tradefee2)
        {
            var rate = (1 - tradefee1) * (1 - tradefee2);
            return (rate * ticker_exch2.Bid / ticker_exch1.Ask, rate * ticker_exch1.Bid / ticker_exch2.Ask, ticker_exch1, ticker_exch2);
        }

        ///// <summary>
        ///// 1. sell crypto at exchange1 to fiat1 at bid price: fiat1 = c * bid1, convert fiat1 to fiat2 = c * bid1 * exchrate = gain
        ///// 2. buy crypto at exchange2 using fiat2 at ask price: fiat2 = c / ask2 = cost
        ///// 3. profit rate = gain / cost = bid1 * exchrate / ask2
        ///// 4. if considering trade commission and cost of transfer, we multiply a factor
        ///// </summary>
        ///// <param name="exchange1"></param>
        ///// <param name="exchange2"></param>
        ///// <param name="crypto"></param>
        ///// <param name="tradefee"></param>
        ///// <returns></returns>
        //private double? ComputeOneWayCoinToCurrencyProfit(string exchange1, string exchange2, string crypto, double tradefee)
        //{
        //    var symbol_exch1 = ProductMeta.ConvertCryptoToStandardSymbol(exchange1, crypto);
        //    var symbol_exch2 = ProductMeta.ConvertCryptoToStandardSymbol(exchange2, crypto);

        //    var ticker_exch1 = _store.GetTicker(exchange1, symbol_exch1);
        //    var ticker_exch2 = _store.GetTicker(exchange2, symbol_exch2);
        //    if (ticker_exch1 == null || ticker_exch2 == null) return null;

        //    return Math.Pow(1 - tradefee, 2) * ticker_exch2.Bid / ticker_exch1.Ask * _exchangeRates[exchange1][exchange2];
        //}

        /// <summary>
        /// 1) buy  crypto1 in fiat1
        /// 2) sell crypto1 in fiat2
        /// 3) buy  crypto2 in fiat2
        /// 4) sell crypto2 in fiat1
        /// 5) end result:
        ///    a. crypto1: exchange2 -> exchange1, sum not changed
        ///    b. crypto2: exchange2 -> exchange1, sun not changed 
        ///    c. fiat2: no change; 
        ///    d. fiat1 should increase. 
        /// </summary>
        /// <param name="crypto1"></param>
        /// <param name="crypto2"></param>
        /// <returns></returns>
        private double? ComputeTwowayCoinToCurrencyProfit(string exchange1, string crypto1, string exchange2, string crypto2)
        {
            if (crypto2 == crypto1) return null;

            var symbol1_exch1 = ProductMeta.ConvertCryptoToStandardSymbol(exchange1, crypto1); //crypto1_fiat1
            var symbol1_exch2 = ProductMeta.ConvertCryptoToStandardSymbol(exchange2, crypto1); //crypto1_fiat2
            var ticker1_exch1 = _store.GetTicker(exchange1, symbol1_exch1);
            var ticker1_exch2 = _store.GetTicker(exchange2, symbol1_exch2);
            if (ticker1_exch1 == null || ticker1_exch2 == null) return null;

            var symbol2_exch1 = ProductMeta.ConvertCryptoToStandardSymbol(exchange1, crypto2); //crypto2_fiat1
            var symbol2_exch2 = ProductMeta.ConvertCryptoToStandardSymbol(exchange2, crypto2); //crypto2_fiat2
            var ticker2_exch1 = _store.GetTicker(exchange1, symbol2_exch1);
            var ticker2_exch2 = _store.GetTicker(exchange2, symbol2_exch2);
            if (ticker2_exch1 == null || ticker2_exch2 == null) return null;

            return ComputeTwowayProfit(ticker1_exch1, ticker1_exch2, ticker2_exch1, ticker2_exch2, _meta.TradingFee[exchange1], _meta.TradingFee[exchange2]);
        }

        /// <summary>
        /// symbol1 = c1_c2, symbol2 = c3_c4
        /// 1. buy  c1 using c2 at exch1
        /// 2. sell c1 for   c2 at exch2
        /// 3. buy  c3 using c4 at exch2
        /// 4. sell c3 for   c4 at exch1
        /// 5. end result
        ///    a. c1: exch2 -> exch1
        ///    b. c2: exch1 -> exch2
        ///    c. c3: exch1 -> exch2
        ///    d. c4: exch2 -> exch1
        /// </summary>
        /// <param name="exchange1"></param>
        /// <param name="symbol1"></param>
        /// <param name="exchange2"></param>
        /// <param name="symbol2"></param>
        /// <returns></returns>
        private double? ComputeTwowayCoinToCoinProfit(string exchange1, string symbol1, string exchange2, string symbol2, double fee1, double fee2)
        {
            if (symbol2 == symbol1) return null;

            var ticker1_exch1 = _store.GetTicker(exchange1, symbol1);
            var ticker1_exch2 = _store.GetTicker(exchange2, symbol1);
            if (ticker1_exch1 == null || ticker1_exch2 == null) return null;

            var ticker2_exch1 = _store.GetTicker(exchange1, symbol2);
            var ticker2_exch2 = _store.GetTicker(exchange2, symbol2);
            if (ticker2_exch1 == null || ticker2_exch2 == null) return null;

            return ComputeTwowayProfit(ticker1_exch1, ticker1_exch2, ticker2_exch1, ticker2_exch2, fee1, fee2);
        }

        public static double ComputeTwowayProfit(Ticker ticker1_exch1, Ticker ticker1_exch2, Ticker ticker2_exch1, Ticker ticker2_exch2, double fee1, double fee2)
        {
            var r1 = (ticker1_exch2.Bid / ticker1_exch1.Ask);   //buy at exch1 and sell at exch2
            var r2 = (ticker2_exch1.Bid / ticker2_exch2.Ask);   //buy at exch2 and sell at exch1
            var profit = (1 - fee1) * (1 - fee1) * (1 - fee2) * (1 - fee2) * r1 * r2;   //take fee into account
            return profit;
        }


        private void ProfitCalculator_OnTickerListReceived(object sender, string exchange, List<Ticker> ticker)
        {
            foreach (var t in ticker)
                _store.AddTicker(exchange, t);
        }
    }
}
