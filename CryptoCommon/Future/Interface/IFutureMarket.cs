using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CryptoCommon.DataTypes;
using CryptoCommon.Future.Interface;
using PortableCSharpLib.DataType;
using PortableCSharpLib.TechnicalAnalysis;
using Ticker = PortableCSharpLib.DataType.Ticker;

namespace CryptoCommon.Future.Interface
{
    public interface IFutureMarket
    {
        string Exchange { get; }
        //var r3 = api.getTradesAsync(eos_instrument_id, null, null, null).Result;            //public filled data -> [PublicTrade]
        //var r6 = api.getInstrumentsAsync().Result;                                          //public contract info -> [ContractInfo]
        //var r7 = api.getOpenInterestAsync(eos_instrument_id).Result;                        //public total hold amount by okex -> PublicHoldAmount
        //var r8 = api.getLiquidationAsync(btc_instrument_id, "1", null, null, null).Result;  //public forced liquidified orders -> [ForcedLiquid]
        //var r9 = api.getCandlesDataAsync(eos_instrument_id, null, null, 60).Result;         //public candle -> [List<string>]: ["2019-05-19T16:25:00.000Z","6.4","6.402","6.395","6.402","31186","48735.14697163"]
        //var r12 = api.getPriceLimitAsync(eos_instrument_id).Result;                         //public bid ask price -> BidAskPrice
        //var r13 = api.getBookAsync(eos_instrument_id, 200).Result;                          //public order book -> OrderBook

        List<string> GetAvailableSymbols();

        /// <summary>
        /// 平台成交
        /// </summary>
        /// <param name="instrument_id"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="limit"></param>
        Task<List<PublicTrade>> GetTradesByInstrumentIdAsync(string instrument_id, int? from = null, int? to = null, int? limit = null);

        Task<List<OHLC>> GetOHLCByInstrumentIdAsync(string instrument_id, DateTime? start, DateTime? end, int? granularity);
        Task<QuoteBasicBase> Download(string symbol, int interval, int? limit = 2000);

        /// <summary>
        /// 获取平台总持仓量
        /// </summary>
        /// <param name="instrument_id">合约ID，如BTC-USD-180213</param>
        /// <returns></returns>
        Task<PublicHoldAmount> GetOpenInterestByInstrumentIdAsync(string instrument_id);

        /// <summary>
        /// 获取当前限价
        /// </summary>
        /// <param name="instrument_id">合约ID，如BTC-USD-180213</param>
        /// <returns></returns>
        Task<BidAskPrice> GetBidAskByInstrumentIdAsync(string instrument_id);

        Task<Dictionary<string, Ticker>> GetTickersAsync();
        Task<Ticker> GetTickerByInstrumentIdAsync(string instrument_id);

        /// <summary>
        /// 获取平台爆仓单
        /// </summary>
        /// <param name="instrument_id">合约ID，如BTC-USD-180213</param>
        /// <param name="status">状态(0:最近7天数据（包括第7天） 1:7天前数据)</param>
        /// <param name="from">分页游标开始</param>
        /// <param name="to">分页游标截至</param>
        /// <param name="limit">分页数据数量，默认100</param>
        /// <returns></returns>
        Task<List<ForcedLiquid>> GetLiquidationByInstrumentIdAsync(string instrument_id);//, string status, int? from, int? to, int? limit);

        Task<Orderbook> GetOrderBookByInstrumentIdAsync(string instrument_id);

        //var r13 = api.getBookAsync(eos_instrument_id, 200).Result;                          //public order book -> OrderBook
        //获取合约信息
        //var r6 = api.getInstrumentsAsync().Result;                                          //public contract info -> [ContractInfo]
    }
}
