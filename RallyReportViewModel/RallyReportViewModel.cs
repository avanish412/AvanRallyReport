using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvanRallyReport.Model;
using AvanRallyReport.DataAccess;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using AvanRallyReport.Helpers;

namespace AvanRallyReport.ViewModel
{
    public partial class RallyReportViewModel: INotifyPropertyChanged
    {
        private ConfigAccess _cfgAcccess = new ConfigAccess();
        private string _userid="";
        private string _password="";
        private bool _loggedin = false;
        List<Project> _projlist = null;
        List<Release> _releaselist = null;
        List<Sprint> _sprintlist = null;
        List<Story> _griditemlist = null;
        private Project _project = null;
        private Release _release=null;
        private Sprint _sprint=null;
        private ReportType _reporttype=ReportType.DAILY_ASSIGNMENT;
        private bool _inProgress = false;
        private bool _tasksIncluded = false;

        public RelayCommand LoginCommand { get; set; }
        public RelayCommand ResetCommand { get; set; }
        public RelayCommand CloseCommand { get; set; }
        public RelayCommand PopulateCommand { get; set; }
        public RelayCommand SelectedProjectChangedCommand { get; set; }

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public string txtUserID
        {
            get { return _userid; }
            set
            {
                _userid = value;
                NotifyPropertyChanged("txtUserID");
            }
        }
        public string txtPassword
        {
            get { return _password; }
            set
            {
                    _password = value;
                    NotifyPropertyChanged("txtPassword");
            }
        }
        public List<Project> cmbProjects
        {
            get { return _projlist; }
            set
            {
                _projlist = value;
                NotifyPropertyChanged("cmbProjects");
            }
        }
        public List<Release> cmbReleases
        {
            get { return _releaselist; }
            set
            {
                _releaselist = value;
                NotifyPropertyChanged("cmbReleases");
            }
        }
        public List<Sprint> cmbSprints
        {
            get { return _sprintlist; }
            set
            {
                _sprintlist = value;
                NotifyPropertyChanged("cmbSprints");
            }
        }
        public List<Story> gridItems
        {
            get { return _griditemlist; }
            set
            {
                _griditemlist = value;
                NotifyPropertyChanged("gridItems");
            }
        }
        public Project SelectedProject
        {
            get { return _project; }
            set
            {
                _project = value;
                NotifyPropertyChanged("SelectedProject");
            }
        }
        public Release SelectedRelease
        {
            get { return _release; }
            set
            {
                _release = value;
                NotifyPropertyChanged("SelectedRelease");  
            }
        }
        public Sprint SelectedSprint
        {
            get { return _sprint; }
            set
            {
                _sprint = value;
                NotifyPropertyChanged("SprintName");
            }
        }
        public ReportType SelectedReportType
        {
            get { return _reporttype; }
            set
            {
                _reporttype = value;
                NotifyPropertyChanged("SelectedReportType");
            }
        }
        public bool LoggedIn
        {
            get { return _loggedin; }
            set
            {
                _loggedin = value;
                NotifyPropertyChanged("LoggedIn");
            }
        }
        public bool InProgress
        {
            get { return _inProgress; }
            set
            {
                _inProgress = value;
                NotifyPropertyChanged("InProgress");
            }
        }
        public bool TasksIncluded
        {
            get { return _tasksIncluded; }
            set
            {
                _tasksIncluded = value;
                NotifyPropertyChanged("TasksIncluded");
            }
        }
        public void Initialize()
        {
            if (_cfgAcccess.LoadConfigData())
            {
                txtUserID = RallyReportHelper.DecryptString(_cfgAcccess.RallyCredential.userid);
                txtPassword = RallyReportHelper.DecryptString(_cfgAcccess.RallyCredential.password);
                cmbProjects = _cfgAcccess.Projects;
                cmbReleases = _cfgAcccess.Releases;
                cmbSprints = _cfgAcccess.Sprints;

                SelectedProject = _cfgAcccess.SelectedProject;
                SelectedRelease = _cfgAcccess.SelectedRelease;
                SelectedSprint = _cfgAcccess.SelectedSprint;
                SelectedReportType = _cfgAcccess.SelectedReportType;
            }
        }
        public RallyReportViewModel()
        {
            Initialize();

            LoginCommand = new RelayCommand(Login);
            ResetCommand = new RelayCommand(Reset);
            CloseCommand = new RelayCommand(Close);
            PopulateCommand = new RelayCommand(Populate);
            SelectedProjectChangedCommand = new RelayCommand(SelectedProjectChanged);

            ///Login Thread
            _bwLogin.DoWork += bw_LoginDoWork;
            _bwLogin.RunWorkerCompleted += bw_LoginRunWorkerCompleted;
            ///Login Thread
            _bwReleaseStory.DoWork += bw_ReleaseStoryDoWork;
            _bwReleaseStory.RunWorkerCompleted += bw_ReleaseStoryRunWorkerCompleted;
            ///Generate Report Thread
            _bwReport.DoWork += bw_ReportDoWork;
            _bwReport.RunWorkerCompleted += bw_ReportRunWorkerCompleted;
        }
        void Login(object parameter)
        {
            InProgress = true;
            _bwLogin.RunWorkerAsync();
        }
        void Reset(object parameter)
        {
            RallyReportHelper.LogOut(); 
            LoggedIn = false;
            cmbProjects = new List<Project>();
            cmbReleases = new List<Release>();
            cmbSprints = new List<Sprint>();
            gridItems = null;
        }
        void SelectedProjectChanged(object parameter)
        {
            InProgress = true;
            _bwReleaseStory.RunWorkerAsync();
        }
        void Close(object parameter)
        {
            SaveWindowInfo();
        }
        void Populate(object parameter)
        {
            InProgress = true;
            _bwReport.RunWorkerAsync();
        }
        public List<Project> GetProjects()
        {
            return RallyReportHelper.GetProjects();
        }
        public List<Release> GetReleases()
        {
            return RallyReportHelper.GetReleases(SelectedProject);
        }
        public List<Sprint> GetSprints()
        {
            return RallyReportHelper.GetSprints(SelectedProject);
        }
        public List<Story> GetStories(bool tasksincluded)
        {
            return RallyReportHelper.GetStories("HierarchicalRequirement", SelectedProject, SelectedRelease, SelectedSprint,tasksincluded);
        }
        public List<Story> GetDefects(bool tasksincluded)
        {
            return RallyReportHelper.GetStories("Defects",SelectedProject, SelectedRelease, SelectedSprint,tasksincluded);
        }
        public void PopulateGrid()
        {
            List<Story> strlist = new List<Story>();

            strlist = GetStories(_tasksIncluded);

            //Get Defects too
            strlist.AddRange(GetDefects(_tasksIncluded));
            gridItems = strlist;
        }
        public void SaveWindowInfo()
        {
            _cfgAcccess.RallyCredential.userid = RallyReportHelper.EncryptString(txtUserID);
            _cfgAcccess.RallyCredential.password = RallyReportHelper.EncryptString(txtPassword);
            _cfgAcccess.Projects = cmbProjects;
            _cfgAcccess.Releases = cmbReleases;
            _cfgAcccess.Sprints = cmbSprints;

            _cfgAcccess.SelectedProject = SelectedProject;
            _cfgAcccess.SelectedRelease = SelectedRelease;
            _cfgAcccess.SelectedSprint = SelectedSprint;
            _cfgAcccess.SelectedReportType = SelectedReportType;

            _cfgAcccess.SavetoConfigData();
        }
    }
}
