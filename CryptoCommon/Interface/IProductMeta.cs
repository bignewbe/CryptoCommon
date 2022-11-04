using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoCommon.Interfaces
{
    public interface IProductMeta
    {
        List<string> Exchanges { get; }
        Dictionary<string, double> TradingFee { get; }
        Dictionary<string, Dictionary<string, string>> ExchangeRate { get; }
        Dictionary<string, Dictionary<string, List<string>>> OneWayCoinToCurrencyTakeProfit { get; }
        Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>> TwoWayCoinToCurrencyTakeProfit { get; }
        Dictionary<string, Dictionary<string, double>> MinMaxTradingAmount { get; }

        //string ConvertExchangeSymbolToStandard(string exchangeSymbol);
        string ConvertExchangeSymbolToStandardSymbol(string exchange, string exchangeSymbol);
        string ConvertStandardSymbolToExchangeSymbol(string exchange, string standardSymbol);
        string ConvertExchangeCurrencyToStandardCurrency(string exchange, string exchagneCurrency);
        string ConvertStandardCurrencyToExchangeCurrency(string exchange, string standardCurrency);
        //string ConvertCryptoToStandardSymbol(string exchange, string crypto);

        //string ConvertExchangeAltNameToExchangeSymbol(string exchange, string altName);
        //string ConvertExchangeAltNameToStandardSymbol(string exchange, string altName);

        List<string> GetExchangeSymbolsForExchange(string exchange);
        List<string> GetStandardSymbolsForExchange(string exchange);
        string GetStandardSymbolFromStandardCurrency(string exchange, string currency);
        string GetTransferAddress(string srcexchange, string dstexchange, string crypto);
        string GetCurrencyForExchange(string exchange);
        string GetUniqueSymbolFromExchangeSymbol(string exchange, string exchangeSymbol);


        Dictionary<string, Dictionary<string, double>> GetExchangeRate();
        Dictionary<string, Dictionary<string, List<string>>> GetOneWayCoinToCurrencyTakeProfit();
        Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>> GetTwoWayCoinToCurrencyTakeProfit();
        Dictionary<string, Dictionary<string, string>> ExchangeSymbolToStandard { get; }

        bool CheckIfTakeProfitOnewayCoinToCurrency(string exchange1, string exchange2, string crypto);
        bool CheckIfTakeProfitTwowayCoinToCurrency(string exchange1, string crypto1, string exchange2, string crypto2);
        bool CheckIfTakeProfitTwowayCoinToCoin(string exchange1, string symbol1, string exchange2, string symbol2);
        bool CheckIfTakeProfitOnewayCoinToCoin(string exchange1, string exchange2, string symbol);

        double GetWithdrawCost(string srcExchange, string dstExchagne, string crypto);
    }
}
