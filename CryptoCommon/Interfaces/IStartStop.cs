namespace CryptoCommon.Interfaces
{
    public interface IStartStop
    {
        void Start();
        void Stop();
        bool IsStarted { get; }
    }
}
