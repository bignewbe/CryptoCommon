using CryptoCommon.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoCommon.Interfaces
{
    public interface IMarket
    {
        void Start();
        void Stop();
        bool IsStarted { get; }
        HashSet<string> SubscribedStandardSymbols { get; }
        string Exchange { get; }
        //Task Init();
        //event EventHandlers.DataBarReceivedEventHandler OnDataBarReceived;
        //event EventHandlers.DataBarReceivedEventHandlerList OnDataBarListReceived;
        event EventHandlers.TickerReceivedEventHandlerList OnTickerListReceived;
        event EventHandlers.TickerReceivedEventHandler OnTickerReceived;
        event EventHandlers.CaptureStateChangedEventHandler OnCaptureStateChanged;
        event EventHandlers.ExceptionOccuredEventHandler OnExceptionOccured;

        Task<Ticker> GetTicker(string symbol, int timeout = 5000);
        Task<Orderbook> GetOrderbook(string symbol, int timeout = 5000);
    }
}
