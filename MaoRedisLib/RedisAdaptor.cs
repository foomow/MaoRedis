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
            options.Password = _password;
            options.AllowAdmin = true;
            options.ConnectTimeout = 30000;
            options.ResponseTimeout = 30000;
            options.ClientName = "MaoRedis Client";
            try
            {
                Redis = ConnectionMultiplexer.Connect(options);
                _server = Redis.GetServer(_connectionString);
                Logger.Info("Redis connected");
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
            Logger.Warn(ret.ToString());
            return ret;
        }

        public void UseDB(int db_number)
        {
            _current_db = Redis.GetDatabase(db_number);
            _current_db_number = db_number;
        }

        public RedisKey[] GetKeys()
        {
            _server = Redis.GetServer(_connectionString);
            RedisKey[] keys = _server.Keys(_current_db_number).ToArray();
            return keys;
        }
    }
}
