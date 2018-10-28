using CryptoCommon.DataTypes;
using PortableCSharpLib.TechnicalAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoCommon.Interfaces
{
    public interface IDownloadHistoricalQuote
    {
        string Exchange { get; }
        Task<QuoteBasic> DownloadHistoricalData(string symbol, int interval, int timeout = 50000);
    }
}
