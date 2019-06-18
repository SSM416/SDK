using System;
using System.Collections.Generic;

namespace CWC.CachedManager
{
    public interface ICacheManager
    {
        bool Add(string key, object value, int expireMin);

        bool Add(string key, object value);

        bool Add(string key, object value, TimeSpan timeSpan);

        bool Add(string key, object value, DateTime absDt);

        [Obsolete("请使用Get的泛型方法Get<T>来获取", true)]
        object Get(string key);

        T Get<T>(string key);

        IDictionary<string, object> Get(params string[] keys);

        object Remove(string key);
    }
}
