using CryptoCommon.DataTypes;
using System.Collections.Generic;

namespace CryptoCommon.ClientInterface
{
    public interface ITradeClient
    {
        string PlaceOrder(string symbol, OrderType type, double amount, double price);  //market or limit order only
        bool CancelOrder(string symbol, string orderId);
        Order QueryOrderById(string symbol, string orderId);

        List<Order> GetOpenOrders(string symbol);
        List<Order> GetClosedOrders(string symbol);

        (bool Success, string WithDrawId) RequestWithdraw(string currency, string addressLabel, double amount);
        bool CancelWithdrawById(string currency, string withDrawId);
        Funding QueryWithdrawById(string currency, string withDrawId);

        Assets GetAssets();
        List<Funding> GetDepositRecords(string currency);
        List<Funding> GetWithdrawRecords(string currency);
    }
}
