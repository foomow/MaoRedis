using System;
using System.Collections.Generic;
using System.Text;

namespace MaoRedisMianBan
{
    public class Server
    {
        public string Name { get; set; }
        public List<Database> Databases { get; set; }

        public Server(string name, List<Database> databases)
        {
            Name = name;
            Databases = databases;
        }
    }

    public class Database
    {
        public string Name { get; set; }
        public List<Key> Keys { get; set; }
        public List<SubKey> SubKeys { get; set; }

        public Database(string name, List<Key> keys,List<SubKey> subKeys)
        {
            Name = name;
            Keys = keys;
            SubKeys = subKeys;
        }
    }

    public class Key
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public Key(string name,string value)
        {
            Name = name;
            Value = value;
        }
    }

    public class SubKey
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public List<SubKey> SubKeys { get; set; }

        public SubKey(string name, string value, List<SubKey> subKeys)
        {
            Name = name;
            Value = value;
            SubKeys = subKeys;
        }
    }
}
