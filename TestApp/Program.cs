using System;
using System.Linq;
using MaoRedisLib;
using StackExchange.Redis;

namespace TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("************** Hello Chengdu! **************");
            RedisAdaptor adaptor = new RedisAdaptor("13.231.216.183:6379", "MukAzxGMOL2");
            adaptor.Connect();
            int db_count=adaptor.GetDBCount();
            Logger.Info($"Database count:{db_count}");
            int current_db=adaptor.UseDB(0);
            Logger.Info($"Current Database:{current_db}");
            RedisKey[] keys = adaptor.GetKeys().ToArray();
            foreach (RedisKey key in keys)
            {
                Logger.Info(adaptor.KeyType(key)+ " "+key.ToString()+":["+adaptor.Get(key)+"]");
            }
            Console.WriteLine("************** Bye! **************");
        }
    }
}
