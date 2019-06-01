using System.Threading.Tasks;

namespace CryptoCommon.Interfaces
{
    public interface IStartStop
    {
        Task<bool> StartAsync();
        Task<bool> StopAsync();
        bool IsStarted { get; }
    }
}
