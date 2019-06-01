using PortableCSharpLib.TechnicalAnalysis;
using System.Threading.Tasks;

namespace CryptoCommon.Interfaces
{
    public interface IQuoteBasicFileStore
    {
        string DataFolder { get; }
        string Exchange { get; }

        Task<bool> Update(IQuoteBasicDownloader download, string symbol, int interval, int timeout = 50000);
        QuoteBasicBase Load(string symbol, int interval, long? startTime, int maxCount=500);
        bool Save(IQuoteBasicBase quote);

        event EventHandlers.QuoteSavedEventHandler OnQuoteSaved;
    }
}
