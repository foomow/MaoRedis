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
using StackExchange.Redis;

namespace MaoRedisMianBan
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<Server> servers = new List<Server>();
        public MainWindow()
        {
            InitializeComponent();

            //RedisAdaptor adaptor = new RedisAdaptor("13.231.216.183:6379", "MukAzxGMOL2");
            RedisAdaptor adaptor = new RedisAdaptor("192.168.3.90:6379");
            adaptor.Connect();
            Server server = new Server("192.168.3.90:6379", new List<Database>());
            int db_count = adaptor.GetDBCount();
            for (int i = 0; i < db_count; i++)
            {
                Database db = new Database($"db_{i}", new List<Key>(), new List<SubKey>());
                adaptor.UseDB(i);
                RedisKey[] keys = adaptor.GetKeys().ToArray();
                db.Name += $" ({keys.Length})";
                foreach (RedisKey key in keys)
                {
                    Key _key = new Key(key.ToString(), adaptor.Get(key));
                    db.Keys.Add(_key);
                }
                server.Databases.Add(db);
            }
            List<Server> servers = new List<Server>() { server };
            MyMenuItems.ItemsSource = servers;
            MyTreeViewItems.Header = server.Name;
            MyTreeViewItems.ItemsSource = server.Databases;

        }
    }
}
