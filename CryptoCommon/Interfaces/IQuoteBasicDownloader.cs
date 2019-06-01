using PortableCSharpLib.TechnicalAnalysis;
using System.Threading.Tasks;

namespace CryptoCommon.Interfaces
{
    public interface IQuoteBasicDownloader
    {
        string Exchange { get; }
        Task<QuoteBasicBase> Download(string symbol, int interval, int timeout = 50000);
    }
}
