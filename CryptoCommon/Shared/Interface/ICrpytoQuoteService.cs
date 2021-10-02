using CryptoCommon.DataTypes;
using PortableCSharpLib.DataType;
using PortableCSharpLib.TechnicalAnalysis;
using System.Collections.Generic;

namespace CryptoCommon.Services
{
    public interface IGetInMemoryQuoteBasic
    {
        ServiceResult<bool> InitSymbol(string symbol);
        //void AddCandleList(params OHLC[] candles);
        //ServiceResult<List<string>> GetAvaliableSymbols();
        //ServiceResult<List<string>> GetAvaliableQuoteIds();
        ServiceResult<QuoteBasicBase> GetInMemoryQuoteBasic(string symbol, int interval);
    }

    public interface ICrpytoQuoteService : IGetInMemoryQuoteBasic
    {
        //ServiceResult<string> GetExchange();
        //void InitSymbol(string symbol);
        void AddCandleList(params OHLC[] candles);
        void AddTickerList(params Ticker[] tickers);

        ServiceResult<List<string>> GetAvaliableSymbols();
        ServiceResult<List<string>> GetAvaliableQuoteIds();
        ServiceResult<QuoteCapture> GetInMemoryQuoteCapture(string symbol);
        ServiceResult<QuoteBasicBase> GetQuoteBasic(string symbol, int interval, long stime, int num);
    }
}
