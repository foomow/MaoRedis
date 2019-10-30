using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading;
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
    /// Interaction logic for ReloadDlg.xaml
    /// </summary>
    public partial class ReloadDlg : Window
    {
        public R_Record Record { get; set; }

        private readonly BackgroundWorker worker;

        private int totalbyte = 0;

        public ReloadDlg(R_Record record)
        {
            Record = record;
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
                DialogResult = true;
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
            ((R_Folder)Record).Server.RefreshKeys((R_Folder)Record, (i) =>
            {
                (sender as BackgroundWorker).ReportProgress(i);
                return 1;
            });
        }
        protected override void OnClosing(CancelEventArgs e)
        {
            if (!worker.CancellationPending) e.Cancel = true;
            base.OnClosing(e);
        }
    }
}
