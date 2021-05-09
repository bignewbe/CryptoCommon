using CryptoCommon.DataTypes;
using CryptoCommon.Future.DataType;
using CryptoCommon.Future.Interface;
using System.Collections.Generic;

namespace CryptoCommon.Services
{
    public interface IFutureTradeService
    {
        // order
        ServiceResult<List<FutureOrder>> GetOpenOrders(string instrument_id);       //submmiting + open + cancelling + partial filled
        ServiceResult<List<FutureOrder>> GetClosedOrders(string instrument_id);     //fully filled orders
        ServiceResult<List<FutureOrder>> GetCancelleddOrders(string instrument_id); //fully filled orders
        ServiceResult<List<FutureOrder>> GetFailedOrders(string instrument_id);     //fully filled orders

        ServiceResult<FutureOrder> CancelOrder(string symbol, string orderId);
        ServiceResult<FutureOrder> PlaceOrder(string symbol, OrderType type, double price, int amount, string refid, ExecutionType executionType = ExecutionType.Standard);
        ServiceResult<FutureOrder> CheckOrder(string symbol, string orderId);

        //ServiceResult<FutureOrder> PlaceOrder(FutureOrder order);
        //ServiceResult<FutureOrder> CancelOrder(FutureOrder order);
        //ServiceResult<FutureOrder> CheckOrder(FutureOrder order);

        ServiceResult<Dictionary<string, FuturePosition>> GetPositionBatch();
        ServiceResult<FuturePosition> GetPositionById(string instrument_id);

        //account
        ServiceResult<Dictionary<string, FutureBalance>> GetAccounts();
        ServiceResult<FutureBalance> GetAccountByCurrency(string currency);
    }
}
