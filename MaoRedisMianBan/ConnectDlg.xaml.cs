using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
    /// Interaction logic for ConnectDlg.xaml
    /// </summary>
    public partial class ConnectDlg : Window
    {
        public R_Server Server { get; set; }

        public ConnectDlg(R_Server server)
        {
            Server = server;

            InitializeComponent();

            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += worker_DoWork;
            worker.ProgressChanged += worker_ProgressChanged;

            worker.RunWorkerAsync();
        }

        private void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (PBar.Value >= 100)
            {
                PBar.Value = 100;
                PercentText.Text = "100";
                Close();
            }
            PBar.Value = e.ProgressPercentage;
            PercentText.Text = e.ProgressPercentage.ToString();
            
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (Server.MIHeader == "Connect")
            {
                Server.Connect((i)=> {
                    (sender as BackgroundWorker).ReportProgress(i);
                    return 1;
                });
            }
            else
            {
                Server.Disconnect();
                (sender as BackgroundWorker).ReportProgress(100);
            }
        }

        
    }
}
