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
        private readonly App theApp;

        public MainWindow()
        {
            InitializeComponent();
            theApp = (App)Application.Current;
            MyTreeViewItems.ItemsSource = theApp.ServerPool;
        }
        
        public void UI_RefreshTree()
        {
            MyTreeViewItems.Items.Refresh();
        }

        private void UI_FocusItem(R_Record record)
        {
            string path = "db" + ((R_Folder)record).Pattern;
            string[] segs = path.Split(':');
            TreeViewItem item = (TreeViewItem)MyTreeViewItems.ItemContainerGenerator.ContainerFromItem(((R_Folder)record).Server);
            item.IsExpanded = true;
            MyTreeViewItems.UpdateLayout();
            R_Folder folder = ((R_Folder)record).Server.Databases.Find(x => x.Name == segs[0]);
            if (folder != null)
            {
                item = (TreeViewItem)item.ItemContainerGenerator.ContainerFromItem(folder);
                if (item != null)
                {
                    for (int i = 1; i < segs.Length; i++)
                    {
                        item.IsExpanded = true;
                        MyTreeViewItems.UpdateLayout();
                        string seg = segs[i];
                        folder = (R_Folder)folder.Records.Find(x => x.Name == seg);
                        if (folder != null)
                        {
                            TreeViewItem nextitem = (TreeViewItem)item.ItemContainerGenerator.ContainerFromItem(folder);
                            if (nextitem != null)
                            {
                                item = nextitem;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
            }
            item.IsExpanded = true;
            item.Focus();
            item.IsSelected = true;
        }

        public void RemoveServer(R_Server server)
        {
            theApp.ServerPool.Remove(server);
            UI_RefreshTree();
        }

        private void BTN_Add_Click(object sender, RoutedEventArgs e)
        {
            AddServerDlg dlg = new AddServerDlg();
            dlg.Owner = this;
            if ((bool)dlg.ShowDialog())
            {
                string addr = dlg.Addr;
                ushort port = dlg.Port;
                string name = dlg.ServerName;
                if (name == "")
                    name = addr + ":" + port;
                string psw = dlg.Password;

                R_Server server = theApp.AddServer(name, addr, port, psw);
                TreeViewItem item = (TreeViewItem)MyTreeViewItems.ItemContainerGenerator.ContainerFromItem(server);
                if (item != null)
                {
                    item.Focus();
                    item.IsSelected = true;
                }
                UI_RefreshTree();
            }
        }

        private void MI_ServerConnect(object sender, RoutedEventArgs e)
        {
            MenuItem item = (MenuItem)sender;
            R_Server server = (R_Server)item.DataContext;
            if (item.Header.ToString() == "Connect")
            {
                server.Connect();
            }
            else
            {
                server.Disconnect();
            }
            UI_RefreshTree();
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
                UI_RefreshTree();
            }
        }

        private void MI_FolderRefresh(object sender, RoutedEventArgs e)
        {
            R_Record record = (R_Record)((MenuItem)sender).DataContext;
            RefreshFolder(record);
        }

        private void RefreshFolder(R_Record record)
        {
            if (record.GetType() == typeof(R_Folder) || record.GetType() == typeof(R_Database))
            {
                theApp.RefreshFolder(record);
                UI_RefreshTree();
                UI_FocusItem(record);
            }
        }

        private void MI_KeyDelete(object sender, RoutedEventArgs e)
        {

        }

        private void MI_KeyReload(object sender, RoutedEventArgs e)
        {
            R_Key key = (R_Key)((MenuItem)sender).DataContext;
            string ret=theApp.GetKey(key);
            logwnd.Text = ret;
        }

        private void MI_FolderDelete(object sender, RoutedEventArgs e)
        {

        }
    }
}
