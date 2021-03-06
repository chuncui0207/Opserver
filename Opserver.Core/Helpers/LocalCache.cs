﻿using System;
using System.Runtime.Caching;

namespace StackExchange.Opserver.Helpers
{
    public class LocalCache
    {
        private static readonly MemoryCache Cache = new MemoryCache(nameof(LocalCache));

        private readonly object _lock = new object();
        
        public bool Exists(string key)
        {
            return Cache[key] != null;
        }

        /// <summary>
        /// Gets an item of type T from local cache
        /// </summary>
        public T Get<T>(string key)
        {
            var o = Cache[key];
            if (o == null) return default(T);
            if (o is T)
                return (T)o;
            return default(T);
        }

        /// <summary>
        /// Places an item of type T into local cache for the specified duration
        /// </summary>
        public void Set<T>(string key, T value, TimeSpan? duration, bool sliding = false)
        {
            SetWithPriority<T>(key, value, duration, sliding, CacheItemPriority.Default);
        }

        public void SetWithPriority<T>(string key, T value, TimeSpan? duration, bool isSliding, CacheItemPriority priority)
        {
            RawSet(key, value, duration, isSliding, priority);
        }

        private void RawSet(string cacheKey, object value, TimeSpan? duration, bool isSliding, CacheItemPriority priority)
        {
            var policy = new CacheItemPolicy { Priority = priority };
            if (!isSliding && duration.HasValue)
                policy.AbsoluteExpiration = DateTime.UtcNow.Add(duration.Value);
            if (isSliding && duration.HasValue)
                policy.SlidingExpiration = duration.Value;
            
            Cache.Add(cacheKey, value, policy);
        }

        /// <summary>
        /// Removes an item from local cache
        /// </summary>
        public void Remove(string key)
        {
            Cache.Remove(key);
        }
        
        public bool SetNXSync<T>(string key, T val)
        {
            lock (_lock)
            {
                if (Get<T>(key).Equals(default(T)))
                {
                    Set<T>(key, val, null, false);
                    return true;
                }
                return false;
            }
        }
    }
}