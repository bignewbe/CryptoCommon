using CryptoCommon.DataBase.Entity;
using CryptoCommon.DataBase;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CryptoCommon.DataTypes;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace CryptoCommonUnitTest
{
    [TestClass]
    public class KeyValueDbTests_FZOrder
    {
        private KeyValueDb<FZOrder> _db;
        private List<FZOrder> _orders;

        public KeyValueDbTests_FZOrder()
        {
            var connectionStr = $"Data Source=47.90.96.154,1306;Initial Catalog=RepoTest;Persist Security Info=True;TrustServerCertificate=True;User ID=SA;Password=Password!";
            var repo = new Repo<KeyValueEnt<FZOrder>, ApplicationDbContext>(connectionStr);
            _db = new KeyValueDb<FZOrder>(repo);

            var delimiter = "\nqxdjt394";
            var fileName = "E:\\data\\Okex\\okexSwapOrderProxy.json";
            var str = File.ReadAllText(fileName);
            var items = Regex.Split(str, delimiter);
            var openOrders = JsonConvert.DeserializeObject<ConcurrentDictionary<string, FZOrder>>(items[0]);
            _orders = JsonConvert.DeserializeObject<ConcurrentDictionary<string, FZOrder>>(items[1]).Values.ToList();

        }

        [TestMethod]
        public async Task Test_All()
        {
            {
                var r1 = await _db.GetAllKeyValues();
                await _db.DeleteByKeys(r1.Keys.ToArray());
            }

            var orders = _orders.Take(10).ToList();
            var numCancelled = orders.Count(o => o.State == OrderState.cancelled && o.DealAmount == 0);
            {
                foreach (var o in orders)
                    await _db.AddUpdateByKey(o.OrderId, o);

                var r2 = await _db.GetAllKeyValues();
                Assert.IsNotNull(r2);
                Assert.IsTrue(r2.Count() == orders.Count);
            }

            {
                var o = orders[0];
                o.RefId = "refid1";
                await _db.AddUpdateByKey(o.OrderId, o);
                var r1 = await _db.GetByKey(o.OrderId);
                Assert.IsNotNull(r1);
                Assert.IsTrue(r1.RefId == o.RefId);
            }

            {
                var o = orders[0];
                await _db.DeleteByKey(o.OrderId);
                var r1 = await _db.GetByKey(o.OrderId);
                Assert.IsNull(r1);

                var r2 = await _db.GetAllKeyValues();
                Assert.IsNotNull(r2);
                Assert.IsTrue(r2.Count() == orders.Count-1);
                                
                await _db.DeleteByKeys(r2.Keys.ToArray());
            }
        }
    }
}
