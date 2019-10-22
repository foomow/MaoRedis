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
        List<R_Server> servers = new List<R_Server>();
        public MainWindow()
        {
            InitializeComponent();

            RedisAdaptor adaptor = new RedisAdaptor("13.231.216.183:6379", "MukAzxGMOL2");
            //RedisAdaptor adaptor = new RedisAdaptor("192.168.3.90:6379");
            adaptor.Connect();
            R_Server server = new R_Server(adaptor.Name, new List<R_Database>());
            int db_count = adaptor.GetDBCount();
            for (int i = 0; i < db_count; i++)
            {
                R_Database db = new R_Database($"db_{i}", new List<R_Key>(), new List<R_SubKey>());
                adaptor.UseDB(i);
                RedisKey[] keys = adaptor.GetKeys().ToArray();                
                foreach (RedisKey key in keys)
                {
                    R_Key _key = new R_Key(key.ToString(), adaptor.Get(key));
                    db.Keys.Add(_key);
                }
                db.Name += $"db_{i} ({keys.Length})";
                server.Databases.Add(db);
            }
            List<R_Server> servers = new List<R_Server>() { server };
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
