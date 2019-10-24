using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MaoRedisLib;
using Newtonsoft.Json.Linq;

namespace MaoRedisMianBan
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<R_Server> servers = new List<R_Server>();
        public MainWindow()
        {
            InitializeComponent();

            //RedisAdaptor adaptor = new RedisAdaptor("13.231.216.183");
            //adaptor.Connect("MukAzxGMOL2");
            RedisAdaptor adaptor = new RedisAdaptor("192.168.3.90");
            adaptor.Connect();

            R_Server server = new R_Server(adaptor.Name, new List<R_Database>());
            JObject infoJson = adaptor.Info("Keyspace");

            foreach (JProperty dbInfo in infoJson["data"]["Keyspace"])
            {
                int db_number = int.Parse(dbInfo.Name.Replace("db",""));                
                R_Database db = new R_Database(dbInfo.Name+" (keys="+ dbInfo.Value.ToString().Split(',')[0].Replace("keys=", "") + ")",  new List<R_Record>());
                int keyCount = int.Parse(dbInfo.Value.ToString().Split(',')[0].Replace("keys=", ""));

                JObject useRet=adaptor.UseDB(db_number);
                if (useRet.ContainsKey("data"))
                {
                    int adffd = 0;
                }
                JObject keysJson = adaptor.ScanKeys(0, keyCount);
                JArray keys= new JArray();
                if (keysJson["data"].Type == JTokenType.Array)
                {
                    keys = (JArray)keysJson["data"][1];
                }
                
                foreach (JToken key in keys)
                {
                    string keyName = key.ToString();
                    R_Folder folder = GetFolder(db, keyName);
                    R_Key _key = new R_Key(key.ToString(), null);
                    folder.Records.Add(_key);                    
                }
                server.Databases.Add(db);
            }            
            List<R_Server> servers = new List<R_Server>() { server };
            MyMenuItems.ItemsSource = servers;
            MyTreeViewItems.Header = server.Name;
            foreach(R_Folder folder in server.Databases)
            SortFolder(folder);
            MyTreeViewItems.ItemsSource = server.Databases;
        }
        private void SortFolder(R_Folder folder)
        {
            folder.Records.Sort((x,y)=> {
                if (x.GetType() == typeof(R_Folder) && y.GetType() == typeof(R_Key)) return -1;
                else if (y.GetType() == typeof(R_Folder) && x.GetType() == typeof(R_Key)) return 1;
                else return x.Name.CompareTo(y.Name); });
            List<R_Record> folders = folder.Records.FindAll(x=>x.GetType()==typeof(R_Folder));
            foreach (R_Folder subFolder in folders)
            {
                SortFolder(subFolder);
            }
        }
        private R_Folder GetFolder(R_Folder parentFolder, string keyName)
        {
            if (keyName.Contains(":"))
            {
                int idx = keyName.IndexOf(":");
                string folderName = keyName.Substring(0,idx);
                R_Record subFolder = parentFolder.Records.Find(x => x.Name.Equals(folderName) && x.GetType() == typeof(R_Folder));
                if (subFolder== null)
                {
                    subFolder = new R_Folder(folderName, new List<R_Record>());
                    parentFolder.Records.Add(subFolder);
                }
                return GetFolder((R_Folder)subFolder, keyName.Substring(idx + 1));
            }
            return parentFolder;            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
