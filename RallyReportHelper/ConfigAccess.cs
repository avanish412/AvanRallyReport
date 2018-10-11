using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvanRallyReport.Model;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;

namespace AvanRallyReport.DataAccess
{
    [DataContract]
    public class RallyConfig
    {
        [DataMember(Name = "credential")]
        public Credential credential { get; set; }

        [DataMember(Name = "projects")]
        public Project[] projects { get; set; }

        [DataMember(Name = "releases")]
        public Release[] releases { get; set; }

        [DataMember(Name = "sprints")]
        public Sprint[] sprints { get; set; }

        [DataMember(Name = "selectedproject")]
        public Project selectedproject { get; set; }

        [DataMember(Name = "selectedrelease")]
        public Release selectedrelease { get; set; }

        [DataMember(Name = "selectedsprint")]
        public Sprint selectedsprint { get; set; }

        [DataMember(Name = "selectedreporttype")]
        public ReportType selectedreporttype { get; set; }

        public RallyConfig()
        {
            credential = new Credential();
        }
    }
    [DataContract]
    public class Credential
    {
        [DataMember(Name = "userid")]
        public string userid { get; set; }
        [DataMember(Name = "password")]
        public string password { get; set; }
    }
    [DataContract]
    public class Project
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }
        [DataMember(Name = "ref")]
        public string Ref { get; set; }
    }
    [DataContract]
    public class Release
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }
        [DataMember(Name = "ref")]
        public string Ref { get; set; }
    }
    [DataContract]
    public class Sprint
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }
        [DataMember(Name = "ref")]
        public string Ref { get; set; }
    }
    public class ConfigAccess
    {
        
        const string configFile = "\\rally.conf";

        string _configfile;
        FileStream _fileStream = null;
        DataContractJsonSerializer _jsonSerializer = null;
        RallyConfig _rallyConfig = new RallyConfig();

        public Credential RallyCredential
        {
            get { return _rallyConfig.credential; }
            set { _rallyConfig.credential = value; }
        }
        public Project SelectedProject
        {
            get { return _rallyConfig.selectedproject; }
            set { _rallyConfig.selectedproject = value; }
        }
        public Release SelectedRelease
        {
            get { return _rallyConfig.selectedrelease; }
            set { _rallyConfig.selectedrelease = value; }
        }
        public Sprint SelectedSprint
        {
            get { return _rallyConfig.selectedsprint; }
            set { _rallyConfig.selectedsprint = value; }
        }
        public ReportType SelectedReportType
        {
            get { return _rallyConfig.selectedreporttype; }
            set { _rallyConfig.selectedreporttype = value; }
        }


        public List<Project> Projects
        {
            get { return _rallyConfig.projects.ToList(); }
            set { _rallyConfig.projects = value.ToArray(); }
        }
        public List<Release> Releases
        {
            get { return _rallyConfig.releases.ToList(); }
            set { _rallyConfig.releases = value.ToArray(); }
        }
        public List<Sprint> Sprints
        {
            get { return _rallyConfig.sprints.ToList(); }
            set { _rallyConfig.sprints = value.ToArray(); }
        }

        public ConfigAccess()
        {             
            _configfile = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + configFile;            
            _jsonSerializer = new DataContractJsonSerializer(typeof(RallyConfig));
        }
        public bool SavetoConfigData()
        {
            try {
                _fileStream = new FileStream(_configfile, FileMode.Open, FileAccess.Write);
                _fileStream.SetLength(0);
                _jsonSerializer.WriteObject(_fileStream, _rallyConfig);
                _fileStream.Close();
            }
            catch(SerializationException ex)
            {
                return false;
            }

            return true;
        }
        public bool LoadConfigData()
        {
            try {
                _fileStream = new FileStream(_configfile, FileMode.Open, FileAccess.Read);
                object objResponse = _jsonSerializer.ReadObject(_fileStream);

                RallyConfig jsonResponse = objResponse as RallyConfig;

                if (jsonResponse == null)
                    return false;
                //////Credential
                _rallyConfig.credential.userid = jsonResponse.credential.userid;
                _rallyConfig.credential.password = jsonResponse.credential.password;

                //////All Projects
                _rallyConfig.projects = jsonResponse.projects;

                //////All Releases
                _rallyConfig.releases = jsonResponse.releases;
                //////All Sprints
                _rallyConfig.sprints = jsonResponse.sprints;
                //////Selected Project
                _rallyConfig.selectedproject = jsonResponse.selectedproject;

                //////Selected Release
                _rallyConfig.selectedrelease = jsonResponse.selectedrelease;

                //////Selected Sprint
                _rallyConfig.selectedsprint = jsonResponse.selectedsprint;

                //////Selected Project
                _rallyConfig.selectedreporttype = jsonResponse.selectedreporttype;

                _fileStream.Close();
                return true;
            }
            catch(Exception ex)
            {
                string error = ex.ToString();
            }
            return false;
        }
    }
}
