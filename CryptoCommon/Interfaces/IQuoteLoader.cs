using PortableCSharpLib.TechnicalAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoCommon.Interfaces
{
    public interface IQuoteLoader
    {
        QuoteBasic LoadQuote(string symbol, int interval, long startTime, int maxCount);
    }
}
