using CryptoCommon.DataBase;
using CryptoCommon.DataBase.Entity;
using CryptoCommon.DataBase.Interface;
using CryptoCommon.DataTypes;
using LogInterface;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PortableCSharpLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoCommonUnitTest
{
    [TestClass]
    public class OrderDbTests
    {
        private OrderDb _db1;
        private OrderDb2 _db2;

        public OrderDbTests()
        {
            var connectionStr1 = $"Data Source=47.90.96.154,1306;Initial Catalog=BinanceSwapTrader;Persist Security Info=True;TrustServerCertificate=True;User ID=SA;Password=Password!";
            var connectionStr2 = $"Data Source=47.90.96.154,1306;Initial Catalog=TestOrder2;Persist Security Info=True;TrustServerCertificate=True;User ID=SA;Password=Password!";
            var repo1 = new Repo<FZOrderEnt, OrderDbContext>(connectionStr1);
            var repo2 = new Repo<FZOrderEnt2, OrderDbContext2>(connectionStr2);
            _db1 = new OrderDb(repo1);
            _db2 = new OrderDb2(repo2);
        }

        [TestMethod]
        public async Task Test_All()
        {
            _db2.AddUpdateAsync(new FZOrder() { OrderId="testid1"}).Wait();

            var orders = _db1.GetAllOrdersAsync().Result;
            foreach (var order in orders)
                _db2.AddUpdateAsync(order).Wait();
        }
    }
}
