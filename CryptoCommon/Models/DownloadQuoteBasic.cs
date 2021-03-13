using CryptoCommon.Interfaces;
using PortableCSharpLib.TechnicalAnalysis;
using System.Threading.Tasks;

namespace CryptoCommon.Models
{
    public class DownloadQuoteBasic : IDownloadQuoteBasic
    {
        IMarket _market;
        public string Exchange { get { return _market.Exchange; } }

        public DownloadQuoteBasic(IMarket market)
        {
            _market = market;
        }

        public async Task<QuoteBasic> DownloadQuote(string symbol, int interval, int timeout)
        {
            var r = await _market.GetOHLC(symbol, interval, timeout);

            var q = new QuoteBasic(symbol, interval);

            foreach (var d in r)
                q.Add(d.Time, d.Open, d.High, d.Low, d.Close, d.Volume);

            return q;
        }
    }
}
