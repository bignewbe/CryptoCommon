using CryptoCommon.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PortableCSharpLib;
using PortableCSharpLib.TechnicalAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoCommonUnitTest
{
    [TestClass]
    public class TestQuoteBasicMemStore
    {
        int _numBars = 100000;
        List<int> _intervals = new List<int> { 60, 180, 300, 900, 3600 };
        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
        }

        [TestInitialize]
        public void Initialize()
        {
        }

        [TestCleanup]
        public void Cleanup()
        {
        }

        [TestMethod]
        public void TestAddCandle()
        {
            var qbstore = new QuoteBasicMemStore("Okex", 1000000, _intervals);
            var interval = 60;
            var q = this.CreateQuoteBasic("symbol", interval, 0, 10000);

            for (int i = 0; i < 3700; i++)
                qbstore.AddCandle(q.Symbol, q.Interval, q.Time[i], q.Open[i], q.Close[i], q.High[i], q.Low[i], q.Volume[i], true);

            var q1 = qbstore.GetQuoteBasic("symbol", 60);
            Assert.IsTrue(q1.Count == 3700);
            var q2 = qbstore.GetQuoteBasic("symbol", 180);
            Assert.IsTrue(q2.Count == 3700 / 3 + 1);
            var q3 = qbstore.GetQuoteBasic("symbol", 300);
            Assert.IsTrue(q3.Count == 3700 / 5);
            var q4 = qbstore.GetQuoteBasic("symbol", 900);
            Assert.IsTrue(q4.Count == 3700 / 15 + 1);
            var q5 = qbstore.GetQuoteBasic("symbol", 3600);
            Assert.IsTrue(q5.Count == 3700 / 60 + 1);
        }

        [TestMethod]
        public void TestAddQuoteBasic()
        {
            {
                var qbstore = new QuoteBasicMemStore("Okex", 1000000, _intervals);
                var interval = 60;
                var q = this.CreateQuoteBasic("symbol", interval, 0, 10000);
                qbstore.AddQuoteBasic(q, true);

                var q1 = qbstore.GetQuoteBasic("symbol", 60);
                Assert.IsTrue(q1.Count == 10000);
                var q2 = qbstore.GetQuoteBasic("symbol", 180);
                Assert.IsTrue(q2.Count == 10000 / 3 + 1);
                var q3 = qbstore.GetQuoteBasic("symbol", 300);
                Assert.IsTrue(q3.Count == 10000 / 5);
                var q4 = qbstore.GetQuoteBasic("symbol", 900);
                Assert.IsTrue(q4.Count == 10000 / 15 + 1);
                var q5 = qbstore.GetQuoteBasic("symbol", 3600);
                Assert.IsTrue(q5.Count == 10000 / 60 + 1);
            }
            {
                var qbstore = new QuoteBasicMemStore("Okex", 100, _intervals);
                var interval = 60;
                var q = this.CreateQuoteBasic("symbol", interval, 0, 10000);
                qbstore.AddQuoteBasic(q, true);

                var q1 = qbstore.GetQuoteBasic("symbol", 60);
                Assert.IsTrue(q1.Count == 100);
                var q2 = qbstore.GetQuoteBasic("symbol", 180);
                Assert.IsTrue(q2.Count == 100);
                var q3 = qbstore.GetQuoteBasic("symbol", 300);
                Assert.IsTrue(q3.Count == 100);
                var q4 = qbstore.GetQuoteBasic("symbol", 900);
                Assert.IsTrue(q4.Count == 100);
                var q5 = qbstore.GetQuoteBasic("symbol", 3600);
                Assert.IsTrue(q5.Count == 10000 / 60 + 1);
            }
        }

        QuoteBasicBase CreateQuoteBasic(string symbol, int interval, long stime, int num)
        {
            var q = new QuoteBasicBase(symbol, interval);
            var count = 0;
            while (count < num)
                q.AddUpdate(stime + interval * count++, 1, 1, 1, 1, 1, false);
            return q;
        }
    }
}
