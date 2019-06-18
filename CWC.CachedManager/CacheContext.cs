using System;
using System.Collections.Generic;
using System.Configuration;

namespace CWC.CachedManager
{
    public static class CacheContext
    {
        private static string CacheManagerType = ConfigurationManager.AppSettings["CacheManagerType"] ?? "Default";

        private static Dictionary<string, ICacheManager> dict = new Dictionary<string, ICacheManager>();

        public static ICacheManager Current
        {
            get
            {
                bool flag = !CacheContext.dict.ContainsKey(CacheContext.CacheManagerType);
                if (flag)
                {
                    string cacheManagerType = CacheContext.CacheManagerType;
                    if (!(cacheManagerType == "Default"))
                    {
                        if (!(cacheManagerType == "Memcached"))
                        {
                            if (!(cacheManagerType == "Redis"))
                            {
                                throw new NotImplementedException("未找到相关缓存引擎信息");
                            }
                            CacheContext.dict[CacheContext.CacheManagerType] = new RedisCacheManager();
                        }
                        else
                        {
                            CacheContext.dict[CacheContext.CacheManagerType] = new MemcachedCacheManager();
                        }
                    }
                    else
                    {
                        CacheContext.dict[CacheContext.CacheManagerType] = new DNTCacheManager();
                    }
                }
                return CacheContext.dict[CacheContext.CacheManagerType];
            }
        }
    }
}