using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MaoRedisLib
{
    public partial class RedisAdaptor
    {
        public JObject SAdd(string key, string[] elements, Func<int, int> report = null)
        {
            string cmd = $"sadd {key.Replace(" ", "%20")}";
            foreach (string element in elements)
            {
                cmd += $" {element.Replace(" ", "%20")}";
            }
            JObject ret = Interact(cmd,report);
            report?.Invoke(-1);
            return ret;
        }

        public JObject SMembers(string key, Func<int, int> report = null)
        {
            JObject ret = Interact($"smembers {key.Replace(" ", "%20")}",report);
            report?.Invoke(-1);
            return ret;
        }
    }
}
