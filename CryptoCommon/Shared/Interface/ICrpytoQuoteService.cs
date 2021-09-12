﻿using CryptoCommon.DataTypes;
using PortableCSharpLib.DataType;
using PortableCSharpLib.TechnicalAnalysis;
using System.Collections.Generic;

namespace CryptoCommon.Services
{
    public interface ICrpytoQuoteService
    {
        //ServiceResult<string> GetExchange();
        void AddCandleList(params OHLC[] candles);
        void AddTickerList(params Ticker[] tickers);

        ServiceResult<List<string>> GetAvaliableSymbols();
        ServiceResult<List<string>> GetAvaliableQuoteIds();
        ServiceResult<QuoteCapture> GetInMemoryQuoteCapture(string symbol);
        ServiceResult<QuoteBasicBase> GetInMemoryQuoteBasic(string symbol, int interval);
        ServiceResult<QuoteBasicBase> GetQuoteBasic(string symbol, int interval, long stime, int num);
    }
}
