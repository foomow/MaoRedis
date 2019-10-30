using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MaoRedisLib
{
    public partial class RedisAdaptor
    {
        public JObject RPush(string key, string[] elements, Func<int, int> report = null)
        {
            string cmd = $"rpush {key.Replace(" ", "%20")}";
            foreach (string element in elements)
            {
                cmd += $" {element.Replace(" ", "%20")}";
            }
            JObject ret = Interact(cmd,report);
            report?.Invoke(-1);
            return ret;
        }

        public JObject LPush(string key, string[] elements, Func<int, int> report = null)
        {
            string cmd = $"lpush {key.Replace(" ", "%20")}";
            foreach (string element in elements)
            {
                cmd += $" {element.Replace(" ", "%20")}";
            }
            JObject ret = Interact(cmd,report);
            report?.Invoke(-1);
            return ret;
        }

        public JObject RPop(string key, Func<int, int> report = null)
        {
            JObject ret = Interact($"rpop {key.Replace(" ", "%20")}",report);
            report?.Invoke(-1);
            return ret;
        }

        public JObject LPop(string key, Func<int, int> report = null)
        {
            JObject ret = Interact($"lpop {key.Replace(" ", "%20")}",report);
            report?.Invoke(-1);
            return ret;
        }

        public JObject LRange(string key, int start, int stop, Func<int, int> report = null)
        {
            JObject ret = Interact($"lrange {key.Replace(" ", "%20")} {start} {stop}",report);
            report?.Invoke(-1);
            return ret;
        }

        public JObject LLen(string key, Func<int, int> report = null)
        {
            JObject ret = Interact($"llen {key.Replace(" ", "%20")}",report);
            report?.Invoke(-1);
            return ret;
        }

        public JObject LIndex(string key, int index, Func<int, int> report = null)
        {
            JObject ret = Interact($"lindex {key.Replace(" ", "%20")} {index}",report);
            report?.Invoke(-1);
            return ret;
        }

        public JObject LSet(string key, int index, string element, Func<int, int> report = null)
        {
            JObject ret = Interact($"lset {key.Replace(" ", "%20")} {index} {element}",report);
            report?.Invoke(-1);
            return ret;
        }

        public JObject LRem(string key, int count, string element, Func<int, int> report = null)
        {
            JObject ret = Interact($"lrem {key.Replace(" ", "%20")} {count} {element}",report);
            report?.Invoke(-1);
            return ret;
        }
    }
}
