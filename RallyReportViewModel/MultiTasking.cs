using System;
using System.Windows.Data;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvanRallyReport.Helpers;
using System.Globalization;

namespace AvanRallyReport.ViewModel
{
   
    public partial class RallyReportViewModel : INotifyPropertyChanged
    {
        
        BackgroundWorker _bwLogin = new BackgroundWorker
        {
            WorkerSupportsCancellation = true
        };
        BackgroundWorker _bwReleaseStory = new BackgroundWorker
        {
            WorkerSupportsCancellation = true
        };
        BackgroundWorker _bwReport = new BackgroundWorker
        {
            WorkerSupportsCancellation = true
        };
        private void bw_LoginDoWork(object sender, DoWorkEventArgs e)
        {
            LoggedIn = RallyReportHelper.Login(txtUserID, txtPassword);
            cmbProjects = GetProjects();
        }
        private void bw_LoginRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            InProgress = false;
        }
        private void bw_ReleaseStoryDoWork(object sender, DoWorkEventArgs e)
        {
            cmbReleases = GetReleases();
            cmbSprints = GetSprints();
        }
        private void bw_ReleaseStoryRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            InProgress = false;
        }
        private void bw_ReportDoWork(object sender, DoWorkEventArgs e)
        {
            PopulateGrid();
        }
        private void bw_ReportRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            InProgress = false;
        }
    }
}
