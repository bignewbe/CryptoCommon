using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoCommon.Interfaces
{
    public interface ISpotWebSocket
    {
        Task ConncectAsync();
        event EventHandlers.CandleListReceivedEventHandler OnSpotCandleListRecevied;
        event EventHandlers.SpotOrderReceivedEventHandler OnSpotOrderReceived;
        event EventHandlers.SpotBalanceReceivedEventHandler OnSpotBalanceReceived;
    }
}
