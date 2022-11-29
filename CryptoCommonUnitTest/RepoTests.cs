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
    public class RepoTests 
    {
        private IRepo<KeyValueEnt<string>> _repo;

        public RepoTests() 
        {
            var connectionStr = $"Data Source=47.90.96.154,1306;Initial Catalog=RepoTest;Persist Security Info=True;TrustServerCertificate=True;User ID=SA;Password=Password!";
            _repo = new Repo<KeyValueEnt<string>, ApplicationDbContext>(connectionStr);
        }

        [TestMethod]
        public async Task Test_All()
        {
            var ent = new KeyValueEnt<string> { Name = "name1", Content = "" };
            
            {
                var r1 = await _repo.GetAllIdsAsync();
                await _repo.DeleteByIdsAsync(r1.ToArray());               
            }

            {
                await _repo.AddAsync(ent);
            }

            {
                var r1 = await _repo.GetByNameAsync("name1");
                Assert.IsNotNull(r1);
                Assert.IsTrue(r1.Id > 0 && r1.Name == "name1" && r1.Content == "");
            }

            {
                await _repo.UpdateByNameAsync(ent.Name, u => u.SetProperty(p => p.Content, p => "content1"));
            }

            {
                var r1 = await _repo.GetByNameAsync("name1");
                Assert.IsNotNull(r1);
                Assert.IsTrue(r1.Id > 0 && r1.Name == "name1" && r1.Content == "content1");

                await _repo.UpdateByIdAsync(r1.Id, u => u.SetProperty(p => p.Content, p => "content2"));

                var r2 = await _repo.GetByIdAsync(r1.Id);
                Assert.IsNotNull(r2);
                Assert.IsTrue(r2.Id > 0 && r2.Name == "name1" && r2.Content == "content2");
            }

            {
                var r1 = await _repo.GetByNameAsync("name1");
                Assert.IsNotNull(r1);
                Assert.IsTrue(r1.Id > 0 && r1.Name == "name1" && r1.Content == "content2");
                ent.Id= r1.Id;
            }

            {
                ent.Name = "name2";
                ent.Content = "content3";
                await _repo.UpdateAsync(ent);

                //name is alternate key, wont be updated
                var r1 = await _repo.GetByNameAsync("name2");
                Assert.IsNull(r1);
            }

            {
                var r1 = await _repo.GetByNameAsync("name1");
                Assert.IsTrue(r1.Id == ent.Id  && r1.Name == "name1" && r1.Content == ent.Content);
            }

            {
                var r3 = await _repo.DeleteByNamesAsync("name1");
                Assert.IsTrue(r3 == 1);

                var r2 = await _repo.GetByNameAsync("name1");
                Assert.IsNull(r2);
            }
        }
    }
}
