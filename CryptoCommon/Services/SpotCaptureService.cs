using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CryptoCommon.DataTypes;
using CryptoCommon.Interfaces;
using PortableCSharpLib.DataType;
using PortableCSharpLib.TechnicalAnalysis;
using CryptoCommon.Services;

namespace CryptoCommon.Services
{
    public class SpotCaptureService : ICryptoCaptureService
    {
        ISpotMarket _market;

        public SpotCaptureService(ISpotMarket market)
        {
            _market = market;
        }

        public ServiceResult<QuoteBasicBase> Download(string symbol, int interval, int limit = 300)
        {
            var r = ServiceResult<QuoteBasicBase>.CallAsyncFunction(() => _market.Download(symbol, interval, limit)).Result;
            return r;
            //if (r.Result)
            //    return new ServiceResult<QuoteBasicBase> { Result = true, Data = new QuoteBasicBase(r.Data) };
            //return new ServiceResult<QuoteBasicBase> { Result = false, Data = null };
        }

        public ServiceResult<List<string>> GetAvailableSymbols()
        {
            try
            {
                var tickers = _market.GetTickers().Result;
                return new ServiceResult<List<string>> { Result = true, Data = tickers.Keys.ToList() };
            }
            catch (Exception ex)
            {
                return new ServiceResult<List<string>> { Result = false, Message = ex.ToString() };
            }
        }

        public ServiceResult<string> GetExchange()
        {
            return new ServiceResult<string> { Result = true, Data = _market.Exchange };
        }

        public ServiceResult<List<OHLC>> GetOHLC(string symbol, int interval)
        {
            return ServiceResult<List<OHLC>>.CallAsyncFunction(() => _market.GetOHLC(symbol, interval)).Result;
        }

        public ServiceResult<Orderbook> GetOrderbook(string symbol)
        {
            return ServiceResult<Orderbook>.CallAsyncFunction(() => _market.GetOrderbook(symbol)).Result;
        }

        public ServiceResult<Ticker> GetTicker(string symbol)
        {
            return ServiceResult<Ticker>.CallAsyncFunction(() => _market.GetTickerBySymbol(symbol)).Result;
        }

        public ServiceResult<Dictionary<string, Ticker>> GetTickers()
        {
            return ServiceResult<Dictionary<string, Ticker>>.CallAsyncFunction(() => _market.GetTickers()).Result;
        }
    }
}
