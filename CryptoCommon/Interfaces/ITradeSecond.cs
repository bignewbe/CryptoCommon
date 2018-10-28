using CryptoCommon.DataTypes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CryptoCommon.Interfaces
{
    public interface ITradeSecond
    {
        string Exchange { get; }

        Task<string> PlaceOrder(string symbol, OrderType type, double amount, double price, int timeout = 5000);  //market or limit order only
        Task<bool> CancelOrder(string symbol, string orderId, int timeout = 5000);
        Task<Order> QueryOrderById(string symbol, string orderId, int timeout = 5000);

        Task<List<Order>> GetOpenOrders(string symbol, int timeout = 5000);
        Task<List<Order>> GetClosedOrders(string symbol, int timeout = 5000);

        Task<(bool Success, string WithDrawId)> RequestWithdraw(string currency, string addressLabel, double amount, int timeout = 5000);
        Task<bool> CancelWithdrawById(string currency, string withDrawId, int timeout = 5000);
        Task<Funding> QueryWithdrawById(string currency, string withDrawId, int timeout = 5000);

        Task<Assets> GetAssets(int timeout = 5000);
        Task<List<Funding>> GetDepositRecords(string currency, int timeout = 5000);
        Task<List<Funding>> GetWithdrawRecords(string currency, int timeout = 5000);
    }
}
