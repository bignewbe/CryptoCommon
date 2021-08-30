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
        //Task<SpotOrder> BuyMarket(SpotOrder order, int timeout = 5000);
        //Task<SpotOrder> SellMarket(SpotOrder order, int timeout = 5000);
        //Task<SpotOrder> BuyLimit(SpotOrder order, int timeout = 5000);
        //Task<SpotOrder> SellLimit(SpotOrder order, int timeout = 5000);
        //Task<List<string>> CancelStopOrders(string symbol, params string[] orderIds);
        //Task<SpotOrder> GetStopOrderByOrderId(string symbol, string orderId);
        //Task<List<SpotOrder>> GetStopOrderByState(string symbol, OrderState state);
        //Task<string> PlaceStopOrder(string symbol, double amount, OrderType type, double? TPTriggerPrice, double? TPPrice, double? SLTriggerPrice, double? SLPrice);


        string Exchange { get; }

        Task<long> GetLastTimeForClosedOrder(string orderId, string symbol);
        Task<bool> UpdateLastTimeForClosedOrder(FZOrder order);

        Task<FZOrder> PlaceOrder(FZOrder order, int timeout = 5000);
        Task<string> ModifyOrderPrice(string symbol, string orderId, double newPrice, int timeout = 5000);
        Task<bool> CancelOrder(string symbol, string orderId, bool isStopOrder=false, int timeout = 5000);
        Task<FZOrder> CheckOrder(string symbol, string orderId, bool isStopOrder=false, int timeout = 5000);

        Task<List<FZOrder>> GetAllOpenOrders();
        Task<List<FZOrder>> GetOpenOrdersBySymbol(string symbol);
        Task<List<FZOrder>> GetClosedOrdersBySymbol(string symbol, bool isReturnAll = false);      //order which is filled
        Task<List<FZOrder>> GetCancelledOrdersBySymbol(string symbol, bool isReturnAll = false);
        Task<List<FZOrder>> GetOpenStopOrder(string symbol);
    }
}
