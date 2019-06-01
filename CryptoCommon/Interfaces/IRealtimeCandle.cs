namespace CryptoCommon.Interfaces
{
    public interface IRealtimeCandle : IStartStop
    {
        string Exchange { get; }
        event EventHandlers.CandleListReceivedEventHandler OnCandleListRecevied;
        event EventHandlers.ExceptionOccuredEventHandler OnExceptionOccured;
    }
}
