using Enyim.Caching;
using Enyim.Caching.Memcached;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace CWC.CachedManager
{
    public class MemcachedCacheManager : ICacheManager
    {
        private int defaultExpireMin = Convert.ToInt32(ConfigurationManager.AppSettings["DefaultCacheTimeOut"] ?? "15");

        private static MemcachedClient Cache;

        public MemcachedCacheManager()
        {
            bool flag = MemcachedCacheManager.Cache == null;
            if (flag)
            {
                MemcachedCacheManager.Cache = new MemcachedClient();
            }
        }

        public bool Add(string key, object value, int expireMin)
        {
            return MemcachedCacheManager.Cache.Store(StoreMode.Set, key, value, new TimeSpan(0, expireMin, 0));
        }

        public bool Add(string key, object value)
        {
            return MemcachedCacheManager.Cache.Store(StoreMode.Set, key, value, new TimeSpan(0, this.defaultExpireMin, 0));
        }

        public bool Add(string key, object value, TimeSpan timeSpan)
        {
            return MemcachedCacheManager.Cache.Store(StoreMode.Set, key, value, timeSpan);
        }

        public bool Add(string key, object value, DateTime absDt)
        {
            return MemcachedCacheManager.Cache.Store(StoreMode.Set, key, value, absDt);
        }

        public object Get(string key)
        {
            return MemcachedCacheManager.Cache.Get(key);
        }

        public T Get<T>(string key)
        {
            return MemcachedCacheManager.Cache.Get<T>(key);
        }

        public IDictionary<string, object> Get(params string[] keys)
        {
            bool flag = keys != null;
            IDictionary<string, object> result;
            if (flag)
            {
                result = MemcachedCacheManager.Cache.Get(keys);
            }
            else
            {
                result = new Dictionary<string, object>();
            }
            return result;
        }

        public object Remove(string key)
        {
            return MemcachedCacheManager.Cache.Remove(key);
        }

        public ulong Increment(string key, ulong defaultValue, ulong min)
        {
            return MemcachedCacheManager.Cache.Increment(key, defaultValue, min);
        }
    }
}