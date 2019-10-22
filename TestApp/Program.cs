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
            RedisAdaptor adaptor = new RedisAdaptor("13.231.216.183");
            adaptor.Connect("MukAzxGMOL2");
            Console.WriteLine("************** Bye! **************");
        }
    }
}
