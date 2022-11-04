using CryptoCommon.DataTypes;
using PortableCSharpLib.DataType;
using PortableCSharpLib.TechnicalAnalysis;
using System.Collections.Generic;

namespace CryptoCommon.Services
{
    public interface ICryptoCaptureService: IDownLoadService
    {
        ServiceResult<string> GetExchange();
        ServiceResult<List<string>> GetAvailableSymbols();
        ServiceResult<Ticker> GetTicker(string symbol);
        ServiceResult<Dictionary<string, Ticker>> GetTickers();
        ServiceResult<Orderbook> GetOrderbook(string symbol);
        ServiceResult<List<OHLC>> GetOHLC(string symbol, int interval);

        //ServiceResult<QuoteBasicBase> Download(string symbol, int interval, int limit = 300);
        //Orderbook GetOrderBookByInstrumentId(string instrument_id);
        //three messages
        //event EventHandlers.TickerReceivedEventHandlerList OnTickerListReceived;
        //event EventHandlers.ExceptionOccuredEventHandler OnExceptionOccured;
        //event EventHandlers.CandleListReceivedEventHandler OnCandleListRecevied;
    }
}
