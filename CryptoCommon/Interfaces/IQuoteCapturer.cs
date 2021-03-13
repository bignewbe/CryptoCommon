using PortableCSharpLib.TechnicalAnalysis;
using System;
using System.Collections.Generic;

namespace CryptoCommon.Interfaces
{
    public interface IQuoteCapturer
    {
        List<int> Intervals { get; }
        string DataFolder { get; }
        string Exchange { get; }
        int MaxNumTicker { get; }
        int MaxNumBars { get; }
        bool IsStarted { get; }

        void Start();
        void Stop();

        /// <summary>
        /// 获取本周的QuoteCapture
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        /// <exception cref="NoClientAailableException"></exception>
        /// <exception cref="FailedToConnectException"></exception>
        /// <exception cref="TimeoutException"></exception>
        IQuoteCapture GetInMemoryQuoteCapture(string symbol);

        /// <summary>
        /// 获取特定ip port上所有有效的symbol
        /// </summary>
        /// <param name="timeout"></param>
        /// <exception cref="NoClientAailableException"></exception>
        /// <exception cref="FailedToConnectException"></exception>
        /// <exception cref="TimeoutException"></exception>
        /// <returns></returns>
        List<string> GetAvaliableSymbols();

        /// <summary>
        /// 获取QuoteBasic. 最多返回maxCount个数据。当stime和etime之间的数据量大于maxCount时，从etime端开始向前取maxCount个数据
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="interval"></param>
        /// <param name="stime"></param>
        /// <param name="etime"></param>
        /// <param name="maxCount"></param>
        /// <returns>如果没有数据，返回空的QuoteBasic。如果返回null，表示服务器内部错误</returns>
        IQuoteBasic GetInMemoryQuoteBasic(string symbol, int interval); //, long stimeUtc = -1, long etimeUtc = -1, int maxCount = -1);
        
        //event EventHandlers.TickerReceivedEventHandler OnTickerReceived;
        event EventHandlers.TickerReceivedEventHandlerList OnTickerListReceived;
        event EventHandlers.ExceptionOccuredEventHandler OnExceptionOccured;
        event EventHandlers.DataAddedOrUpdatedEventHandler OnQuoteBasicDataAddedOrUpated;
        event EventHandlers.QuoteSavedEventHandler OnQuoteSaved;
    }
}
