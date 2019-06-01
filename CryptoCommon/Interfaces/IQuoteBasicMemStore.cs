using PortableCSharpLib.DataType;
using PortableCSharpLib.TechnicalAnalysis;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace CryptoCommon.Interfaces
{
    public interface IQuoteBasicMemStore
    {
        string Exchange { get; }
        int MaxNumCandles { get; }
        List<int> Intervals { get; }
        ConcurrentDictionary<string, QuoteBasicBase> Quotes { get; }
        event EventHandlers.QuoteBasicDataAddedOrUpdatedEventHandler OnQuoteBasicDataAddedOrUpdated;
        void AddQuoteCapture(IQuoteCapture qc);
        void AddQuoteBasic(IQuoteBasicBase qb, bool isAddToAll);
        void AddCandle(List<OHLC> ohlc, bool isAddToAll);
        /// <summary>
        /// add single bar
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="interval"></param>
        /// <param name="time"></param>
        /// <param name="open"></param>
        /// <param name="close"></param>
        /// <param name="high"></param>
        /// <param name="low"></param>
        /// <param name="volume"></param>
        /// <param name="isAddToAll">indicate wether add candle to all quotes of intervals greater than interval</param>
        void AddCandle(string symbol, int interval, long time, double open, double close, double high, double low, double volume, bool isAddToAll);
        QuoteBasicBase GetQuoteBasic(string symbol, int interval);
    }
}