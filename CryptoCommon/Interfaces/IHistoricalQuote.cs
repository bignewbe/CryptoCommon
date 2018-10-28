using PortableCSharpLib.TechnicalAnalysis;
using System.Threading.Tasks;

namespace CryptoCommon.Interfaces
{
    public interface IHistoricalQuote : IDownloadHistoricalQuote
    {
        Task<bool> UpdateHistoricalData(string symbol, int interval, int timeout = 50000);
        QuoteBasic LoadHistoricalData(string symbol, int interval, long startTime, int maxCount);
        bool SaveHistoricalData(IQuoteBasic quote);
    }
}
