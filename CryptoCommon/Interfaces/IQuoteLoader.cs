using PortableCSharpLib.TechnicalAnalysis;

namespace CryptoCommon.Interfaces
{
    public interface IQuoteLoader
    {
        QuoteBasic LoadQuote(string symbol, int interval, long startTime, int maxCount);
    }
}
