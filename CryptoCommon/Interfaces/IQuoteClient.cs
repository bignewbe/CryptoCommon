using PortableCSharpLib.TechnicalAnalysis;
using System.Threading.Tasks;

namespace CryptoCommon.Interfaces
{
    public interface IQuoteClient
    {
        void Start();
        void Stop();

        int NumBar { get; }                                                //always keep this number of bars in memory
        int Interval { get; }
        bool IsShowChart { get; set; }
        event EventHandlers.DataBarReceivedEventHandler OnRealtimeBar;
    }
}