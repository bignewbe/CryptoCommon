using System.Collections.Generic;

namespace CryptoCommon.Interfaces
{
    public interface ISocketTicker
    {
        bool IsStarted { get; }
        string Exchange { get; }
        //HashSet<string> SubscribedStandardSymbols { get; }

        void Start();
        void Stop();
        void Subsribe(params string[] symbols);

        event EventHandlers.TickerReceivedEventHandler OnTickerReceived;
        event EventHandlers.CaptureStateChangedEventHandler OnCaptureStateChanged;
        event EventHandlers.ExceptionOccuredEventHandler OnExceptionOccured;
    }
}
