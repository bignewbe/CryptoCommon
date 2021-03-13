using CryptoCommon.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoCommon.Interfaces
{
    public interface IOrder
    {
        //bool IsStarted { get; }
        //Task Start(int miliseconds);
        //void Stop();
        //Task<string> BuyMarketWithValue(string symbol, double value, int timeout = 5000);
        //Task<string> BuyMarket(string symbol, double amount, int timeout = 5000);
        //Task<SpotOrder> BuyMarket(string symbol, double amount, int timeout = 5000);
        //Task<SpotOrder> SellMarket(string symbol, double amount, int timeout = 5000);
        //Task<SpotOrder> BuyLimit(string symbol, double amount, double price, int timeout = 5000);
        //Task<SpotOrder> SellLimit(string symbol, double amount, double price, int timeout = 5000);

        string Exchange { get; }


        //Task<SpotOrder> BuyMarket(SpotOrder order, int timeout = 5000);
        //Task<SpotOrder> SellMarket(SpotOrder order, int timeout = 5000);
        //Task<SpotOrder> BuyLimit(SpotOrder order, int timeout = 5000);
        //Task<SpotOrder> SellLimit(SpotOrder order, int timeout = 5000);

        Task<long> GetLastTimeForOrder(string orderId, string symbol);
        Task<bool> UpdateLastTimeForOrder(SpotOrder order);

        Task<SpotOrder> PlaceOrder(SpotOrder order, int timeout = 5000);
        Task<bool> CancelOrder(SpotOrder order, int timeout = 5000);
        Task<SpotOrder> CheckOrder(SpotOrder order, int timeout = 5000);

        Task<List<SpotOrder>> GetOpenOrders(string symbol, bool isReturnAll = false);
        Task<List<SpotOrder>> GetClosedOrders(string symbol, bool isReturnAll = false);      //order which is filled
        Task<List<SpotOrder>> GetCancelledOrders(string symbol, bool isReturnAll = false);

        //Task<List<SpotOrder>> GetAllOpenOrders(int timeout = 5000);
        //Task<List<SpotOrder>> GetAllClosedOrders(int timeout = 5000);
        //Task<List<Order>> GetAllOrders(int timeout = 5000);
        //Task<Order> QueryOrderById(string symbol, string orderId, int timeout = 5000);
        //Task<bool> CancelOrder(string symbol, string orderId, int timeout = 5000);
        //Task<List<Order>> GetOpenOrders(string symbol, int timeout = 5000);
        //Task<List<Order>> GetClosedOrders(string symbol, int timeout = 5000);
        //Task<List<Order>> GetAllOpenOrders(int timeout = 5000);
        //Task<List<Order>> GetAllClosedOrders(int timeout = 5000);        
        //event EventHandlers.NewOrderAddedEventHandler OnNewOrderAdded;
        //event EventHandlers.CaptureStateChangedEventHandler OnCaptureStateChanged;
        //event EventHandlers.ExceptionOccuredEventHandler OnExceptionOccured;
        //event EventHandlers.OrderStatusChangedEventHandler OnOrderStatusChanged;
    }
}
