using PortableCSharpLib.TechnicalAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoCommon.Interfaces
{
    public interface IQuoteQuery
    {
        string Exchange { get; }
        ///// <summary>
        ///// 连接. 连接后，如果有数据，会通过ReceiveBasicProductInfo事件接收到数据。
        ///// </summary>
        ///// <param name="timeout"></param>
        ///// <returns></returns>
        ///// <exception cref="FailedToConnectExecption">连接失败</exception>
        //Task ConnectAsync();

        ///// <summary>
        ///// 断开连接
        ///// </summary>
        ///// <param name="timeout"></param>
        ///// <exception cref="TimeoutException"></exception>
        ///// <returns></returns>
        //Task DisconnectAsync(int timeout = 10000);

        /// <summary>
        /// 获取特定ip port上所有有效的symbol
        /// </summary>
        /// <param name="timeout"></param>
        /// <exception cref="NoClientAailableException"></exception>
        /// <exception cref="FailedToConnectException"></exception>
        /// <exception cref="TimeoutException"></exception>
        /// <returns></returns>
        List<string> GetAvaliableSymbols(int timeout = 10000);

        /// <summary>
        /// 获取QuoteBasic. 最多返回maxCount个数据。当stime和etime之间的数据量大于maxCount时，从etime端开始向前取maxCount个数据
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="interval"></param>
        /// <param name="stime"></param>
        /// <param name="etime"></param>
        /// <param name="maxCount"></param>
        /// <returns>如果没有数据，返回空的QuoteBasic。如果返回null，表示服务器内部错误</returns>
        IQuoteBasic GetQuoteAsync(string symbol, int interval, long stimeUtc, long etimeUtc, int maxCount, int timeout = 15000);
    }

}
