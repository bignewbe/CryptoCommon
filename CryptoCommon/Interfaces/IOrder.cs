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
        string Exchange { get; }

        Task<string> BuyMarket(string symbol, double amount, double price, int timeout);
        Task<string> SellMarket(string symbol, double amount, int timeout = 5000);
        Task<string> BuyLimit(string symbol, double amount, double price, int timeout = 5000);
        Task<string> SellLimit(string symbol, double amount, double price, int timeout = 5000);
        Task<Order> CheckOrder(string symbol, string orderId, int timeout = 5000);

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
