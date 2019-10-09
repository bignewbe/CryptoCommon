using CryptoCommon.Future.DataType;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CryptoCommon.Future.Interface
{
    public interface IFutureAccount
    {
        /// <summary>
        /// 所有币种合约账户信息， 持仓，保证金等
        /// </summary>
        /// <returns>合约账户信息</returns>
        Task<Dictionary<string, FutureBalance>> GetAccountsAsync();

        ///// <summary>
        ///// 获取单个币种的合约账户信息
        ///// </summary>
        ///// <param name="currency">币种，如：btc</param>
        ///// <returns>该币种的合约账户信息</returns>
        Task<FutureBalance> GetAccountByCurrencyAsync(string currency);


        Task<Dictionary<string, FuturePosition>> GetPositionBatchAsysnc();
        Task<FuturePosition> GetPositionByIdAsysnc(string instrument_id);
    }
}
