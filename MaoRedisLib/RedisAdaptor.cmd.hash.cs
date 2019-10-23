using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MaoRedisLib
{
    public partial class RedisAdaptor
    {
        public JObject GetHashAll(string key)
        {
            return Interact($"hgetall {key.Replace(" ", "%20")}");
        }

        public JObject GetHash(string key, string field)
        {
            return Interact($"hget {key.Replace(" ", "%20")} {field.Replace(" ", "%20")}");
        }

        public JObject SetHash(string key, IEnumerable<KeyValuePair<string, string>> kvs)
        {
            string cmd = $"hset {key}";
            if (kvs.Count() > 1) cmd = $"hmset {key}";
            foreach (KeyValuePair<string, string> kv in kvs)
            {
                cmd += $" {kv.Key.Replace(" ", "%20")} {kv.Value.Replace(" ", "%20")}";
            }
            return Interact(cmd);
        }
    }
}
