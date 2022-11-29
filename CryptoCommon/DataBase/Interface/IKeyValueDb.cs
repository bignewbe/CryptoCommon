using CryptoCommon.DataBase.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoCommon.DataBase.Interface
{
    public interface IKeyValueDb<T>
    {
        Task DeleteByKey(string key);
        Task<int> DeleteByKeys(params string[] keys);
        Task<T> GetByKey(string key);
        Task AddUpdateByKey(string key, T Content);
        Task<Dictionary<string, T>> GetAllKeyValues();
    }
}
