﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MaoRedisLib
{
    public partial class RedisAdaptor
    {
        public JObject GetHashAll(string key, Func<int, int> report = null)
        {
            JObject ret= Interact($"hgetall {key.Replace(" ", "%20")}",report);
            report?.Invoke(-1);
            return ret;
        }

        public JObject GetHash(string key, string field, Func<int, int> report = null)
        {
            JObject ret = Interact($"hget {key.Replace(" ", "%20")} {field.Replace(" ", "%20")}",report);
            report?.Invoke(-1);
            return ret;
        }

        public JObject SetHash(string key, IEnumerable<KeyValuePair<string, string>> kvs, Func<int, int> report = null)
        {
            string cmd = $"hset {key}";
            if (kvs.Count() > 1) cmd = $"hmset {key}";
            foreach (KeyValuePair<string, string> kv in kvs)
            {
                cmd += $" {kv.Key.Replace(" ", "%20")} {kv.Value.Replace(" ", "%20")}";
            }
            JObject ret = Interact(cmd,report);
            report?.Invoke(-1);
            return ret;
        }
    }
}
