using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web;
using System.Web.Caching;

namespace CWC.CachedManager
{
    public class DNTCacheManager : ICacheManager
    {
        private Cache cache = (HttpContext.Current == null) ? new Cache() : HttpContext.Current.Cache;

        private double defaultExpireMin = (double)Convert.ToInt32(ConfigurationManager.AppSettings["DefaultCacheTimeOut"] ?? "15");

        public bool Add(string key, object value, int expireMin)
        {
            return this.cache.Add(key, value, null, DateTime.Now.AddMinutes((double)expireMin), TimeSpan.Zero, CacheItemPriority.Normal, null) != null;
        }

        public bool Add(string key, object value)
        {
            return this.cache.Add(key, value, null, DateTime.Now.AddMinutes(this.defaultExpireMin), TimeSpan.Zero, CacheItemPriority.Normal, null) != null;
        }

        public bool Add(string key, object value, TimeSpan timeSpan)
        {
            return this.cache.Add(key, value, null, Cache.NoAbsoluteExpiration, timeSpan, CacheItemPriority.Normal, null) != null;
        }

        public bool Add(string key, object value, DateTime absDt)
        {
            return this.cache.Add(key, value, null, absDt, Cache.NoSlidingExpiration, CacheItemPriority.Normal, null) != null;
        }

        public object Get(string key)
        {
            return this.cache[key];
        }

        public T Get<T>(string key)
        {
            bool flag = this.cache[key] == null;
            T result;
            if (flag)
            {
                result = default(T);
            }
            else
            {
                result = (T)((object)this.cache[key]);
            }
            return result;
        }

        public IDictionary<string, object> Get(params string[] keys)
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            for (int i = 0; i < keys.Length; i++)
            {
                string key = keys[i];
                dictionary.Add(key, this.cache[key]);
            }
            return dictionary;
        }

        public object Remove(string key)
        {
            return this.cache.Remove(key);
        }
    }
}