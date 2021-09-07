using CryptoCommon.DataTypes;
using CryptoCommon.Shared.ExchProxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoCommon.Interfaces
{
    public interface IOrder: IFZOrder, IFZTrade
    {
        string Exchange { get; }
        Task<bool> UpdateLastTimeForClosedOrder(FZOrder order);

        //Task<long> GetLastTimeForClosedOrder(string orderId, string symbol);

        //Task<FZOrder> PlaceOrder(FZOrder order, int timeout = 5000);
        //Task<string> ModifyOrderPrice(string symbol, string orderId, double newPrice, int timeout = 5000);
        //Task<bool> CancelOrder(string symbol, string orderId, bool isStopOrder=false, int timeout = 5000);
        //Task<FZOrder> CheckOrder(string symbol, string orderId, bool isStopOrder=false, int timeout = 5000);

        //Task<List<FZOrder>> GetAllOpenOrders();
        //Task<List<FZOrder>> GetOpenOrdersBySymbol(string symbol);
        //Task<List<FZOrder>> GetClosedOrdersBySymbol(string symbol, bool isReturnAll = false);      //order which is filled
        //Task<List<FZOrder>> GetCancelledOrdersBySymbol(string symbol, bool isReturnAll = false);
        //Task<List<FZOrder>> GetOpenStopOrder(string symbol);
    }
}
