using CryptoCommon.Interfaces;
using CryptoCommon.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PortableCSharpLib.TechnicalAnalysis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoCommonUnitTest
{
    [TestClass]
    public class TestQuoteCapturer
    {
        QuoteCapturer _capturer;
        Mock<IHistoricalQuote> _moqHist = new Mock<IHistoricalQuote>();
        Mock<ISocketTicker> _moqSocket = new Mock<ISocketTicker>();
        string _folder = "data";

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

            //_timenow = DateTime.UtcNow.GetUnixTimeFromUTC() / _interval * _interval;
            //_moqDownload.Setup(d => d.Exchange).Returns("Bittrex");
            //_moqDownload.Setup(d => d.DownloadHistoricalData(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>())).Returns<string, int, int>((s, i, t) =>
            //{
            //    //if (_count == 0)
            //    //    _lasttime = _timenow - _startInterval * _interval;
            //    if (_count < 0)
            //    {
            //        return Task.FromResult<QuoteBasic>(null);
            //    }
            //    else
            //    {
            //        ++_count;

            //        //create quote file and save to folder
            //        var q1 = new QuoteBasic(_symbol, _interval);
            //        var ts = _lasttime + _interval;
            //        for (int j = 0; j < 700; j++) q1.Add(ts + j * _interval, 0, 0, 0, 0, 0);
            //        _lasttime = q1.LastTime;

            //        return Task.FromResult<QuoteBasic>(q1);
            //    }
            //});

            //_hist = new HistoricalQuote(_folder, _moqDownload.Object, _numBarsInFile);
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (Directory.Exists(_folder))
                Directory.Delete(_folder, true);
        }

        [TestMethod]
        public async Task TestStart()
        {
            var countLoadHistoricalData = 0;
            var countSaveHistoricalData = 0;
            var countOnTickerListReceived = 0;
            var countOnTickerReceived = 0;

            _moqHist.Setup(d => d.LoadHistoricalData(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<long>(), It.IsAny<int>())).Returns<string, int, long, int>((s, i, t, m) =>
            {
                ++countLoadHistoricalData;
                return null;
            });

            _moqHist.Setup(d => d.SaveHistoricalData(It.IsAny<IQuoteBasic>())).Returns<IQuoteBasic>((q) =>
            {
                ++countSaveHistoricalData;
                return true;
            });

            var moqDownload = new Mock<IDownloadQuoteBasic>();
            moqDownload.Setup(d => d.Exchange).Returns("Bittrex");
            //moqDownload.Setup(d => d.DownloadHistoricalData(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>())).Returns<string, int, int>((s, i, t) =>
            //{
            //    return null;
            //});
            
            var hist = new HistoricalQuote(_folder, moqDownload.Object, 10000);
            var socketTicker = new MockSocketTicker(1);
            _capturer = new QuoteCapturer(_moqHist.Object, socketTicker, 100, 10);
            _capturer.OnTickerListReceived += (object sender, string exchange, List<CryptoCommon.DataTypes.Ticker> ticker) =>
            {
                ++countOnTickerListReceived;
            };
            _capturer.OnTickerReceived += (object sender, string exchange, CryptoCommon.DataTypes.Ticker ticker) =>
            {
                ++countOnTickerReceived;
            };

            _capturer.Start();

            await Task.Delay(2000);  //since timer interval = 50, there should around 80 OnTickerList events and around 80 elements in quote capture
            //_capturer.Stop();

            var qc = _capturer.GetInMemoryQuoteCapture("A");
            var qb = _capturer.GetInMemoryQuoteBasic("A", 60);

            Assert.IsTrue(countOnTickerListReceived == 1000);
            Assert.IsTrue(qc.Count > 0);//== countOnTickerListReceived);
            Assert.IsTrue(qb.Count > 0);
            Assert.IsTrue(countSaveHistoricalData > 0);
        }
    }
}
