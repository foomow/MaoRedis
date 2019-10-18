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
            Console.WriteLine("Hello World!");
            RedisAdaptor adaptor = new RedisAdaptor("13.231.216.183:6379", "MukAzxGMOL2");
            adaptor.Connect();
            adaptor.GetDBCount();
            adaptor.UseDB(0);
            RedisKey[] keys = adaptor.GetKeys().ToArray();
            foreach (RedisKey key in keys)
            {
                Logger.Warn(key);
            }
        }
    }
}
