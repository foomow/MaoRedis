using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MaoRedisLib
{
    public partial class RedisAdaptor
    {
        public JObject SAdd(string key, string[] elements)
        {
            string cmd = $"sadd {key.Replace(" ", "%20")}";
            foreach (string element in elements)
            {
                cmd += $" {element.Replace(" ", "%20")}";
            }
            return Interact(cmd);
        }

        public JObject SMembers(string key)
        {
            return Interact($"smembers {key.Replace(" ", "%20")}");
        }
    }
}
