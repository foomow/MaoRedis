using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MaoRedisLib
{
    public class RedisAdaptor
    {
        private string _connectionString;
        private string _password;
        private ConnectionMultiplexer Redis;
        private int _current_db_number;
        private IDatabase _current_db;
        private IServer _server;

        public RedisAdaptor(string connection_string, string password = "")
        {
            _connectionString = connection_string;
            _password = password;
            _current_db_number = 0;
            Logger.Info("Redis initialized");

        }

        public bool Connect()
        {
            ConfigurationOptions options = ConfigurationOptions.Parse(_connectionString);
            if (_password != "")
                options.Password = _password;
            options.AllowAdmin = true;
            options.ConnectTimeout = 30000;
            options.SyncTimeout = 30000;
            options.ResponseTimeout = 30000;
            options.ClientName = "MaoRedis Client";
            try
            {
                Redis = ConnectionMultiplexer.Connect(options);
                _server = Redis.GetServer(_connectionString);
                Logger.Info("Redis connected");
                Logger.Info("Server:"+Redis.GetStatus());
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                Logger.Error(e.StackTrace);
            }
            return true;
        }

        public int GetDBCount()
        {
            string CountStr = Redis.GetStatus()
                .Split(";")
                .First(x => x.EndsWith("databases"))
                .Replace("databases", "")
                .Replace(" ", "");
            int.TryParse(CountStr, out int ret);
            return ret;
        }

        public int UseDB(int db_number)
        {
            _current_db = Redis.GetDatabase(db_number);
            _current_db_number = _current_db.Database;
            return _current_db.Database;
        }

        public RedisKey[] GetKeys()
        {
            _server = Redis.GetServer(_connectionString);
            RedisKey[] keys = _server.Keys(_current_db_number).ToArray();
            return keys;
        }

        public string Get(string key)
        {
            if (KeyType(key).ToString() == "String")
                return _current_db.StringGet(key);
            else
                return $"({KeyType(key).ToString()})";
        }

        public HashEntry[] GetHash(string key)
        {
            return _current_db.HashGetAll(key);
        }

        public void SetHash(string key, HashEntry[] fields)
        {
            _current_db.HashSet(key, fields);
        }

        public RedisValue[] GetListAll(string key)
        {
            return _current_db.ListRange(key);
        }

        public RedisValue GetListByIndex(string key, int index = 0)
        {
            return _current_db.ListGetByIndex(key, index);
        }

        public bool Set(string key, string value)
        {
            return _current_db.SetAdd(key, value);
        }

        public bool Del(string key)
        {
            return _current_db.KeyDelete(key);
        }

        public RedisType KeyType(string key)
        {
            return _current_db.KeyType(key);
        }
    }
}
