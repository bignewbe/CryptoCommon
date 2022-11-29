using CryptoCommon.DataBase.Entity;
using CryptoCommon.DataBase.Interface;
using CryptoCommon.DataBase;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoCommonUnitTest
{
    [TestClass]
    public class KeyValueDbTests_string
    {
        private KeyValueDb<string> _db;

        public KeyValueDbTests_string()
        {
            var connectionStr = $"Data Source=47.90.96.154,1306;Initial Catalog=RepoTest;Persist Security Info=True;TrustServerCertificate=True;User ID=SA;Password=Password!";
            var repo = new Repo<KeyValueEnt<string>, ApplicationDbContext>(connectionStr);
            _db = new KeyValueDb<string>(repo);
        }

        [TestMethod]
        public async Task Test_All()
        {
            {
                var r1 = await _db.GetAllKeyValues();
                await _db.DeleteByKeys(r1.Keys.ToArray());
            }

            {
                await _db.AddUpdateByKey("key1", "content1");
                var r1 = await _db.GetByKey("key1");
                Assert.IsNotNull(r1);
                Assert.IsTrue(r1 == "content1");

                var r2 = await _db.GetAllKeyValues();
                Assert.IsNotNull(r2);
                Assert.IsTrue(r2.Count() == 1);
            }

            {
                await _db.AddUpdateByKey("key1", "content2");
                var r1 = await _db.GetByKey("key1");
                Assert.IsNotNull(r1);
                Assert.IsTrue(r1 == "content2");
            }

            {
                await _db.DeleteByKey("key1");
                var r1 = await _db.GetByKey("key1");
                Assert.IsNull(r1);
            }
        }
    }
}
