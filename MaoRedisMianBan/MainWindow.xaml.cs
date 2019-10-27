using System;
using System.Collections.Generic;
using System.Diagnostics;
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

            MyTreeViewItems.ItemsSource = servers;
        }

        public void RefreshTree()
        {
            MyTreeViewItems.Items.Refresh();
        }
        public void RemoveServer(R_Server server) {
            servers.Remove(server);
            RefreshTree();
        }

        

        private void BTN_Add_Click(object sender, RoutedEventArgs e)
        {
            AddServerDlg dlg = new AddServerDlg();
            dlg.Owner = this;
            if ((bool)dlg.ShowDialog())
            {
                string addr = dlg.Addr;
                ushort port = dlg.Port;
                string serverName = dlg.ServerName;
                if (serverName == "")
                    serverName = addr + ":" + port;
                string psw = dlg.Password;
                R_Server server = new R_Server(serverName, new List<R_Database>())
                {
                    Addr = addr,
                    Port = port,
                    Psw = psw
                };

                if (servers.Exists(x => x.Addr == server.Addr && x.Port == server.Port))
                {
                    servers.Find(x => x.Addr == server.Addr && x.Port == server.Port).Databases = server.Databases;
                }
                else
                {
                    servers.Add(server);
                }
                RefreshTree();
            }
        }

        private void AddServerDlg_Close_Click(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
