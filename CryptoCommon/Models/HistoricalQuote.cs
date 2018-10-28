using CommonCSharpLibary.Facility;
using CryptoCommon.Interfaces;
using PortableCSharpLib;
using PortableCSharpLib.TechnicalAnalysis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoCommon.Models
{
    public class HistoricalQuote : IHistoricalQuote
    {
        const int maxNumBarsInFile = 1000000;

        public string Exchange { get { return _download.Exchange; } }
        public string Folder { get; private set; }

        private ICache<QuoteBasic> _cache = new Cache<QuoteBasic>(50);
        private IDownloadHistoricalQuote _download;

        public HistoricalQuote(string folder, IDownloadHistoricalQuote download)
        {
            if (!Directory.Exists(folder))
                throw new DirectoryNotFoundException(folder);

            this.Folder = folder;
            _download = download;
        }

        private string GetQuoteFileName(string symbol, int interval, int index)
        {
            var fn = Path.Combine(this.Folder, string.Format("{0}_{1}_{2}_{3}.txt", Exchange, symbol, interval, index));
            return fn;
        }

        private List<string> FindQuoteFiles(string symbol, int interval)
        {
            return FileOperation.FindFilesContainingStr(this.Folder, string.Format("{0}_{1}_{2}", Exchange, symbol, interval));
        }

        //file naming convention: exchange_symbol_interval_index.txt
        public async Task<bool> UpdateHistoricalData(string symbol, int interval, int timeout = 50000)
        {
            var files = this.FindQuoteFiles(symbol, interval);
            if (files != null && files.Count > 0)
            {
                files.Sort();
                var readtream = new FileStream(files.Last(), FileMode.Open);
                var q1 = QuoteBasic.InitByStream(readtream);
                readtream.Close();

                var utcNow = DateTime.UtcNow.GetUnixTimeFromUTC();
                if (utcNow - q1.LastTime <= interval)
                    return false;

                var q = await _download.DownloadHistoricalData(symbol, interval, timeout);
                if (q == null || q.Count == 0)
                    return false;

                if (q1.Count < maxNumBarsInFile)
                {
                    q1.Append(q);
                    var writestream = new FileStream(files.Last(), FileMode.Truncate);
                    q1.AppendStream(writestream);
                    writestream.Close();
                }
                else
                {
                    var index = int.Parse(files.Last().Split('_')[3].Split('.')[0]);
                    var fn = this.GetQuoteFileName(symbol, interval, index + 1);
                    var writestream = new FileStream(fn, FileMode.OpenOrCreate);
                    q.AppendStream(writestream);
                    writestream.Close();
                }
            }
            else
            {
                var q = await this.DownloadHistoricalData(symbol, interval, timeout);
                if (q == null || q.Count == 0)
                    return false;

                var fn = this.GetQuoteFileName(symbol, interval, 0);
                var writestream = new FileStream(fn, FileMode.OpenOrCreate);
                q.AppendStream(writestream);
                writestream.Close();
            }

            return true;
        }

        public static QuoteBasic LoadQuote(string filename)
        {
            var readtream = new FileStream(filename, FileMode.Open);
            var q1 = QuoteBasic.InitByStream(readtream);
            readtream.Close();
            return q1;
        }

        public QuoteBasic LoadHistoricalData(string symbol, int interval, long startTime, int maxCount)
        {
            var files = this.FindQuoteFiles(symbol, interval);
            if (files == null || files.Count <= 0) return null;

            var quotes = new List<QuoteBasic>();
            var quote = new QuoteBasic(symbol, interval);
            files.Sort();

            var count = 0;
            for (int i = files.Count - 1; i >= 0; i--)
            {
                var q = _cache.GetItem(files[i]);
                if (q == null)
                {
                    q = LoadQuote(files[i]);
                    _cache.AddItem(files[i], q);
                }

                quotes.Add(q);
                count += q.Count;
                if (count >= maxCount || q.FirstTime <= startTime)
                    break;
            }

            for (int i = 0; i < quotes.Count; i++)
                quote.Append(quotes[i]);

            var ind1 = quote.FindIndexWhereTimeLocated(startTime);
            var ind2 = quote.Count - maxCount;
            var index = Math.Max(0, Math.Max(ind1, ind2));

            if (index != 0)
                return quote.Extract(index, quote.Count - 1) as QuoteBasic;
            else
                return quote;
        }

        //file naming convention: exchange_symbol_interval_index.txt
        public bool SaveHistoricalData(IQuoteBasic quote)
        {
            if (quote == null || quote.Count == 0) return false;
            var files = this.FindQuoteFiles(quote.Symbol, quote.Interval);

            if (files != null && files.Count > 0)
            {
                files.Sort();
                var readtream = new FileStream(files.Last(), FileMode.Open);
                var q1 = QuoteBasic.InitByStream(readtream);
                readtream.Close();

                if (quote.LastTime <= q1.LastTime) return false;

                if (q1.Count < maxNumBarsInFile)
                {
                    q1.Append(quote);
                    var writestream = new FileStream(files.Last(), FileMode.Truncate);
                    q1.AppendStream(writestream);
                    writestream.Close();
                }
                else
                {
                    var index = int.Parse(files.Last().Split('_')[3].Split('.')[0]);
                    var fn = this.GetQuoteFileName(quote.Symbol, quote.Interval, index + 1);
                    var writestream = new FileStream(fn, FileMode.OpenOrCreate);

                    if (q1.LastTime >= quote.FirstTime)
                    {
                        var sind = quote.FindIndexWhereTimeLocated(q1.LastTime);
                        var q2 = new QuoteBasic(quote);
                        q2.Clear(0, sind);
                        q2.AppendStream(writestream);
                    }
                    else
                    {
                        quote.AppendStream(writestream);
                    }
                    writestream.Close();
                }
            }
            else
            {
                var fn = this.GetQuoteFileName(quote.Symbol, quote.Interval, 0);
                var writestream = new FileStream(fn, FileMode.OpenOrCreate);
                quote.AppendStream(writestream);
                writestream.Close();
            }

            return true;
        }

        public async Task<QuoteBasic> DownloadHistoricalData(string symbol, int interval, int timeout = 50000)
        {
            return await _download.DownloadHistoricalData(symbol, interval, timeout);
        }
    }
}
