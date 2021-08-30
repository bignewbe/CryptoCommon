using CryptoCommon.DataTypes;
using PortableCSharpLib.DataType;
using PortableCSharpLib.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CryptoCommon.Interfaces
{
    public interface IMarket : IRealtimeTicker, IQuoteBasicDownloader
    {
        //void Start();
        //void Stop();
        //bool IsStarted { get; }
        string Exchange { get; }
        //HashSet<string> SubscribedStandardSymbols { get; }
        //event EventHandlers.DataBarReceivedEventHandler OnDataBarReceived;
        //event EventHandlers.DataBarReceivedEventHandlerList OnDataBarListReceived;
        //event EventHandlers.TickerReceivedEventHandler OnTickerReceived;
        //event EventHandlers.CaptureStateChangedEventHandler OnCaptureStateChanged;
        //event EventHandlers.ExceptionOccuredEventHandler OnExceptionOccured;

        Task<List<string>> GetAvailableSymbols();
        Task<Ticker> GetTicker(string symbol, int timeout = 5000);
        Task<Orderbook> GetOrderbook(string symbol, int timeout = 5000);
        Task<List<OHLC>> GetOHLC(string symbol, int interval, int timeout = 5000);
        //Task<QuoteBasicBase> DownloadHistQuote(string symbol, int interval, int timeout = 50000);
    }
}
