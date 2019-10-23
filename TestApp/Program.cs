using System;
using System.Collections.Generic;
using System.Linq;
using MaoRedisLib;

namespace TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("************** Hello Chengdu! **************");
            RedisAdaptor adaptor = new RedisAdaptor("13.231.216.183");
            adaptor.Connect("MukAzxGMOL2");
            //RedisAdaptor adaptor = new RedisAdaptor("192.168.3.90");
            //adaptor.Connect();
            adaptor.UseDB(0);

            //adaptor.GetKeys();
            adaptor.ScanKeys(0);            
            //adaptor.SetHash("HashC",new KeyValuePair<string, string>[] { new KeyValuePair<string, string>("name","茂大叔mao"),new KeyValuePair<string, string>("city", "成都cd") });
            //Console.WriteLine(adaptor.Get("message"));
            //Console.WriteLine(adaptor.Get("spacemsg"));
            Console.WriteLine(adaptor.KeyType("HashC"));
            //Console.WriteLine(adaptor.GetHash("HashB","city"));
            Console.WriteLine("************** Bye! **************");
        }
    }
}
