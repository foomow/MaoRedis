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

        public string Result { get; set; }
        private readonly BackgroundWorker worker;

        private int totalbyte = 0;

        public ConnectDlg(R_Server server)
        {
            Server = server;

            InitializeComponent();

            worker = new BackgroundWorker
            {
                WorkerSupportsCancellation = true,
                WorkerReportsProgress = true
            };
            worker.DoWork += worker_DoWork;
            worker.ProgressChanged += worker_ProgressChanged;

            worker.RunWorkerAsync();
        }

        private void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage == -1)
            {
                worker.CancelAsync();
                Result = "success";
                DialogResult = true;
                Close();
            }
            else if (e.ProgressPercentage == -2)
            {
                worker.CancelAsync();
                Result = "fail";
                DialogResult = false;
                Close();
            }
            else if (e.ProgressPercentage == -3)
            {
                worker.CancelAsync();
                Result = "noauth";
                DialogResult = false;
                Close();
            }
            else
            {
                totalbyte += e.ProgressPercentage;
                ReceivedDataText.Text = totalbyte.ToString();
            }

        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (Server.MIHeader == "Connect")
            {
                Server.Connect((i) =>
                 {
                     (sender as BackgroundWorker).ReportProgress(i);
                     return 1;
                 });
            }
            else
            {
                Server.Disconnect();
                (sender as BackgroundWorker).ReportProgress(-1);
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (!worker.CancellationPending) e.Cancel = true;
            base.OnClosing(e);
        }
    }
}
