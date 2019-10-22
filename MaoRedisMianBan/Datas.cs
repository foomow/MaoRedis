using System;
using System.Collections.Generic;
using System.Text;

namespace MaoRedisMianBan
{
    public class R_Server
    {
        public string Name { get; set; }
        public List<R_Database> Databases { get; set; }

        public R_Server(string name, List<R_Database> databases)
        {
            Name = name;
            Databases = databases;
        }
    }

    public class R_Database
    {
        public string Name { get; set; }
        public List<R_Key> Keys { get; set; }
        public List<R_SubKey> SubKeys { get; set; }

        public R_Database(string name, List<R_Key> keys,List<R_SubKey> subKeys)
        {
            Name = name;
            Keys = keys;
            SubKeys = subKeys;
        }
    }

    public class R_Key
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public R_Key(string name,string value)
        {
            Name = name;
            Value = value;
        }
    }

    public class R_SubKey
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public List<R_SubKey> SubKeys { get; set; }

        public R_SubKey(string name, string value, List<R_SubKey> subKeys)
        {
            Name = name;
            Value = value;
            SubKeys = subKeys;
        }
    }
}
