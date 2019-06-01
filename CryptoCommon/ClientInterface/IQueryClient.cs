using PortableCSharpLib.TechnicalAnalysis;
using System.Collections.Generic;

namespace CryptoCommon.ClientInterface
{
    public interface IQueryClient
    {
        List<string> GetAvaliableSymbols();
        IQuoteBasic GetQuoteAsync(string symbol, int interval, long stimeUtc, long etimeUtc, int maxCount);
    }
}
