namespace CryptoCommon.Interfaces
{
    public interface IRealtimeTicker : IStartStop
    {
        string Exchange { get; }
        event EventHandlers.TickerReceivedEventHandlerList OnTickerListReceived;
        event EventHandlers.ExceptionOccuredEventHandler OnExceptionOccured;
    }
}
