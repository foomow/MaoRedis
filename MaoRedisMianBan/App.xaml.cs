using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MaoRedisMianBan
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void MI_ServerConnect(object sender, RoutedEventArgs e)
        {
            MenuItem item = (MenuItem)sender;
            R_Server server=  (R_Server)item.DataContext;
            if (item.Header.ToString() == "Connect")
            {
                server.Connect();
                ((MainWindow)MainWindow).RefreshTree();
                
            }
            else
            {
                server.Disconnect();
                ((MainWindow)MainWindow).RefreshTree();
            }
        }

        private void MI_ServerRemove(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Do you want to remove this server?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                MenuItem item = (MenuItem)sender;
                R_Server server = (R_Server)item.DataContext;
                ((MainWindow)MainWindow).RemoveServer(server);
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
            dlg.Owner = MainWindow;
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
                ((MainWindow)MainWindow).RefreshTree();
            }
        }
    }
}
