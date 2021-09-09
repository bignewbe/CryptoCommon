﻿using CryptoCommon.DataTypes;
using CryptoCommon.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace CryptoCommon.Shared.ExchProxy
{
    public interface IOrderProxy
    {
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

        FZOrder PlaceOrder(FZOrder order);
        bool CancelOrder(FZOrder order);

        //void PlaceOrder(FZOrder order);
        //void CancelOrder(FZOrder order);
        //FZOrder CheckOrder(FZOrder order);

        List<FZOrder> GetOpenOrdersBySymbol(string symbol);
        List<FZOrder> GetClosedOrdersBySymbol(string symbol);

        List<FZOrder> GetOpenOrders();
        List<FZOrder> GetClosedOrders();

        void UpdateOrders(params FZOrder[] orders);
    }
}