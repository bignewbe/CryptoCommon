using CryptoCommon.Interfaces;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace CryptoCommon.Models
{
    public class Cache<T> : ICache<T>
    {
        public int MaxCount { get; private set; }

        List<string> _keys = new List<string>();
        ConcurrentDictionary<string, T> _cache = new ConcurrentDictionary<string, T>();

        public Cache(int maxCount)
        {
            this.MaxCount = maxCount;
        }

        public T GetItem(string key)
        {
            if (_cache.ContainsKey(key)) return _cache[key];
            else return default(T);
        }
        public bool RemoveItem(string key)
        {
            T q;
            var r = _cache.TryRemove(key, out q);
            if (r) _keys.Remove(key);
            return r;
        }

        public bool AddItem(string key, T item)
        {
            if (_cache.Count > MaxCount)
            {
                var keys = _keys.GetRange(0, MaxCount / 2);
                foreach (var k in keys)
                    this.RemoveItem(key);
            }

            var r = _cache.TryAdd(key, item);
            if (r) _keys.Add(key);
            return r;
        }
    }
}
