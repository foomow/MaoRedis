﻿using MaoRedisLib;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading;

namespace MaoRedisMianBan
{
    public class R_Server
    {
        public string Name { get; set; }
        public string FontColor
        {
            get
            {
                return Databases.Count == 0 ? "Gray" : "Black";
            }
        }
        public string MIHeader
        {
            get
            {
                return Databases.Count == 0 ? "Connect" : "Disconnect";
            }
        }

        public List<R_Database> Databases { get; set; }

        private RedisAdaptor _redis;
        private string _addr;
        public string Addr { get => _addr; set => _addr = value; }
        private ushort _port;
        public ushort Port { get => _port; set => _port = value; }
        private string _psw;
        public string Psw { get => _psw; set => _psw = value; }

        public R_Server(string name, List<R_Database> databases)
        {
            Name = name;
            Databases = databases;
        }

        public void Disconnect()
        {
            Databases.Clear();
        }

        public bool Connect(Func<int, int> report)
        {
            _redis = new RedisAdaptor(_addr, _port);
            if (!_redis.Connect(_psw)) {
                report?.Invoke(-2);
                return false;
            }
            JObject infoJson = _redis.Info("Keyspace");
            if (infoJson["result"].ToString() == "error")
            {
                report?.Invoke(-3);
                return false;
            }
            JToken dbList = infoJson["data"]["Keyspace"];
            foreach (JProperty dbInfo in dbList)
            {
                int db_number = int.Parse(dbInfo.Name.Replace("db", ""));
                R_Database db = new R_Database(dbInfo.Name, new List<R_Record>(), this);
                db.Pattern = db_number.ToString();
                db.Count = int.Parse(dbInfo.Value.ToString().Split(",")[0].Remove(0, 5));

                Databases.Add(db);
            }
            foreach (R_Folder folder in Databases)
                SortFolder(folder);
            report?.Invoke(-1);
            return true;
        }

        public string GetKey(R_Key key)
        {
            _redis.UseDB(key.Database_Number);
            return _redis.Get(key.Name).ToString();
        }

        public string DeleteKey(R_Key key)
        {
            R_Folder folder = key.Folder;
            _redis.UseDB(key.Database_Number);            
            return _redis.Del(new string[] { key.Name }).ToString();
        }

        public string DeleteKeys(int db,List<string> keynames)
        {
            _redis.UseDB(db);
            return _redis.Del(keynames).ToString();
        }

        public void RefreshKeys(R_Folder folder, Func<int, int> report)
        {
            string pattern = "*";
            int db_number;
            if (folder.Pattern.Contains(":"))
            {
                int idx = folder.Pattern.IndexOf(":");
                db_number = int.Parse(folder.Pattern.Substring(0, idx));
                pattern = folder.Pattern.Substring(idx + 1) + "*";
            }
            else
            {
                db_number = int.Parse(folder.Pattern);
            }

            folder.Records.Clear();

            ArrangeKeys(db_number, pattern, folder,report);

            foreach (R_Folder subfolder in Databases)
                SortFolder(subfolder);

            report?.Invoke(-1);
        }

        private void ArrangeKeys(int db_number, string pattern, R_Folder folder,Func<int, int> report)
        {
            _redis.UseDB(db_number);
            JObject keysJson = _redis.GetKeys(pattern,report);
            JArray keys = new JArray();
            if (keysJson["data"].Type == JTokenType.Array)
            {
                keys = (JArray)keysJson["data"];
            }
            foreach (JToken key in keys)
            {
                string keyName = key.ToString();
                if (pattern != "*")
                    keyName = keyName.Substring(pattern.Replace("*", "").Length).TrimStart(':');
                R_Folder destfolder = GetFolder(folder, keyName);
                R_Key _key = new R_Key(key.ToString(), this, db_number, destfolder);
                destfolder.Records.Add(_key);
            }
        }

        private R_Folder GetFolder(R_Folder parentFolder, string keyName)
        {
            if (keyName.Contains(":"))
            {
                int idx = keyName.IndexOf(":");
                string folderName = keyName.Substring(0, idx);
                R_Record subFolder = parentFolder.Records.Find(x => x.Name.Equals(folderName) && x.GetType() == typeof(R_Folder));
                if (subFolder == null)
                {
                    subFolder = new R_Folder(folderName, new List<R_Record>(), this);
                    ((R_Folder)subFolder).Pattern = parentFolder.Pattern + ":" + folderName;
                    parentFolder.Records.Add(subFolder);
                }
                return GetFolder((R_Folder)subFolder, keyName.Substring(idx + 1));
            }
            return parentFolder;
        }

        private void SortFolder(R_Folder folder)
        {
            folder.Records.Sort((x, y) =>
            {
                if (x.GetType() == typeof(R_Folder) && y.GetType() == typeof(R_Key)) return -1;
                else if (y.GetType() == typeof(R_Folder) && x.GetType() == typeof(R_Key)) return 1;
                else return x.Name.CompareTo(y.Name);
            });
            List<R_Record> folders = folder.Records.FindAll(x => x.GetType() == typeof(R_Folder));
            foreach (R_Folder subFolder in folders)
            {
                SortFolder(subFolder);
            }
        }
    }
}
