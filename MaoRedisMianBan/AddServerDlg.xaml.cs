using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MaoRedisMianBan
{
    /// <summary>
    /// Interaction logic for AddServerDlg.xaml
    /// </summary>
    public partial class AddServerDlg : Window
    {
        public string Addr { get; set; }
        public ushort Port { get; set; }
        public string ServerName { get; set; }
        public string Password { get; set; }
        public AddServerDlg()
        {
            InitializeComponent();
        }

        private void BTN_Add_Click(object sender, RoutedEventArgs e)
        {
            Addr = TB_Addr.Text;

            ServerName = TB_ServerName.Text.Trim();
            Password = TB_Psw.Text.Trim();

            if (!IPAddress.TryParse(Addr, out IPAddress a))
            {
                MessageBox.Show("Invalid IP Address");
                return;
            }
            try
            {
                Port = ushort.Parse(TB_Port.Text);
                if (Port < 0 || Port > 65535) throw new Exception();
            }
            catch
            {
                MessageBox.Show("Invalid Port");
                return;
            }
            DialogResult = true;
            Close();
        }

        private void BTN_Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void TB_Addr_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            string allowString = "1234567890.";
            int length = e.Text.Length;
            for (int i = 0; i < length; i++)
            {
                string c = e.Text.Substring(i, 1);
                if (allowString.IndexOf(c) == -1)
                {
                    e.Handled = true;
                    return;
                }
            }
            e.Handled = false;
        }

        private void TB_Port_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            string allowString = "1234567890";
            int length = e.Text.Length;
            for (int i = 0; i < length; i++)
            {
                string c = e.Text.Substring(i, 1);
                if (allowString.IndexOf(c) == -1)
                {
                    e.Handled = true;
                    return;
                }
            }
            e.Handled = false;
        }
    }
}
