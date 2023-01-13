using CryptoCommon.DataTypes;
using PortableCSharpLib.DataType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CryptoCommon.Shared.ExchProxy
{
    public interface IExchangeWebSocket
    { 
        Task StartAsync();
        Task StopAsync();
        Task StartListenSwapUserData();
        Task StartListenSpotUserData();
        Task StopListenSwapUserData();
        Task StopListenSpotUserData();
        Task SubscribeKline60s(params string[] symbols);
        Task UnSubscribeKline60s(params string[] symbols);

        event PortableCSharpLib.EventHandlers.ItemWithIdChangedEventHandler<OHLC> OnCandleRecevied;
        event PortableCSharpLib.EventHandlers.ItemWithIdChangedEventHandler<FZOrder> OnOrderReceived;
        event PortableCSharpLib.EventHandlers.ItemWithIdChangedEventHandler<List<AccountBalance>> OnAccountBalancesReceived;
        event PortableCSharpLib.EventHandlers.ItemWithIdChangedEventHandler<Orderbook> OnOrderbookReceived;
        //event PortableCSharpLib.EventHandlers.ItemWithIdChangedEventHandler<List<FundingRate>> OnFundingRateReceived;
        event PortableCSharpLib.EventHandlers.ItemWithIdChangedEventHandler<List<AccountPosition>> OnAccountPositionsReceived;
        event PortableCSharpLib.EventHandlers.ItemChangedEventHandler<long> OnHeartbeatReceived;
    }
}
