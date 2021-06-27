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
        ConcurrentDictionary<string, SpotOrder> OpenOrders { get; }
        ConcurrentDictionary<string, SpotOrder> ClosedOrders { get; }

        event PortableCSharpLib.EventHandlers.ItemChangedEventHandler<SpotOrder> OnOrderOpened;
        event PortableCSharpLib.EventHandlers.ItemChangedEventHandler<SpotOrder> OnOrderClosed;
        event PortableCSharpLib.EventHandlers.ItemChangedEventHandler<SpotOrder> OnOrderUpdated;
        event PortableCSharpLib.EventHandlers.ItemChangedEventHandler<SpotOrder> OnOrderCancelled;
        event PortableCSharpLib.EventHandlers.ItemChangedEventHandler<List<SpotOrder>> OnOpenOrderListChanged;
        event PortableCSharpLib.EventHandlers.ItemChangedEventHandler<List<SpotOrder>> OnClosedOrderListChanged;
        //event EventHandlers.SpotOrderListChangedEventHandler OnOpenOrderListChanged;
        //event EventHandlers.SpotOrderListChangedEventHandler OnClosedOrderListChanged;
        event PortableCSharpLib.EventHandlers.ItemChangedEventHandler<SpotBalance> OnCurrencyBalanceUpdated;
        event PortableCSharpLib.EventHandlers.ItemChangedEventHandler<List<SpotBalance>> OnAccountBalanceUpdated;
        event CryptoCommon.EventHandlers.StateChangedEventHandler OnStateChanged;
        event CryptoCommon.EventHandlers.ExceptionOccuredEventHandler OnExceptionOccured;
        
        //double? ConvertCryptoToUsd(string crypto, double amount);
        //void UpdateTicker(params Ticker[] tickers);
        void UpdateBalance(params SpotBalance[] balances);
        void UpdateOrders(params SpotOrder[] orders);
        //void AddOrderIdToUpdateTimeLast(string orderId);

        bool IsOrderActionInProgress(string symbol);
        DateTime GetCurrentTime();
        //Ticker GetTicker(string symbol);
        SpotBalance GetBalance(string crypto);
        SpotTradeInfo GetOrderAndBalanceStatusForSymbol(string symbol, string paramId = null, long? stime=null);
        List<SpotOrder> GetOpenOrders(string symbol);
        List<SpotOrder> GetClosedOrders(string symbol);

        //SpotOrder CheckOrderStatus(SpotOrder order);
        //void SubmitOrder(CommandId commandId, SpotOrder order);

        void PlaceOrder(SpotOrder order);
        void CancelOrder(SpotOrder order);
        void ModifyOrderPrice(string symbol, string orderId, double newPrice);

        Task Initialize();
        Task StartAsync();
        void Stop();
        bool IsStarted { get; }
    }
}

