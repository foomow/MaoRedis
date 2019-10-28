using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
        private readonly List<R_Server> servers = new List<R_Server>();
        public MainWindow()
        {
            InitializeComponent();
            LoadConfig();
            MyTreeViewItems.ItemsSource = servers;
        }

        private void LoadConfig()
        {
            string FileName = AppContext.BaseDirectory + "/config.json";

            if (File.Exists(FileName))
            {
                try
                {
                    JObject json = JObject.Parse(File.ReadAllText(FileName));
                    if (json.ContainsKey("servers"))
                    {
                        if (json["servers"].Type == JTokenType.Array)
                        {
                            JArray serverJArray = (JArray)json["servers"];
                            foreach (JToken serverToken in serverJArray)
                            {
                                R_Server newserver = new R_Server(serverToken["name"].ToString(), new List<R_Database>())
                                {
                                    Addr = serverToken["addr"].ToString(),
                                    Port = ushort.Parse(serverToken["port"].ToString()),
                                    Psw = serverToken["psw"].ToString()
                                };
                                if (!servers.Exists(x => x.Addr == newserver.Addr && x.Port == newserver.Port))
                                    servers.Add(newserver);
                            }
                        }
                    }
                    return;
                }
                catch
                {
                }

            }
            JObject cfgJson = new JObject();
            JObject server = new JObject();
            server.Add("name", "localhost");
            server.Add("addr", "127.0.0.1");
            server.Add("port", "6379");
            server.Add("psw", "");
            JArray serversJson = new JArray();
            serversJson.Add(server);
            cfgJson.Add("servers", serversJson);
            File.WriteAllText(FileName, cfgJson.ToString());

            R_Server defaultserver = new R_Server(server["name"].ToString(), new List<R_Database>())
            {
                Addr = server["addr"].ToString(),
                Port = ushort.Parse(server["port"].ToString()),
                Psw = server["psw"].ToString()
            };

            servers.Add(defaultserver);
        }

        public void RefreshTree()
        {
            MyTreeViewItems.Items.Refresh();
        }

        public void RemoveServer(R_Server server)
        {
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
                    MessageBox.Show("The server already exists.");
                    R_Server exist_server = servers.Find(x => x.Addr == server.Addr && x.Port == server.Port);
                    TreeViewItem item = (TreeViewItem)MyTreeViewItems.ItemContainerGenerator.ContainerFromItem(exist_server);
                    item.Focus();
                    item.IsSelected = true;
                }
                else
                {
                    servers.Add(server);
                    RefreshTree();
                }

            }
        }

        private void MI_ServerConnect(object sender, RoutedEventArgs e)
        {
            MenuItem item = (MenuItem)sender;
            R_Server server = (R_Server)item.DataContext;
            if (item.Header.ToString() == "Connect")
            {
                server.Connect();
                RefreshTree();

            }
            else
            {
                server.Disconnect();
                RefreshTree();
            }
        }

        private void MI_ServerRemove(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Do you want to remove this server?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                MenuItem item = (MenuItem)sender;
                R_Server server = (R_Server)item.DataContext;
                RemoveServer(server);
            }
        }

        private void MI_ServerEdit(object sender, RoutedEventArgs e)
        {
            MenuItem item = (MenuItem)sender;
            R_Server server = (R_Server)item.DataContext;
            AddServerDlg dlg = new AddServerDlg();
            dlg.TB_Addr.Text = server.Addr;
            dlg.TB_Port.Text = server.Port.ToString();
            dlg.TB_Psw.Text = server.Psw;
            dlg.TB_ServerName.Text = server.Name;
            dlg.BTN_Add.Content = "Edit";
            dlg.Owner = this;
            if ((bool)dlg.ShowDialog())
            {
                string addr = dlg.Addr;
                ushort port = dlg.Port;
                string serverName = dlg.ServerName;
                if (serverName == "")
                    serverName = addr + ":" + port;
                string psw = dlg.Password;

                server.Addr = addr;
                server.Port = port;
                server.Psw = psw;
                server.Name = serverName;
                RefreshTree();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            string FileName = AppContext.BaseDirectory + "/config.json";
            JObject cfgJson = new JObject();
            JArray serversJson = new JArray();
            foreach (R_Server server in servers)
            {                
                JObject serverJson = new JObject();
                serverJson.Add("name", server.Name);
                serverJson.Add("addr", server.Addr);
                serverJson.Add("port", server.Port.ToString());
                serverJson.Add("psw", server.Psw);                
                serversJson.Add(serverJson);                
            }
            cfgJson.Add("servers", serversJson);
            File.WriteAllText(FileName, cfgJson.ToString());
        }

        private void MI_KeyRefresh(object sender, RoutedEventArgs e)
        {
            R_Record record = (R_Record)((MenuItem)sender).DataContext;
            LoadKeys(record);
        }

        private void LoadKeys(R_Record record)
        {
            if (record.GetType() == typeof(R_Folder))
            {
                foreach (R_Record r in ((R_Folder)record).Records)
                {
                    LoadKeys(r);
                }
            }
            else
            {
                R_Key key = (R_Key)record;
                key.Server.LoadKey(key);
            }
        }

        private void MI_KeyDelete(object sender, RoutedEventArgs e)
        {

        }
    }
}
