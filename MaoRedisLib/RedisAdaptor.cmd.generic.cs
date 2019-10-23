using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace MaoRedisLib
{
    public partial class RedisAdaptor
    {
        public JObject Info()
        {
            return Interact($"info");
        }

        public JObject UseDB(int db_number)
        {
            return Interact($"select {db_number}");
        }

        public JObject GetKeys()
        {
            JObject ret = Interact("keys *");
            Logger.Info("keys result:" + ret);
            return ret;
        }

        public JObject ScanKeys(int cursor, int count = 10)
        {
            JObject ret = Interact($"scan {cursor} count {count}");
            Logger.Info("scan result:" + ret);
            return ret;
        }

        public JObject Get(string key)
        {
            return Interact($"get {key.Replace(" ", "%20")}");
        }

        public JObject Set(string key, string value)
        {
            return Interact($"set {key.Replace(" ", "%20")} {value.Replace(" ", "%20")}");
        }

        public JObject Del(IEnumerable<string> keys)
        {
            string cmd = $"del";
            foreach (string key in keys)
            {
                cmd += $" {key.Replace(" ", "%20")}";
            }
            return Interact(cmd);
        }

        public JObject KeyType(string key)
        {
            return Interact($"type {key.Replace(" ", "%20")}");
        }
    }
}
