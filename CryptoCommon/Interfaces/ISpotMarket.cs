using CryptoCommon.DataTypes;
using PortableCSharpLib.DataType;
using PortableCSharpLib.TechnicalAnalysis;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CryptoCommon.Interfaces
{
    public interface ISpotMarket
    {
        string Exchange { get; }

        Task<Ticker> GetTickerBySymbol(string symbol, int timeout = 5000);
        Task<Orderbook> GetOrderbook(string symbol, int timeout = 5000);
        Task<Dictionary<string, Ticker>> GetTickers();
        Task<List<OHLC>> GetOHLC(string symbol, int interval, int timeout = 5000);
        Task<QuoteBasicBase> Download(string symbol, int interval, int? limit = 2000);
    }
}
