using CryptoCommon.DataTypes;
using PortableCSharpLib.DataType;
using System.Collections.Generic;

namespace CryptoCommon.Interfaces
{
    public interface ITickerStore
    {
        //event CryptoCommon.EventHandlers.TickerReceivedEventHandler OnTickerReceived;
        event CryptoCommon.EventHandlers.TickerUpdatedEventHandler OnTickerUpdated;
        double? ComputeRatioBetweenExchanges(string exchange1, string fiat1, string exchange2, string fiat2, string currency);
        double? ComputeRatioBetweenExchanges(string exchange1, string standardSymbol1, string exchange2, string standardSymbol2);
        void AddTickers(string exchange, List<Ticker> tickers);
        List<Ticker> GetTickers(string exchange);
        Ticker GetTicker(string exchange, string symbol);
    }
}
