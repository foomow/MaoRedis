using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MaoRedisLib
{
    public partial class RedisAdaptor
    {
        public bool Connect(string password = "", Func<int, int> report = null)
        {
            try
            {
                IPEndPoint ipe = new IPEndPoint(IPAddress.Parse(mIP), mPort);
                R_Socket = new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                R_Socket.Connect(ipe);

                if (R_Socket.Connected)
                {
                    Logger.Info($"{mIP.ToString()}:{mPort} connected");
                    if (password != "")
                    {
                        Logger.Info("auth result:" + Interact("auth " + password,null));
                    }
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
            report?.Invoke(-1);
            return true;
        }
        public JObject Info(string section = "all", Func<int, int> report = null)
        {
            JObject ret = Interact($"info {section}", report);
            report?.Invoke(-1);
            return ret;
        }

        public JObject UseDB(int db_number, Func<int, int> report = null)
        {
            JObject ret = Interact($"select {db_number}", report);
            report?.Invoke(-1);
            return ret;
        }

        public JObject GetKeys(string pattern = "*", Func<int, int> report = null)
        {
            JObject ret = Interact($"keys {pattern}", report);
            return ret;
        }

        public JObject ScanKeys(int cursor, int count = 10, Func<int, int> report = null)
        {
            JObject ret = Interact($"scan {cursor} match * count {count}", report);
            report?.Invoke(-1);
            return ret;
        }

        public JObject Get(string key, Func<int, int> report = null)
        {
            JObject ret;
            JObject typeJson = KeyType(key);
            if (typeJson["data"].ToString() == "hash")
                ret = Interact($"hgetall {key.Replace(" ", "%20")}", report);
            else if (typeJson["data"].ToString() == "list")
            {
                JObject retJson = Interact($"llen {key.Replace(" ", "%20")}", report);
                int endIdx = int.Parse(retJson["data"].ToString());
                ret = Interact($"lrange {key.Replace(" ", "%20")} 0 {endIdx}", report);
            }
            else if (typeJson["data"].ToString() == "sort")
                ret = Interact($"smembers {key.Replace(" ", "%20")}", report);
            else
                ret = Interact($"get {key.Replace(" ", "%20")}", report);
            report?.Invoke(-1);
            return ret;
        }

        public JObject Set(string key, string value, Func<int, int> report = null)
        {
            JObject ret = Interact($"set {key.Replace(" ", "%20")} {value.Replace(" ", "%20")}", report);
            report?.Invoke(-1);
            return ret;
        }

        public JObject Del(IEnumerable<string> keys, Func<int, int> report = null)
        {
            string cmd = $"del";
            foreach (string key in keys)
            {
                cmd += $" {key.Replace(" ", "%20")}";
            }
            JObject ret = Interact(cmd, report);
            report?.Invoke(-1);
            return ret;
        }

        public JObject KeyType(string key, Func<int, int> report = null)
        {
            JObject ret = Interact($"type {key.Replace(" ", "%20")}", report);
            report?.Invoke(-1);
            return ret;
        }
    }
}
