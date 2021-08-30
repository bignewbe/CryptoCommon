using CryptoCommon.DataTypes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CryptoCommon.Services
{
    public enum CommandId
    {
        None,
        PlaceOrder,
        CancelOrder,
        QueryOrder
    }

    public interface ISpotProxy
    {
        List<string> Symbols { get; }

        ConcurrentDictionary<string, SpotBalance> Balances { get; }
        ConcurrentDictionary<string, FZOrder> OpenOrders { get; }
        ConcurrentDictionary<string, FZOrder> ClosedOrders { get; }

        event PortableCSharpLib.EventHandlers.ItemChangedEventHandler<FZOrder> OnOrderOpened;
        event PortableCSharpLib.EventHandlers.ItemChangedEventHandler<FZOrder> OnOrderClosed;
        event PortableCSharpLib.EventHandlers.ItemChangedEventHandler<FZOrder> OnOrderUpdated;
        event PortableCSharpLib.EventHandlers.ItemChangedEventHandler<FZOrder> OnOrderCancelled;
        event PortableCSharpLib.EventHandlers.ItemChangedEventHandler<List<FZOrder>> OnOpenOrderListChanged;
        event PortableCSharpLib.EventHandlers.ItemChangedEventHandler<List<FZOrder>> OnClosedOrderListChanged;
        //event EventHandlers.SpotOrderListChangedEventHandler OnOpenOrderListChanged;
        //event EventHandlers.SpotOrderListChangedEventHandler OnClosedOrderListChanged;
        event PortableCSharpLib.EventHandlers.ItemChangedEventHandler<SpotBalance> OnCurrencyBalanceUpdated;
        event PortableCSharpLib.EventHandlers.ItemChangedEventHandler<List<SpotBalance>> OnAccountBalanceUpdated;
        event CryptoCommon.EventHandlers.StateChangedEventHandler OnStateChanged;
        event CryptoCommon.EventHandlers.ExceptionOccuredEventHandler OnExceptionOccured;
        
        //double? ConvertCryptoToUsd(string crypto, double amount);
        //void UpdateTicker(params Ticker[] tickers);
        //void UpdateBalance(params SpotBalance[] balances);
        //void UpdateOrders(params FZOrder[] orders);
        //void AddOrderIdToUpdateTimeLast(string orderId);

        bool IsOrderActionInProgress(string symbol);
        DateTime GetCurrentTime();
        //Ticker GetTicker(string symbol);
        SpotBalance GetBalance(string crypto);
        SpotTradeInfo GetOrderAndBalanceStatusForSymbol(string symbol, string paramId = null, long? stime=null);
        List<FZOrder> GetOpenOrders(string symbol);
        List<FZOrder> GetClosedOrders(string symbol);

        //SpotOrder CheckOrderStatus(SpotOrder order);
        //void SubmitOrder(CommandId commandId, SpotOrder order);

        void PlaceOrder(FZOrder order);
        void CancelOrder(FZOrder order);
        void ModifyOrderPrice(string symbol, string orderId, double newPrice);

        //Task Initialize();
        //Task StartAsync();
        //void Stop();
        //bool IsStarted { get; }
    }
}

