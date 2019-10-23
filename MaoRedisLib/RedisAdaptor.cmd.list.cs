using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MaoRedisLib
{
    public partial class RedisAdaptor
    {
        public JObject RPush(string key, string[] elements)
        {
            string cmd = $"rpush {key.Replace(" ", "%20")}";
            foreach (string element in elements)
            {
                cmd += $" {element.Replace(" ", "%20")}";
            }
            return Interact(cmd);
        }

        public JObject LPush(string key, string[] elements)
        {
            string cmd = $"lpush {key.Replace(" ", "%20")}";
            foreach (string element in elements)
            {
                cmd += $" {element.Replace(" ", "%20")}";
            }
            return Interact(cmd);
        }

        public JObject RPop(string key)
        {
            return Interact($"rpop {key.Replace(" ", "%20")}");
        }

        public JObject LPop(string key)
        {
            return Interact($"lpop {key.Replace(" ", "%20")}");
        }

        public JObject LRange(string key, int start, int stop)
        {
            return Interact($"lrange {key.Replace(" ", "%20")} {start} {stop}");
        }

        public JObject LLen(string key)
        {
            return Interact($"llen {key.Replace(" ", "%20")}");
        }

        public JObject LIndex(string key, int index)
        {
            return Interact($"lindex {key.Replace(" ", "%20")} {index}");
        }

        public JObject LSet(string key, int index, string element)
        {
            return Interact($"lset {key.Replace(" ", "%20")} {index} {element}");
        }

        public JObject LRem(string key, int count, string element)
        {
            return Interact($"lrem {key.Replace(" ", "%20")} {count} {element}");
        }
    }
}
