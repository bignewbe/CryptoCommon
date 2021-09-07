using CryptoCommon.DataTypes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CryptoCommon.Shared.ExchProxy
{
    public interface IFZOrder
    {
        Task<List<FZOrder>> GetAllOpenOrders();
        Task<List<FZOrder>> GetCancelledOrdersBySymbol(string symbol, bool isReturnAll = false);
        Task<List<FZOrder>> GetClosedOrdersBySymbol(string symbol, bool isReturnAll = false);
        Task<List<FZOrder>> GetOpenOrdersBySymbol(string symbol);
        Task<List<FZOrder>> GetOpenStopOrder(string symbol);
    }
}
