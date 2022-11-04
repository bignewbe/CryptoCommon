using CryptoCommon.DataTypes;
using System.Threading.Tasks;

namespace CryptoCommon.Shared.ExchProxy
{
    public interface IFZTrade
    {
        Task<FZOrder> PlaceOrder(FZOrder order, int timeout = 5000);
        Task<bool> CancelOrder(string symbol, string orderId, int timeout = 5000);
        Task<FZOrder> CheckOrder(string symbol, string orderId, int timeout = 5000);
        Task<FZOrder> ModifyOrderSzAndPx(FZOrder order);
        //Task<FZOrder> ModifyOrderSz(FZOrder order);
        Task<FZOrder> ModifyOrderPx(FZOrder order);
        Task<string> ModifyOrderPrice(string symbol, string orderId, double newPrice, int timeout = 5000);
        //Task<long> GetLastTimeForClosedOrder(string orderId, string symbol);
        //Task<bool> UpdateLastTimeForClosedOrder(FZOrder order);
    }
}
