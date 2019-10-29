using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace MaoRedisLib
{
    public static class Logger
    {
        public static void Info(string msg)
        {
            Output(msg, ConsoleColor.White);
        }

        public static void Warn(string msg)
        {
            Output(msg, ConsoleColor.Yellow);
        }

        public static void Error(string msg)
        {
            Output(msg, ConsoleColor.Red);
        }

        private static void Output(string msg, ConsoleColor color)
        {
            string DateString = DateTime.Now.ToString("HH:ss:mm:ffff");
            //Console.ResetColor();
            //Console.Write("[" + DateString + "]");
            //Console.ForegroundColor = color;
            //Console.WriteLine(msg);
            //Console.ResetColor();
            Debug.WriteLine("[" + DateString + "]"+msg);
        }
    }
}
