using CryptoCommon.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoCommon.Models
{
    public class ProductMeta : IProductMeta
    {
        private Dictionary<string, Dictionary<string, string>> _standardCurrencyToExchangeCurrency;
        //private Dictionary<string, Dictionary<string, string>> _altNameToExchangeSymbol;

        //public static Dictionary<string, string> OkBaseUrl = new Dictionary<string, string>
        //{
        //    { "OkCoin", "https://www.okcoin.cn/api/v1/" },
        //    { "Okex", "https://www.okex.com/api/v1/" }
        //};
        //public Dictionary<string, string> BaseUrl { get; set; }

        public Dictionary<string, double> TradingFee { get; set; }
        public Dictionary<string, Dictionary<string, string>> API { get; set; }
        public Dictionary<string, List<string>> SymbolToCapture { get; set; }

        public Dictionary<string, Dictionary<string, Dictionary<string, string>>> Address { get; set; }
        public Dictionary<string, string> ExchangeToFiat { get; set; }
        public Dictionary<string, string> ExchangeCurrency { get; set; }
        public Dictionary<string, Dictionary<string, string>> ExchangeRate { get; set; }
        public Dictionary<string, Dictionary<string, List<string>>> OneWayCoinToCurrencyTakeProfit { get; set; }
        public Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>> TwoWayCoinToCoinTakeProfit { get; set; }
        public Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>> TwoWayCoinToCurrencyTakeProfit { get; set; }
        public Dictionary<string, Dictionary<string, List<string>>> OneWayCoinToCoinTakeProfit { get; set; }
        public Dictionary<string, Dictionary<string, string>> ExchangeCurrencyToStandard { get; set; }
        public Dictionary<string, Dictionary<string, string>> ExchangeSymbolToStandard { get; set; }
        public Dictionary<string, Dictionary<string, double>> MinMaxTradingAmount { get; private set; } = new Dictionary<string, Dictionary<string, double>>();
        public Dictionary<string, Dictionary<string, string>> minMaxTradingAmount { get; set; }
        Dictionary<string, Dictionary<string, string>> _standardSymbolToExchangeSymbol;

        public Dictionary<string, Dictionary<string, Dictionary<string, double>>> WithDrawCost { get; set; }
        public List<string> Exchanges { get { return _standardSymbolToExchangeSymbol == null? null : _standardSymbolToExchangeSymbol.Keys.ToList(); } }
        
        public ProductMeta()
        {
        }
        public ProductMeta(string fileProjectMeta)
        {
            this.LoadFromJsonFile(fileProjectMeta);
        }

        void LoadFromJsonFile(string filename)
        {
            var str = File.ReadAllText(filename);
            var m = JsonConvert.DeserializeObject<ProductMeta>(str);
            this.TwoWayCoinToCoinTakeProfit = m.TwoWayCoinToCoinTakeProfit;
            this.API = m.API;
            this.SymbolToCapture = m.SymbolToCapture;
            this.WithDrawCost = m.WithDrawCost;
            this.ExchangeCurrency = m.ExchangeCurrency;
            this.ExchangeRate = m.ExchangeRate;
            this.OneWayCoinToCoinTakeProfit = m.OneWayCoinToCoinTakeProfit;
            this.OneWayCoinToCurrencyTakeProfit = m.OneWayCoinToCurrencyTakeProfit;
            this.TwoWayCoinToCurrencyTakeProfit = m.TwoWayCoinToCurrencyTakeProfit;
            this.Address = m.Address;
            this.ExchangeToFiat = m.ExchangeToFiat;
            this.ExchangeCurrencyToStandard = m.ExchangeCurrencyToStandard;
            this.ExchangeSymbolToStandard = m.ExchangeSymbolToStandard;
            this.TradingFee = m.TradingFee;
            this.minMaxTradingAmount = m.minMaxTradingAmount;

            foreach (var k1 in this.minMaxTradingAmount.Keys)
            {
                this.MinMaxTradingAmount.Add(k1, new Dictionary<string, double>());
                foreach (var k2 in this.minMaxTradingAmount[k1].Keys)
                    this.MinMaxTradingAmount[k1].Add(k2, double.Parse(this.minMaxTradingAmount[k1][k2]));
            }

            _standardCurrencyToExchangeCurrency = new Dictionary<string, Dictionary<string, string>>();
            foreach (var key in this.ExchangeCurrencyToStandard.Keys)
            {
                var d = new Dictionary<string, string>();
                foreach (var kv in this.ExchangeCurrencyToStandard[key])
                    if (!d.ContainsKey(kv.Value)) d.Add(kv.Value, kv.Key);
                _standardCurrencyToExchangeCurrency.Add(key, d);
            }

            _standardSymbolToExchangeSymbol = new Dictionary<string, Dictionary<string, string>>();
            foreach (var key in this.ExchangeSymbolToStandard.Keys)
            {
                _standardSymbolToExchangeSymbol.Add(key, new Dictionary<string, string>());

                var d = this._standardSymbolToExchangeSymbol[key];

                foreach (var kv in this.ExchangeSymbolToStandard[key])
                {
                    if (!d.ContainsKey(kv.Value))
                        d.Add(kv.Value, kv.Key);
                    else
                        Console.WriteLine($"{kv.Value}:{kv.Key} exist already with value = {d[kv.Value]}");
                }
                //_standardSymbolToExchangeSymbol.Add(key, this.ExchangeSymbolToStandard[key].ToDictionary(kv => kv.Value, kv => kv.Key));
            }
        }

        public string ConvertExchangeSymbolToStandardSymbol(string exchange, string exchangeSymbol)
        {
            //if (!this.SymbolToStandard.ContainsKey(exchange)) return null;
            //if (!this.SymbolToStandard[exchange].ContainsKey(exchangeSymbol)) return null;
            if (this.ExchangeSymbolToStandard[exchange].ContainsKey(exchangeSymbol))
                return this.ExchangeSymbolToStandard[exchange][exchangeSymbol];
            else
                return exchangeSymbol;
        }

        public string ConvertStandardSymbolToExchangeSymbol(string exchange, string standardSymbol)
        {
            //if (!this._standardSymbolToExchangeSymbol.ContainsKey(exchange)) return null;
            //if (!this._standardSymbolToExchangeSymbol[exchange].ContainsKey(standardSymbol)) return null;
            if (this._standardSymbolToExchangeSymbol[exchange].ContainsKey(standardSymbol))
                return this._standardSymbolToExchangeSymbol[exchange][standardSymbol];
            else
                return standardSymbol;
        }

        public static (string fiat, string crypto) GetCurrenciesFromStandardSymbol(string standardSymbol)
        {
            var items = standardSymbol.Split('_');
            return (items[1], items[0]);
        }

        public static (string assetName1, string assetName2) GetAssetNamesFromStandardSymbol(string standardSymbol)
        {
            var items = standardSymbol.Split('_');
            return (items[0], items[1]);
        }

        public static (string exchange, string fiat, string crypto) GetCurrenciesFromUniqueSymbol(string uniqueSymbol)
        {
            var items = uniqueSymbol.Split('_');
            return (items[0], items[2], items[1]);
        }

        public static (string exchange, string asset1, string asset2) SplitUniqueSymbol(string uniqueSymbol)
        {
            var items = uniqueSymbol.Split('_');
            return (items[0], items[1], items[2]);
        }
        public static string GetUniqueSymbolFromStandardSymbol(string exchange, string standardSymbol) 
        {
            return exchange + "_" + standardSymbol;
        }

        public string GetUniqueSymbolFromExchangeSymbol(string exchange, string exchangeSymbol)
        {
            var standardSymbol = this.ConvertExchangeSymbolToStandardSymbol(exchange, exchangeSymbol);
            return string.IsNullOrEmpty(standardSymbol) ? null : string.Format("{0}_{1}", exchange, standardSymbol);
        }

        public string ConvertExchangeCurrencyToStandardCurrency(string exchange, string exchagneCurrency)
        {
            if (!this.ExchangeCurrencyToStandard.ContainsKey(exchange) || !this.ExchangeCurrencyToStandard[exchange].ContainsKey(exchagneCurrency))
                return exchagneCurrency;  //by default exchagneCurrency == standardCurrency
            else
                return this.ExchangeCurrencyToStandard[exchange][exchagneCurrency];
        }

        public string ConvertStandardCurrencyToExchangeCurrency(string exchange, string standardCurrency)
        {
            if (!_standardCurrencyToExchangeCurrency.ContainsKey(exchange) || !_standardCurrencyToExchangeCurrency[exchange].ContainsKey(standardCurrency))
                return standardCurrency; //by default exchagneCurrency == standardCurrency
            else
                return _standardCurrencyToExchangeCurrency[exchange][standardCurrency];
        }

        //public string ConvertExchangeAltNameToExchangeSymbol(string exchange, string altName)
        //{
        //    return _altNameToExchangeSymbol[exchange][altName];
        //}

        //public string ConvertExchangeAltNameToStandardSymbol(string exchange, string altName)
        //{
        //    var exchangeSymbol = _altNameToExchangeSymbol[exchange][altName];
        //    return this.ExchangeSymbolToStandard[exchange][exchangeSymbol];
        //}

        public List<string> GetExchangeSymbolsForExchange(string exchange)
        {
            return this.ExchangeSymbolToStandard[exchange].Keys.ToList();
        }

        public List<string> GetStandardSymbolsForExchange(string exchange)
        {
            return this.ExchangeSymbolToStandard[exchange].Values.ToList();
        }

        public string GetStandardSymbolFromStandardCurrency(string exchange, string currency)
        {
            //return CNY for CNY, EUR for EUR
            if (this.ExchangeToFiat[exchange] == currency)
                return currency;

            return string.Format("{0}_{1}", currency, this.ExchangeToFiat[exchange]);
        }

        public string GetTransferAddress(string srcexchange, string dstexchange, string crypto)
        {
            if (!this.Address.ContainsKey(srcexchange) || !this.Address[srcexchange].ContainsKey(dstexchange) || !this.Address[srcexchange][dstexchange].ContainsKey(crypto))
                return null;

            return this.Address[srcexchange][dstexchange][crypto];
        }

        public static string ConvertCryptoToStandardSymbol(string exchange, string crypto)
        {
            if (exchange == "Kraken") return string.Format("{0}_EUR", crypto);
            if (exchange == "OkCoin") return string.Format("{0}_CNY", crypto);
            return null;
        }

        public Dictionary<string, Dictionary<string, double>> GetExchangeRate()
        {
            return this.ExchangeRate.ToDictionary(kv1 => kv1.Key, kv => kv.Value.ToDictionary(kv2 => kv2.Key, kv2 => double.Parse(kv2.Value)));
        }

        public Dictionary<string, Dictionary<string, List<string>>> GetOneWayCoinToCurrencyTakeProfit()
        {
            return this.OneWayCoinToCurrencyTakeProfit;
        }

        public Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>> GetTwoWayCoinToCurrencyTakeProfit()
        {
            return this.TwoWayCoinToCurrencyTakeProfit;
        }

        public string GetCurrencyForExchange(string exchange)
        {
            if (!this.ExchangeCurrency.ContainsKey(exchange)) return null;
            return this.ExchangeCurrency[exchange];
        }

        public bool CheckIfTakeProfitOnewayCoinToCurrency(string exchange1, string exchange2, string crypto)
        {
            if (exchange1 == exchange2 || string.IsNullOrEmpty(exchange1) || string.IsNullOrEmpty(exchange2) || string.IsNullOrEmpty(crypto))
                return false;
            var d = this.OneWayCoinToCurrencyTakeProfit;
            return (d.ContainsKey(exchange1) &&
                    d[exchange1].ContainsKey(exchange2) &&
                    d[exchange1][exchange2].Contains(crypto));
        }

        public bool CheckIfTakeProfitTwowayCoinToCurrency(string exchange1, string crypto1, string exchange2, string crypto2)
        {
            if (exchange1 == exchange2 || crypto1 == crypto2 || string.IsNullOrEmpty(exchange1) || string.IsNullOrEmpty(exchange2) || string.IsNullOrEmpty(crypto1) || string.IsNullOrEmpty(crypto2))
                return false;

            var d = this.TwoWayCoinToCurrencyTakeProfit;
            return (d.ContainsKey(exchange1) &&
                    d[exchange1].ContainsKey(exchange2) &&
                    d[exchange1][exchange2].ContainsKey(crypto1) &&
                    d[exchange1][exchange2][crypto1].Contains(crypto2));
        }

        public bool CheckIfTakeProfitTwowayCoinToCoin(string exchange1, string symbol1, string exchange2, string symbol2)
        {
            if (exchange1 == exchange2 || symbol1 == symbol2 || string.IsNullOrEmpty(exchange1) || string.IsNullOrEmpty(exchange2) || string.IsNullOrEmpty(symbol1) || string.IsNullOrEmpty(symbol2))
                return false;

            var d = this.TwoWayCoinToCoinTakeProfit;
            return (d.ContainsKey(exchange1) &&
                    d[exchange1].ContainsKey(exchange2) &&
                    d[exchange1][exchange2].ContainsKey(symbol1) &&
                    d[exchange1][exchange2][symbol1].Contains(symbol2));
        }

        public bool CheckIfTakeProfitOnewayCoinToCoin(string exchange1, string exchange2, string symbol)
        {
            if (exchange1 == exchange2 || string.IsNullOrEmpty(exchange1) || string.IsNullOrEmpty(exchange2) || string.IsNullOrEmpty(symbol))
                return false;

            var d = this.OneWayCoinToCoinTakeProfit;
            return ((d.ContainsKey(exchange1) && d[exchange1].ContainsKey(exchange2) && d[exchange1][exchange2].Contains(symbol)) ||
                    (d.ContainsKey(exchange2) && d[exchange2].ContainsKey(exchange1) && d[exchange2][exchange1].Contains(symbol)));
        }

        public double GetWithdrawCost(string srcExchange, string dstExchagne, string crypto)
        {
            //return large default large withdraw cost
            if (!this.WithDrawCost.ContainsKey(srcExchange) || !this.WithDrawCost[srcExchange].ContainsKey(dstExchagne) || !this.WithDrawCost[srcExchange][dstExchagne].ContainsKey(crypto))
                return 0.1;

            return this.WithDrawCost[srcExchange][dstExchagne][crypto];
        }

        public static double GetMinimumBalance(string crypto)
        {
            switch (crypto)
            {
                case "BTC":
                    return 0.1;
                case "LTC":
                    return 4;
                case "ETC":
                    return 15;
                case "ETH":
                    return 1;
                case "BCC":
                    return 1;
                default:
                    return 0;
            }
        }

        public static double GetMinimumAmountToTransfer(string crypto)
        {
            switch (crypto)
            {
                case "LTC":
                    return 5;
                case "ETC":
                    return 10;
                case "BTC":
                    return 1;
                case "ETH":
                    return 5;
                case "BCC":
                    return 5;
                default:
                    return double.MaxValue;
            }
        }

        public static double GetMinimumAmountToTrade(string crypto)
        {
            switch (crypto)
            {
                case "LTC":
                    return 1;
                case "ETC":
                    return 4;
                case "BTC":
                    return 0.015;
                case "ETH":
                    return 0.15;
                case "BCC":
                    return 0.1;
                default:
                    return double.MaxValue;
            }
        }

        //public static (double maxValue, double minProfit, double minProfitRatio, double maxLost) GetMaxBTCValueToTrade(string exchange)
        //{
        //    switch (exchange)
        //    {
        //        case "OkCoin":
        //            return (1000, 10, 0.01, 0);
        //        case "Kraken":
        //            return (130, 1.3, 0.01, 0);
        //        default:
        //            return (-1, -1, -1, -1);
        //    }
        //}

        public static (double maxValue, double minProfit, double minProfitRatio, double maxLost) GetMaxValueToTrade(string exchange, string symbol=null)
        {
            if (string.IsNullOrEmpty(symbol))
            {
                switch (exchange)
                {
                    case "OkCoin":
                        return (1000, 10, 0.01, 0);
                    case "Kraken":
                        return (130, 1.3, 0.01, 0);
                    default:
                        return (-1, -1, -1, -1);
                }
            }
            else
            {
                var asset = GetAssetNamesFromStandardSymbol(symbol).assetName2;
                switch(asset)
                {
                    case "BTC":
                        return (0.03, 0.0003, 0.01, 0);
                    case "ETH":
                        return (0.5, 0.005, 0.01, 0);
                    default:
                        return (-1, -1, -1, -1);
                }
            }
        }

        public static double ApproximateToDigits(double value, int numDigits, bool isRoundToLower=true)
        {
            if (isRoundToLower)
            {
                var m = Math.Pow(10, numDigits);
                return (int)(value * m) / m;
            }
            else
            {
                var m1 = Math.Pow(10, numDigits);
                var m2 = Math.Pow(10, numDigits + 1);
                var r1 = (int)(value * m2) % 10;
                if (r1 == 0)
                    return (int)(value * m2) / m2;
                else
                    return ((int)(value * m2) - r1 + 10) / m2;
            }
        }
    }
}
