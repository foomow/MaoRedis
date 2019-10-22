using System;
using System.Linq;
using MaoRedisLib;

namespace TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("************** Hello Chengdu! **************");
            //RedisAdaptor adaptor = new RedisAdaptor("13.231.216.183");
            //adaptor.Connect("MukAzxGMOL2");
            RedisAdaptor adaptor = new RedisAdaptor("192.168.3.90");
            adaptor.Connect();
            adaptor.UseDB(0);
            adaptor.GetKeys();
            adaptor.ScanKeys(0);
            Console.WriteLine("************** Bye! **************");
        }
    }
}
