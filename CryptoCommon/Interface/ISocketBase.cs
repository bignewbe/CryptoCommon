using System.Net.WebSockets;

namespace CryptoCommon.Interface
{
    public interface ISocketBase
    {
        Task StartAsync();
        Task StopAsync();
        Task SendAsync(string msg);
        Task Reconnect();

        event PortableCSharpLib.EventHandlers.ItemChangedEventHandler<WebSocketState> OnConnectionChanged;
        event PortableCSharpLib.EventHandlers.ItemChangedEventHandler<int> OnReceiveOverTime;
        event PortableCSharpLib.EventHandlers.ItemChangedEventHandler<string> OnMsgReceived;
    }

    public interface ISocketHeartbeatBase: ISocketBase
    {
        event PortableCSharpLib.EventHandlers.ItemChangedEventHandler<long> OnHeartbeatReceived;
    }
}
