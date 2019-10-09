using CryptoCommon.Interfaces;
using CryptoCommon.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PortableCSharpLib;
using System;
using System.IO;
using System.Threading.Tasks;
using CommonCSharpLibary.Extension;
using PortableCSharpLib.TechnicalAnalysis;

namespace CryptoCommonUnitTest
{
    [TestClass]
    public class TestQuoteBasicFileStore
    {
        string _exchange = "Bittrex";
        string _symbol = "LTC_BTC";
        int _interval = 15;
        int _numBarsInFile = 1000;
        long _timenow;
        Mock<IQuoteBasicDownloader> _moqDownload = new Mock<IQuoteBasicDownloader>();
        int _count = 0;
        long _lasttime = -1;
        string _folder = "data";


        QuoteBasicFileStore _hist;


        private string GetQuoteFileName(string symbol, int interval, int index)
        {
            var fn = Path.Combine(_folder, string.Format("{0}_{1}_{2}_{3}.txt", _exchange, symbol, interval, index));
            return fn;
        }

        public TestQuoteBasicFileStore()
        {
            //timenow = DateTime.UtcNow.GetUnixTimeFromUTC() / _interval * _interval;
            //_moqDownload = new Mock<IDownloadHistoricalQuote>();
            //_count = 0;
            //_lasttime = -1;
        }

        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
        }

        [TestInitialize]
        public void Initialize()
        {
            if (Directory.Exists(_folder))
                Directory.Delete(_folder, true);
            Directory.CreateDirectory(_folder);

            _timenow = DateTime.UtcNow.GetUnixTimeFromUTC() / _interval * _interval;

            _moqDownload.Setup(d => d.Exchange).Returns(_exchange);
            _moqDownload.Setup(d => d.Download(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>())).Returns<string, int, int>((s, i, t) =>
            {
                //if (_count == 0)
                //    _lasttime = _timenow - _startInterval * _interval;
                if (_count < 0)
                {
                    return Task.FromResult<QuoteBasicBase>(null);
                }
                else
                {
                    ++_count;

                    //create quote file and save to folder
                    var q1 = new QuoteBasicBase(_symbol, _interval);
                    var ts = _lasttime + _interval;
                    for (int j = 0; j < 700; j++) q1.AddUpdate(ts + j * _interval, 0, 0, 0, 0, 0);
                    _lasttime = q1.LastTime;

                    return Task.FromResult<QuoteBasicBase>(q1);
                }
            });



            _hist = new QuoteBasicFileStore(_exchange, _folder, _numBarsInFile);
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (Directory.Exists(_folder))
                Directory.Delete(_folder, true);
        }

        [TestMethod]
        public void TestUpdateHistoricalDataNoQuoteFilesWithDownloadFail()
        {
            _count = -1; // download return null
            var r = _hist.Update(_moqDownload.Object, _symbol, _interval).Result;
            Assert.IsFalse(r);
        }

        // test no downloaded files and first download success
        [TestMethod]
        public void TestUpdateHistoricalDataNoQuoteFilesWithDownloadSuccess()
        {
            _count = 0;  //download will succeed
            var fn = string.Empty;
            _hist.OnQuoteSaved += (object sender, string exch, string filename) =>
            {
                fn = filename;
            };

            var r = _hist.Update(_moqDownload.Object, _symbol, _interval).Result;
            Assert.IsTrue(r);
            var f = this.GetQuoteFileName(_symbol, _interval, 0);
            Assert.IsTrue(fn == f);
        }

        // test there exists downloaded files but current time and last time is too small => skip download
        [TestMethod]
        public void TestUpdateHistoricalDataWithExistingQuoteFilesButTimeDiffTooSmall()
        {
            //create quote file and save to folder
            var filename = this.GetQuoteFileName(_symbol, _interval, 0);
            var q = new QuoteBasicBase(_symbol, _interval);
            var ts = _timenow - 1000 * _interval;
            for (int j = 0; j < 1000; j++) q.AddUpdate(ts + j * _interval, 0, 0, 0, 0, 0);
            q.Time[q.Count - 1] = DateTime.UtcNow.GetUnixTimeFromUTC();
            q.SaveToFile(filename);
            
            //time interval too small => no quote updated
            var r = _hist.Update(_moqDownload.Object, _symbol, _interval).Result;
            Assert.IsFalse(r);
        }

