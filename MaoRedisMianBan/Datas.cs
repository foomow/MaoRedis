﻿using Newtonsoft.Json.Linq;
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

    public class R_Database : R_Folder
    {
        public new int Count
        {
            get;set;
        }
        public R_Database(string name, List<R_Record> records) : base(name, records)
        {
        }
    }

    public abstract class R_Record
    {
        public string Name { get; set; }
    }

    public class R_Key : R_Record
    {
        public JToken Value { get; set; }

        public R_Key(string name, JToken value)
        {
            Name = name;
            Value = value;
        }
    }

    public class R_Folder : R_Record
    {
        public List<R_Record> Records { get; set; }
        public int Count
        {
            get
            {
                int ret = 0;
                foreach (R_Record record in Records)
                {
                    if (record.GetType() == typeof(R_Key)) ret++;
                    if (record.GetType() == typeof(R_Folder)) ret += ((R_Folder)record).Count;
                }
                return ret;
            }
        }

        public R_Folder(string name, List<R_Record> records)
        {
            Name = name;
            Records = records;
        }
    }
}
