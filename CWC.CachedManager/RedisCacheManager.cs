using Log.SDK;
using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Web;

namespace CWC.CachedManager
{
    public class RedisCacheManager : ICacheManager
    {
        private readonly PooledRedisClientManager pool = null;

        private readonly string[] redisHosts = null;

        public int RedisMaxReadPool = int.Parse(ConfigurationManager.AppSettings["redis_max_read_pool"]);

        public int RedisMaxWritePool = int.Parse(ConfigurationManager.AppSettings["redis_max_write_pool"]);

        private int defaultExpireMin = Convert.ToInt32(ConfigurationManager.AppSettings["DefaultCacheTimeOut"] ?? "15");

        private static LogHelper logger = LogHelper.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        public RedisCacheManager()
        {
            string text = ConfigurationManager.AppSettings["redis_server_session"];
            bool flag = !string.IsNullOrEmpty(text);
            if (flag)
            {
                this.redisHosts = text.Split(new char[]
                {
                    ','
                });
                bool flag2 = this.redisHosts.Length != 0;
                if (flag2)
                {
                    this.pool = new PooledRedisClientManager(this.redisHosts, this.redisHosts, new RedisClientManagerConfig
                    {
                        MaxWritePoolSize = this.RedisMaxWritePool,
                        MaxReadPoolSize = this.RedisMaxReadPool,
                        AutoStart = true
                    });
                }
            }
        }

        public bool Add(string key, object value, int expireMin)
        {
            bool flag = value == null;
            bool result;
            if (flag)
            {
                result = false;
            }
            else
            {
                bool flag2 = expireMin <= 0;
                if (flag2)
                {
                    this.Remove(key);
                    result = false;
                }
                else
                {
                    try
                    {
                        bool flag3 = this.pool != null;
                        if (flag3)
                        {
                            using (IRedisClient client = this.pool.GetClient())
                            {
                                bool flag4 = client != null;
                                if (flag4)
                                {
                                    client.SendTimeout = 1000;
                                    result = client.Set<object>(key, value, DateTime.Now.AddMinutes((double)expireMin));
                                    return result;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        string msg = string.Format("存入Redis缓存{0}失败，异常消息：{1}，代码堆栈：{2}", key, ex.Message, ex.StackTrace);
                        RedisCacheManager.logger.Info(msg, new object[0]);
                    }
                    result = false;
                }
            }
            return result;
        }

        public bool Add(string key, object value)
        {
            return this.Add(key, value, this.defaultExpireMin);
        }

        public bool Add(string key, object value, TimeSpan timeSpan)
        {
            bool flag = value == null;
            bool result;
            if (flag)
            {
                result = false;
            }
            else
            {
                try
                {
                    bool flag2 = this.pool != null;
                    if (flag2)
                    {
                        using (IRedisClient client = this.pool.GetClient())
                        {
                            bool flag3 = client != null;
                            if (flag3)
                            {
                                client.SendTimeout = 1000;
                                result = client.Set<object>(key, value, DateTime.Now + timeSpan);
                                return result;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    string msg = string.Format("存入Redis缓存{0}失败，异常消息：{1}，代码堆栈：{2}", key, ex.Message, ex.StackTrace);
                    RedisCacheManager.logger.Info(msg, new object[0]);
                }
                result = false;
            }
            return result;
        }

        public bool Add(string key, object value, DateTime absDt)
        {
            return this.Add(key, value, absDt - DateTime.Now);
        }

        public object Get(string key)
        {
            return this.Get<object>(key);
        }

        public T Get<T>(string key)
        {
            bool flag = string.IsNullOrEmpty(key);
            T result;
            if (flag)
            {
                result = default(T);
            }
            else
            {
                T t = default(T);
                try
                {
                    bool flag2 = this.pool != null;
                    if (flag2)
                    {
                        using (IRedisClient client = this.pool.GetClient())
                        {
                            bool flag3 = client != null;
                            if (flag3)
                            {
                                client.SendTimeout = 1000;
                                t = client.Get<T>(key);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    string msg = string.Format("获取Redis缓存{0}失败，异常消息：{1}，代码堆栈：{2}", key, ex.Message, ex.StackTrace);
                    RedisCacheManager.logger.Info(msg, new object[0]);
                }
                result = t;
            }
            return result;
        }

        public IDictionary<string, object> Get(params string[] keys)
        {
            IDictionary<string, object> result = null;
            try
            {
                bool flag = this.pool != null;
                if (flag)
                {
                    using (IRedisClient client = this.pool.GetClient())
                    {
                        bool flag2 = client != null;
                        if (flag2)
                        {
                            client.SendTimeout = 1000;
                            result = client.GetAll<object>(keys);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string msg = string.Format("获取Redis缓存{0}失败，异常消息：{1}，代码堆栈：{2}", keys, ex.Message, ex.StackTrace);
                RedisCacheManager.logger.Info(msg, new object[0]);
            }
            return result;
        }

        public object Remove(string key)
        {
            object result;
            try
            {
                bool flag = this.pool != null;
                if (flag)
                {
                    using (IRedisClient client = this.pool.GetClient())
                    {
                        bool flag2 = client != null;
                        if (flag2)
                        {
                            client.SendTimeout = 1000;
                            result = client.Remove(key);
                            return result;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string msg = string.Format("删除Redis缓存{0}失败，异常消息：{1}，代码堆栈：{2}", key, ex.Message, ex.StackTrace);
                RedisCacheManager.logger.Info(msg, new object[0]);
            }
            result = false;
            return result;
        }
    }
}