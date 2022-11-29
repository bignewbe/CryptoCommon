using CryptoCommon.DataBase.Entity;
using CryptoCommon.DataBase.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoCommon.DataBase
{
    public class KeyValueDb<T> : IKeyValueDb<T> where T: IEquatable<T>
    {
        private bool _isBusy;
        private ConcurrentDictionary<string, int> _keyToId = new ConcurrentDictionary<string, int>();

        private IRepo<KeyValueEnt<T>> _repo;
        public KeyValueDb(IRepo<KeyValueEnt<T>> repo)
        {
            _repo = repo;
        }

        public async Task<Dictionary<string, T>> GetAllKeyValues()
        {
            var r = await _repo.GetAllAsync().ConfigureAwait(false);
            _keyToId.Clear();
            foreach (var kv in r) 
                _keyToId.TryAdd(kv.Name, kv.Id);
            return r.ToDictionary(x => x.Name, x => x.Content);
        }

        public async Task DeleteByKey(string key)
        {
            if (await _repo.DeleteByNamesAsync(key).ConfigureAwait(false) > 0)
                _keyToId.TryRemove(key, out _);
        }

        public async Task<T> GetByKey(string key)
        {
            var t = await _repo.GetByNameAsync(key).ConfigureAwait(false);
            if (t != null)
            {
                _keyToId.TryAdd(key, t.Id);
                return t.Content;
            }
            return default;
        }

        public async Task AddUpdateByKey(string key, T content)
        {
            if (_keyToId.ContainsKey(key))
            {
                await _repo.UpdateByIdAsync(_keyToId[key], p => p.SetProperty(x => x.Content, x => content)).ConfigureAwait(false);
            }
            else
            {
                var t = await _repo.GetByNameAsync(key).ConfigureAwait(false);
                if (t != null)
                {
                    _keyToId.TryAdd(key, t.Id);
                    await _repo.UpdateByIdAsync(_keyToId[key], p => p.SetProperty(x => x.Content, x => content)).ConfigureAwait(false);
                }
                else
                {
                    var et = new KeyValueEnt<T> { Name = key, Content = content};
                    await _repo.AddAsync(et).ConfigureAwait(false);
                }
            }
        }

        public async Task<int> DeleteByKeys(params string[] keys)
        {
            if (keys.Length == 0) return 0;

            var num = await _repo.DeleteByNamesAsync(keys).ConfigureAwait(false);
            if (num > 0)
            {
                foreach (var key in keys)
                    _keyToId.TryRemove(key, out var t);
            }
            return num;
        }
    }
}


//public async Task WriteByKey(string key, string content)
//{
//    await _repo.UpdateByNameAsync(key, updates => updates.SetProperty(p => p.Content, p => content)).ConfigureAwait(false);
//}

//public async Task DeleteByKey(string key)
//{
//    if (_keyToId.ContainsKey(key))
//    {
//        await _repo.DeleteByIdAsync(_keyToId[key]).ConfigureAwait(false);
//        _keyToId.TryRemove(key, out _);
//    }
//    else
//    {
//        var t = await _repo.GetByNameAsync(key).ConfigureAwait(false);
//        if (t != null)
//            await _repo.DeleteByIdAsync(t.Id).ConfigureAwait(false);
//    }
//}

//public async Task<string> GetByKey(string key)
//{
//    if (_keyToId.ContainsKey(key))
//    {
//        var t = await _repo.GetByIdAsync(_keyToId[key]).ConfigureAwait(false);
//        return t != null ? t.Content : null;
//    }
//    else
//    {
//        var t = await _repo.GetByNameAsync(key).ConfigureAwait(false);
//        if (t != null)
//        {
//            _keyToId.TryAdd(key, t.Id);
//            return t.Content;
//        }
//        return null;
//    }
//}
