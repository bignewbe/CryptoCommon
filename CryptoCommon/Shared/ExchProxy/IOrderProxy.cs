using CryptoCommon.DataTypes;
using CryptoCommon.Services;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace CryptoCommon.Shared.ExchProxy
{
    public interface IOrderProxy
    {
        ConcurrentDictionary<string, FZOrder> OpenOrders { get; }
        ConcurrentDictionary<string, FZOrder> ClosedOrders { get; }

        event PortableCSharpLib.EventHandlers.ItemChangedEventHandler<FZOrder> OnOrderOpened;
        event PortableCSharpLib.EventHandlers.ItemChangedEventHandler<FZOrder> OnOrderClosed;
        event PortableCSharpLib.EventHandlers.ItemChangedEventHandler<FZOrder> OnOrderUpdated;
        event PortableCSharpLib.EventHandlers.ItemChangedEventHandler<List<FZOrder>> OnOpenOrderListChanged;
        event PortableCSharpLib.EventHandlers.ItemChangedEventHandler<List<FZOrder>> OnClosedOrderListChanged;

        bool IsOrderActionInProgress(string symbol);
        void SubmitOrder(CommandId commandId, FZOrder order);
        List<FZOrder> GetOpenOrdersBySymbol(string symbol);
        List<FZOrder> GetClosedOrdersBySymbol(string symbol);
    }
}
