using CryptoCommon.DataTypes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoCommon.Services
{
    public interface IOrderProxy
    {
        ConcurrentDictionary<string, long> PlaceOrderTimeBySymbolBuyFirst { get; }
        ConcurrentDictionary<string, long> PlaceOrderTimeBySymbolSellFirst { get; }

        bool IsStarted { get; }
        DateTime GetCurrentTime();
        ConcurrentDictionary<string, FZOrder> OpenOrders { get; }
        ConcurrentDictionary<string, FZOrder> ClosedOrders { get; }

        event PortableCSharpLib.EventHandlers.ItemChangedEventHandler<FZOrder> OnOrderOpened;
        event PortableCSharpLib.EventHandlers.ItemChangedEventHandler<FZOrder> OnOrderClosed;
        event PortableCSharpLib.EventHandlers.ItemChangedEventHandler<FZOrder> OnOrderUpdated;
        event PortableCSharpLib.EventHandlers.ItemChangedEventHandler<FZOrder> OnOrderCancelled;
        event PortableCSharpLib.EventHandlers.ItemChangedEventHandler<List<FZOrder>> OnOpenOrderListChanged;
        event PortableCSharpLib.EventHandlers.ItemChangedEventHandler<List<FZOrder>> OnClosedOrderListChanged;

        bool IsOrderActionInProgress(string symbol);

        void Start();
        void Stop();
        FZOrder PlaceOrder(FZOrder order);
        bool CancelOrder(FZOrder order);
        FZOrder ModifyOrderSzAndPx(FZOrder order);
        FZOrder ModifyOrderPx(FZOrder order);

        void ModifyOrderPrice(string symbol, string orderId, double newPrice);
        //FZOrder ModifyOrderSz(FZOrder order);

        //void PlaceOrder(FZOrder order);
        //void CancelOrder(FZOrder order);
        //FZOrder CheckOrder(FZOrder order);

        List<FZOrder> GetOpenOrdersBySymbol(string symbol);
        List<FZOrder> GetClosedOrdersBySymbol(string symbol);

        List<FZOrder> GetOpenOrders();
        List<FZOrder> GetClosedOrders();

        void UpdateOrders(params FZOrder[] orders);
        Orderbook GetOrderbook(string symbol);
        //double GetMinSz(string symbol);
    }
}
