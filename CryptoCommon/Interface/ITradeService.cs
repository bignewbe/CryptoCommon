using CryptoCommon.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoCommon.Shared.ExchProxy
{
    public interface ITradeService
    {
        ServiceResult<long> GetLastTimeForOrder(string orderId, string symbol);
        ServiceResult<List<FZOrder>> GetOpenOrdersBySymbol(string symbol);                                 //submmiting + open + cancelling + partial filled
        ServiceResult<List<FZOrder>> GetClosedOrdersBySymbol(string symbol, bool isReturnAll = false);     //fully filled orders
        ServiceResult<FZOrder> CheckOrder(string symbol, string orderId, bool isStopOrder);

        ServiceResult<bool> CancelOrder(string symbol, string orderId, bool isStopOrder);
        ServiceResult<FZOrder> PlaceOrder(FZOrder order);
        ServiceResult<string> ModifyOrderPrice(string symbol, string orderId, double newPrice);
        //account
        //ServiceResult<Dictionary<string, SpotBalance>> GetAccountBalance();
    }
}
