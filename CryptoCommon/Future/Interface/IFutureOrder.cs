using CryptoCommon.DataTypes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CryptoCommon.Future.Interface
{
    public interface IFutureOrder
    {
        /// <summary>
        /// 下单
        /// </summary>
        /// <param name="instrument_id">合约ID，如BTC-USD-180213</param>
        /// <param name="type">1:开多2:开空3:平多4:平空</param>
        /// <param name="price">每张合约的价格</param>
        /// <param name="size">买入或卖出合约的数量（以张计数）</param>
        /// <param name="leverage">要设定的杠杆倍数，10或20</param>
        /// <param name="client_oid">由您设置的订单ID来识别您的订单</param>
        /// <param name="match_price">是否以对手价下单(0:不是 1:是)，默认为0，当取值为1时。price字段无效</param>
        /// <returns></returns>
        //void PlaceOrderAsync(string instrument_id, string type, decimal price, int size, int leverage, string client_oid, string match_price);
        //void CancelOrderByIdAsync(string instrument_id, string orderId);
        //void CancelOrderBatchAsync(string instrument_id, List<string> orderIds);
        //void GetOrderByIdAsync(string instrument_id, string orderId);
        /// <summary>
        /// 获取订单列表
        /// </summary>
        /// <param name="instrument_id">合约ID，如BTC-USD-180213</param>
        /// <param name="status">订单状态(-1.撤单成功；0:等待成交 1:部分成交 2:全部成交 6：未完成（等待成交+部分成交）7：已完成（撤单成功+全部成交）</param>
        /// <param name="from">分页游标开始</param>
        /// <param name="to">分页游标截至</param>
        /// <param name="limit">分页数据数量，默认100</param>
        /// <returns></returns>
        //Task<List<FZOrder>> GetOrderBatchAsync(string instrument_id, FutureState status, int? from, int? to, int? limit);

        Task<List<FZOrder>> GetOpenOrdersAsync(string instrument_id);   //submmiting + open + cancelling + partial filled
        Task<List<FZOrder>> GetClosedOrdersAsync(string instrument_id); //fully filled orders
        Task<List<FZOrder>> GetCancelleddOrdersAsync(string instrument_id); //fully filled orders
        Task<List<FZOrder>> GetFailedOrdersAsync(string instrument_id); //fully filled orders
        
        //place order, return new order if sucessful, otherwise return null
        Task<FZOrder> PlaceOrderAsync(FZOrder order);
        //cancel order, return new order if sucessful, otherwise return null
        Task<FZOrder> CancelOrderAsync(FZOrder order);
        //check order, return new order if sucessful, otherwise return null
        Task<FZOrder> CheckOrderAsync(FZOrder order);


        ///// <summary>
        ///// 获取某个合约的持仓信息
        ///// </summary>
        ///// <param name="instrument_id">合约ID</param>
        ///// <returns>该合约全部持仓</returns>
        //void GetPositionByIdAsync(string instrument_id);

        ///// <summary>
        ///// 获取合约账户所有的持仓信息。
        ///// </summary>
        ///// <returns>账户所有持仓信息</returns>
        //void GetPositionBatchAsync();        

        ///// <summary>
        ///// 获取合约账户币种杠杆倍数
        ///// </summary>
        ///// <param name="currency">币种，如：btc</param>
        ///// <returns></returns>
        //Task<JObject> GetLeverageAsync(string currency);

        ///// <summary>
        ///// 全仓设定合约币种杠杆倍数
        ///// </summary>
        ///// <param name="currency">币种，如：btc</param>
        ///// <param name="leverage">要设定的杠杆倍数，10或20</param>
        ///// <returns></returns>
        //Task<JObject> SetCrossedLeverageAsync(string currency, int leverage);

        ///// <summary>
        ///// 逐仓设定合约币种杠杆倍数
        ///// </summary>
        ///// <param name="currency">币种，如：btc</param>
        ///// <param name="leverage">要设定的杠杆倍数，10或20</param>
        ///// <param name="instrument_id">合约ID，如BTC-USD-180213</param>
        ///// <param name="direction">开仓方向，long(做多)或者short(做空)</param>
        ///// <returns></returns>
        //Task<JObject> SetFixedLeverageAsync(string currency, int leverage, string instrument_id, string direction);
    }
}
