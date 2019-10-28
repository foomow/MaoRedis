using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace MaoRedisLib
{
    public partial class RedisAdaptor
    {
        public JObject Info(string section="all")
        {
            return Interact($"info {section}");
        }

        public JObject UseDB(int db_number)
        {            
            return Interact($"select {db_number}");
        }

        public JObject GetKeys(string pattern="*")
        {
            JObject ret = Interact($"keys {pattern}");
            return ret;
        }

        public JObject ScanKeys(int cursor, int count = 10)
        {
            JObject ret = Interact($"scan {cursor} match * count {count}");
            return ret;
        }

        public JObject Get(string key)
        {
            JObject typeJson = Interact($"type {key.Replace(" ", "%20")}");
            if (typeJson["data"].ToString() == "hash")
                return Interact($"hgetall {key.Replace(" ", "%20")}");
            else if (typeJson["data"].ToString() == "list")
            {
                int endIdx=int.Parse(Interact($"llen {key.Replace(" ", "%20")}")["data"].ToString());
                return Interact($"lrange {key.Replace(" ", "%20")} 0 {endIdx}");
            }
            else if(typeJson["data"].ToString() == "sort")
                return Interact($"smembers {key.Replace(" ", "%20")}");
            else
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