        [TestMethod]
        public void TestUpdateHistoricalDataWithExistingQuoteFilesDownloadSuccess()
        {
            //create quote file and save to folder
            var filename = this.GetQuoteFileName(_symbol, _interval, 0);
            var q = new QuoteBasicBase(_symbol, _interval) as IQuoteBasicBase;
            var ts1 = _timenow - 10000 * _interval;
            for (int j = 0; j < 700; j++) q.AddUpdate(ts1 + j * _interval, 0, 0, 0, 0, 0);
            q.SaveToFile(filename);

            _lasttime = q.LastTime + _interval;
            _hist = new QuoteBasicFileStore(_exchange, _folder, _numBarsInFile);
            var fn = string.Empty;
            _hist.OnQuoteSaved += (object sender, string exch, string file) => fn = file;

            //append to the existing quote file
            var r = _hist.Update(_moqDownload.Object, _symbol, _interval).Result;
            Assert.IsTrue(r);
            Assert.IsTrue(fn == this.GetQuoteFileName(_symbol, _interval, 0));
            q = q.LoadFile(fn);
            Assert.IsTrue(q.Count == 1400);
                        
            //since the number is larger than maxNumbars, a new file is created
            r = _hist.Update(_moqDownload.Object, _symbol, _interval).Result;
            Assert.IsTrue(r);
            Assert.IsTrue(fn == this.GetQuoteFileName(_symbol, _interval, 1));
            q = q.LoadFile(fn);
            Assert.IsTrue(q.Count == 700);

            r = _hist.Update(_moqDownload.Object, _symbol, _interval).Result;
            Assert.IsTrue(r);
            Assert.IsTrue(fn == this.GetQuoteFileName(_symbol, _interval, 1));
            q = q.LoadFile(fn);
            Assert.IsTrue(q.Count == 1400);
        }

        [TestMethod]
        public void TestLoadHistoricalDataNoQuote()
        {
            _hist = new QuoteBasicFileStore(_exchange, _folder, _numBarsInFile);
            
            //since no quote in folder, null returned
            var quote = _hist.Load(_symbol, _interval, 0, 1000);
            Assert.IsTrue(quote == null);
        }

        [TestMethod]
        public void TestLoadHistoricalDataWithQuote()
        {
            _hist = new QuoteBasicFileStore(_exchange, _folder, _numBarsInFile);
            _lasttime = _timenow - 10000 * _interval;

            //download 700 bars and save to first file
            var r = _hist.Update(_moqDownload.Object, _symbol, _interval).Result;
            var quote = _hist.Load(_symbol, _interval, 0, 1000);
            Assert.IsTrue(quote != null && quote.Count == 700);

            //download 700 bars and save to first file
            r = _hist.Update(_moqDownload.Object, _symbol, _interval).Result;
            quote = _hist.Load(_symbol, _interval, 0, 1000);
            Assert.IsTrue(quote != null && quote.Count == 1000);

            //download 700 bars and save to 2nd file
            r = _hist.Update(_moqDownload.Object, _symbol, _interval).Result;
            quote = _hist.Load(_symbol, _interval, 0, 1000);
            Assert.IsTrue(quote != null && quote.Count == 700);
            
            //download 700 bars and save to 2nd file
            r = _hist.Update(_moqDownload.Object, _symbol, _interval).Result;
            quote = _hist.Load(_symbol, _interval, 0, 1000);
            Assert.IsTrue(quote != null && quote.Count == 1000);
        }

        [TestMethod]
        public void TestSaveHistoricalDataNull()
        {
            _hist = new QuoteBasicFileStore(_exchange, _folder, _numBarsInFile);
        }

        [TestMethod]
        public void TestSaveHistoricalDataEmptyFolder()
        {
            var fn = string.Empty;
            _hist = new QuoteBasicFileStore(_exchange, _folder, _numBarsInFile);
            _hist.OnQuoteSaved += (object sender, string exch, string filename) =>
            {
                fn = filename;
            };

            var starttime = _timenow - 10000 * _interval;
            var q = CreateQuote(starttime, 700);
            var r = _hist.Save(q);
            Assert.IsTrue(r);

            Assert.IsTrue(fn == this.GetQuoteFileName(_symbol, _interval, 0));

            starttime = q.LastTime + _interval;
            q = CreateQuote(starttime, 700);
            r = _hist.Save(q);
            Assert.IsTrue(r);
            Assert.IsTrue(fn == this.GetQuoteFileName(_symbol, _interval, 0));

            starttime = q.LastTime - 100 * _interval;
            q = CreateQuote(starttime, 700);
            r = _hist.Save(q);
            Assert.IsTrue(r);
            Assert.IsTrue(fn == this.GetQuoteFileName(_symbol, _interval, 1));
            q = QuoteBasicExension.LoadFile(fn);
            Assert.IsTrue(q.Count == 599);
        }

        private IQuoteBasicBase CreateQuote(long starttime, int num)
        {
            var q = new QuoteBasicBase(_symbol, _interval);
            for (int j = 0; j < num; j++) q.AddUpdate(starttime + j * _interval, 0, 0, 0, 0, 0);
            return q;
        }
    }
}
