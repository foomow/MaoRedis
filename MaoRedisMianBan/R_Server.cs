using MaoRedisLib;
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

        public void Connect(Func<int, int> report)
        {
            _redis = new RedisAdaptor(_addr, _port);
            _redis.Connect(_psw);
            JObject infoJson = _redis.Info("Keyspace");
            if (infoJson["result"].ToString() == "error") return;
            JToken dbList = infoJson["data"]["Keyspace"];
            int progress = 1;
            int total = ((JContainer)dbList).Count;
            foreach (JProperty dbInfo in dbList)
            {
                int db_number = int.Parse(dbInfo.Name.Replace("db", ""));
                R_Database db = new R_Database(dbInfo.Name, new List<R_Record>(), this);
                db.Pattern = db_number.ToString();
                db.Count = int.Parse(dbInfo.Value.ToString().Split(",")[0].Remove(0, 5));

                ArrangeKeys(db_number, "*", db);

                Databases.Add(db);
                report(100 * progress / total);
                progress++;
            }
            foreach (R_Folder folder in Databases)
                SortFolder(folder);
        }

        //public void Connect(string addr, ushort port = 6379, string psw = "")
        //{
        //    _addr = addr;
        //    _port = port;
        //    _psw = psw;
        //    Connect();
        //}

        public string GetKey(R_Key key)
        {
            _redis.UseDB(key.Database_Number);
            return _redis.Get(key.Name).ToString();
        }

        public void RefreshKeys(R_Folder folder)
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

            ArrangeKeys(db_number, pattern, folder);

            foreach (R_Folder subfolder in Databases)
                SortFolder(subfolder);
        }

        private void ArrangeKeys(int db_number, string pattern, R_Folder folder)
        {
            _redis.UseDB(db_number);
            JObject keysJson = _redis.GetKeys(pattern);
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
