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

            RedisAdaptor adaptor = new RedisAdaptor("13.231.216.183");
            //RedisAdaptor adaptor = new RedisAdaptor("192.168.3.90");
            //adaptor.Connect();
            adaptor.Connect("MukAzxGMOL2");
            R_Server server = new R_Server(adaptor.Name, new List<R_Database>());
            JObject infoJson = adaptor.Info();

            foreach (JProperty dbInfo in infoJson["data"]["Keyspace"])
            {                
                R_Database db = new R_Database(dbInfo.Name+" (keys="+ dbInfo.Value.ToString().Split(',')[0].Replace("keys=", "") + ")", new List<R_Key>(), new List<R_SubKey>());
                server.Databases.Add(db);
            }
            //{
            //    R_Database db = new R_Database($"db_{i}", new List<R_Key>(), new List<R_SubKey>());
            //    adaptor.UseDB(i);
            //    RedisKey[] keys = adaptor.GetKeys().ToArray();                
            //    foreach (RedisKey key in keys)
            //    {
            //        R_Key _key = new R_Key(key.ToString(), adaptor.Get(key));
            //        db.Keys.Add(_key);
            //    }
            //    db.Name += $"db_{i} ({keys.Length})";
            //    server.Databases.Add(db);
            //}
            //List<R_Server> servers = new List<R_Server>() { server };
            MyMenuItems.ItemsSource = servers;
            MyTreeViewItems.Header = server.Name;
            MyTreeViewItems.ItemsSource = server.Databases;

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
