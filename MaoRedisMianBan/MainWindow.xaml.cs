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

        private void BTN_Add_Click(object sender, RoutedEventArgs e)
        {
            R_Server server = theApp.AddServer();
            if (server != null)
            {
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
            theApp.ServerConnect(server);
            UI_RefreshTree();
        }

        private void MI_ServerRemove(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Do you want to remove this server?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                MenuItem item = (MenuItem)sender;
                R_Server server = (R_Server)item.DataContext;
                theApp.ServerPool.Remove(server);
                UI_RefreshTree();
            }
        }

        private void MI_ServerEdit(object sender, RoutedEventArgs e)
        {
            MenuItem item = (MenuItem)sender;
            R_Server server = (R_Server)item.DataContext;
            if (theApp.ServerEdit(server)) UI_RefreshTree();
        }

        private void MI_FolderRefresh(object sender, RoutedEventArgs e)
        {
            R_Record record = (R_Record)((MenuItem)sender).DataContext;
            if (record.GetType() == typeof(R_Folder) || record.GetType() == typeof(R_Database))
            {
                theApp.RefreshFolder(record);
                UI_RefreshTree();
                UI_FocusItem(record);
            }
        }

        private void MI_KeyDelete(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Do you want to remove this key?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                R_Key key = (R_Key)((MenuItem)sender).DataContext;
                R_Folder folder = key.Folder;
                string ret = theApp.DeleteKey(key);
                try
                {
                    JObject retJson = JObject.Parse(ret);
                    if (retJson["result"].ToString() == "success")
                    {
                        UI_RefreshTree();
                        UI_FocusItem(folder);
                    }
                }
                catch (Exception ex)
                {
                    ret = ex.Message + "\r\n" + ex.StackTrace;
                }

                logwnd.Text = ret;
            }
        }

        private void MI_KeyReload(object sender, RoutedEventArgs e)
        {
            R_Key key = (R_Key)((MenuItem)sender).DataContext;
            string ret = theApp.GetKey(key);
            logwnd.Text = ret;
        }

        private void MI_FolderDelete(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Do you want to remove all keys in this Namespace?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                R_Folder folder = (R_Folder)((MenuItem)sender).DataContext;

                string ret = theApp.DeleteFolder(folder);
                try
                {
                    JObject retJson = JObject.Parse(ret);
                    if (retJson["result"].ToString() == "success")
                    {
                        UI_RefreshTree();
                        UI_FocusItem(folder);
                    }
                }
                catch (Exception ex)
                {
                    ret = ex.Message + "\r\n" + ex.StackTrace;
                }

                logwnd.Text = ret;
            }
        }

        private void Server_MouseLeftDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount > 1)
            {
                TextBlock item = (TextBlock)sender;
                R_Server server = (R_Server)item.DataContext;
                theApp.ServerConnect(server);
                UI_RefreshTree();
            }
        }

        private void Folder_MouseLeftDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount > 1)
            {
                R_Record record = (R_Record)((StackPanel)sender).DataContext;
                if (record.GetType() == typeof(R_Folder) || record.GetType() == typeof(R_Database))
                {
                    theApp.RefreshFolder(record);
                    UI_RefreshTree();
                    UI_FocusItem(record);
                }
            }
        }

        private void Key_MouseLeftDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount > 1)
            {
                R_Key key = (R_Key)((TextBlock)sender).DataContext;
                string ret = theApp.GetKey(key);
                logwnd.Text = ret;
            }
        }

        //private void MI_ServerAddKey(object sender, RoutedEventArgs e)
        //{
        //    R_Server server = (R_Server)sender;
        //}
    }
}
