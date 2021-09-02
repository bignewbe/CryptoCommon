using CryptoCommon.DataTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoCommon.Services
{
    public interface ISpotTradeService
    {
        //ServiceResult<bool> UpdateLastTimeForOrder(SpotOrder order);
        //ServiceResult<List<SpotOrder>> GetOpenStopOrder(string symbol);
        //ServiceResult<List<SpotOrder>> GetAllOpenOrders();
        //ServiceResult<List<SpotOrder>> GetCancelleddOrders(string instrument_id); //fully filled orders
        //ServiceResult<List<SpotOrder>> GetFailedOrders(string instrument_id);     //fully filled orders
        //ServiceResult<SpotOrder> PlaceStopOrder(string symbol, OrderType type, double amount, double tpPrice, double slPrice);
        //ServiceResult<SpotOrder> PlaceOrder(string symbol, OrderType type, double price, double amount, string refid, double? triggerprice = null);

        ServiceResult<long> GetLastTimeForOrder(string orderId, string symbol);
        ServiceResult<List<FZOrder>> GetOpenOrdersBySymbol(string symbol);                                 //submmiting + open + cancelling + partial filled
        ServiceResult<List<FZOrder>> GetClosedOrdersBySymbol(string symbol, bool isReturnAll = false);     //fully filled orders
        ServiceResult<FZOrder> CheckOrder(string symbol, string orderId, bool isStopOrder);

        ServiceResult<bool> CancelOrder(string symbol, string orderId, bool isStopOrder);
        ServiceResult<FZOrder> PlaceOrder(FZOrder order);
        ServiceResult<string> ModifyOrderPrice(string symbol, string orderId, double newPrice);

        //account
        ServiceResult<Dictionary<string, SpotBalance>> GetAccountBalance();
        //ServiceResult<SpotBalance> GetAccountByCurrency(string currency);
    }
}
