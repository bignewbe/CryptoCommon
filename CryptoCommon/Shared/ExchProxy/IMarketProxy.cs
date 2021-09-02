using CryptoCommon.DataTypes;
using PortableCSharpLib.DataType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoCommon.Shared.ExchProxy
{
    public interface IMarketProxy
    {
        //DateTime GetCurrentTime();
        Ticker GetTicker(string symbol);
        Orderbook GetOrderbook(string symbol);
        Dictionary<string, double> GetFundingRates();

        event PortableCSharpLib.EventHandlers.ItemChangedEventHandler<Ticker> OnTickerReceived;
        event PortableCSharpLib.EventHandlers.ItemChangedEventHandler<Orderbook> OnOrderBookReceived;
        event PortableCSharpLib.EventHandlers.ItemWithIdChangedEventHandler<double> OnFundingRateReceived;
    }
}
