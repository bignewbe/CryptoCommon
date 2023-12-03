using CryptoCommon.DataTypes;
using PortableCSharpLib.DataType;
using PortableCSharpLib.TechnicalAnalysis;
using System.Collections.Generic;

namespace CryptoCommon.ClientInterface
{
    public interface ICaputreClient
    {
        //void Start();
        //void Stop();
        IQuoteCapture GetInMemoryQuoteCapture(string symbol);
        List<string> GetAvaliableSymbols(int timeout);
        Dictionary<string, Ticker> RequestPrices();
        Ticker RequestPrice(string symbol);
    }
}
