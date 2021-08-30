using CryptoCommon.DataTypes;
using PortableCSharpLib.TechnicalAnalysis;
using System.Collections.Generic;

namespace CryptoCommon.Services
{
    public interface ICrpytoQuoteService
    {
        //ServiceResult<string> GetExchange();
        ServiceResult<List<string>> GetAvaliableSymbols();
        ServiceResult<List<string>> GetAvaliableQuoteIds();
        ServiceResult<QuoteCapture> GetInMemoryQuoteCapture(string symbol);
        ServiceResult<QuoteBasicBase> GetInMemoryQuoteBasic(string symbol, int interval);
        ServiceResult<QuoteBasicBase> GetQuoteBasic(string symbol, int interval, long stime, int num);
    }
}
