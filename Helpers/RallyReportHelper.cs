using Rally.RestApi;
using Rally.RestApi.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvanRallyReport.Model;
using AvanRallyReport.DataAccess;
using System.IO;
using System.Security.Cryptography;

namespace AvanRallyReport.Helpers
{
    public class RallyReportHelper
    {
        public static RallyRestApi RestApi { get; set; }
        public static string WorksspaceRef { get; set; }
        public static string ProjectRef { get; set; }
        public static bool Login(string user, string password)
        {
            RestApi = new RallyRestApi();
            RestApi.Authenticate(user, password, "https://rally1.rallydev.com", proxy: null, allowSSO: false);

            if (RestApi != null)
            {
                return true;
            }
            return false;
        }
        public static void LogOut()
        {
            RestApi = null;
        }
        public static List<Project> GetProjects()
        {
            List<Project> projects = new List<Project>();
            Request projectRequest = new Request("Projects");
            projectRequest.Query = new Query("");
            QueryResult projectResults = RestApi.Query(projectRequest);

            foreach (var project in projectResults.Results)
            {
                Project proj = new Project();
                proj.Name = project._refObjectName;
                proj.Ref = project._ref;
                projects.Add(proj);
            }
            return projects;
        }
        public static List<Release> GetReleases(Project project)
        {
            List<Release> releases = new List<Release>();
            Request releaseRequest = new Request("Releases");
            releaseRequest.Project = project.Ref;
            releaseRequest.Query = new Query("");
            QueryResult releaseResults = RestApi.Query(releaseRequest);

            foreach (var release in releaseResults.Results)
            {
                Release rel = new Release();
                rel.Name = release._refObjectName;
                rel.Ref = release._ref;
                releases.Add(rel);
            }
            return releases;
        }
        public static List<Sprint> GetSprints(Project selectedproject)
        {
            List<Sprint> sprints = new List<Sprint>();
            Request sprintRequest = new Request("Iterations");
            sprintRequest.Project = selectedproject.Ref;
            sprintRequest.Query = new Query("");
            sprintRequest.Limit = 500;
            QueryResult sprintResults = RestApi.Query(sprintRequest);

            foreach (var sprint in sprintResults.Results)
            {
                Sprint spr = new Sprint();
                spr.Name = sprint._refObjectName;
                spr.Ref = sprint._ref;
                sprints.Add(spr);
            }
            return sprints;
        }
        public static List<Story> GetStories(string requestInit, Project selectedproject, Release release, Sprint sprint, bool tasksincluded)
        {
            List<Story> stories = new List<Story>();
            Request storyRequest = new Request(requestInit);
            storyRequest.Project = selectedproject.Ref;
            storyRequest.Workspace = RallyReportHelper.GetWorkspace();
            storyRequest.ProjectScopeUp = false;
            storyRequest.ProjectScopeDown = true;
            storyRequest.Fetch = new List<string>()
                {
                    "Name",
                    "FormattedID",
                    "Tasks",
                    "Owner",
                    "Release",
                    "Iteration",
                    "PlanEstimate",
                    "TaskEstimateTotal",
                    "TaskRemainingTotal",
                    "TaskActualTotal",
                    "ScheduleState",
                    "InProgressDate",
                    "Blocked",
                    "RevisionHistory"
                };
            storyRequest.Query = new Query("Release.Name", Query.Operator.Equals, release.Name);
            storyRequest.Query = new Query("Iteration.Name", Query.Operator.Equals, sprint.Name);
            //string query = String.Format("(Release.Name = "{0}") AND (Iteration.Name = "{1}")", release.Name, sprint.Name);
            //storyRequest.Query = new Query(query);

            QueryResult storyResults = RestApi.Query(storyRequest);

            foreach (var story in storyResults.Results)
            {
                Story stry = new Story();
                stry.Name = story["Name"];
                stry.FormattedID = story["FormattedID"];

                var owner = story["Owner"];
                stry.Owner = owner != null ? owner._refObjectName : "";

                var rel = story["Release"];
                stry.ReleaseName = rel != null ? rel._refObjectName : "";

                var iteration = story["Iteration"];
                stry.IterationName = iteration != null ? iteration._refObjectName : "";

                var planestimate = story["PlanEstimate"];
                stry.PlanEstimate = planestimate != null ? (int)planestimate : 0;

                var taskestimatetotal = story["TaskEstimateTotal"];
                stry.TaskEstimateTotal = taskestimatetotal != null ? (int)taskestimatetotal : 0;

                var taskremainingtotal = story["TaskRemainingTotal"];
                stry.TaskRemainingTotal = taskremainingtotal != null ? (int)taskremainingtotal : 0;
                var taskactualtotal = story["TaskActualTotal"];
                stry.TaskActualTotal = taskactualtotal != null ? (int)taskactualtotal : 0;

                stry.ScheduleState = story["ScheduleState"];
                stry.InProgressDate = Convert.ToDateTime(story["InProgressDate"]);
                stry.DaysSinceInProgress = stry.ScheduleState.CompareTo("In-Progress")==0?
                                        DaysLeft(stry.InProgressDate,DateTime.Now, true,new List<DateTime>()):0;
                stry.Blocked = story["Blocked"];

	//code taken from https://csharp.hotexamples.com/examples/Rally.RestApi/RallyRestApi/Query/php-rallyrestapi-query-method-examples.html
                String historyRef = story["RevisionHistory"]._ref;
                Request revisionsRequest = new Request("Revisions");
                revisionsRequest.Query = new Query("RevisionHistory", Query.Operator.Equals, historyRef);
                revisionsRequest.Fetch = new List<string>() {"Description"};
                QueryResult revisionsResults = RestApi.Query(revisionsRequest);
                stry.SpilloverCount = 0;
                foreach (var r in revisionsResults.Results)
                {
                    
                    string str = r["Description"];
                    
                    if (str.Contains("ITERATION changed from"))
                    {
                        stry.SpilloverCount++;
                        if (str.Contains(stry.IterationName))
                        {
                            stry.Spillover = true;                            
                        }
                    }
                }

                stories.Add(stry);

                //Tasks to be added ?
                if (tasksincluded)
                {
                    Request taskRequest = new Request(story["Tasks"]);
                    QueryResult queryTaskResult = RestApi.Query(taskRequest);
                    if (queryTaskResult.TotalResultCount > 0)
                    {
                        foreach (var t in queryTaskResult.Results)
                        {
                            Story task = new Story();

                            var taskID = t["FormattedID"];
                            task.FormattedID = taskID;

                            var taskName = t["Name"];
                            task.Name = taskName;

                            var taskowner = t["Owner"];
                            task.Owner = taskowner != null ? taskowner._refObjectName : "";

                            var taskrel = t["Release"];
                            task.ReleaseName = taskrel != null ? taskrel._refObjectName : "";

                            var taskiteration = t["Iteration"];
                            task.IterationName = taskiteration != null ? taskiteration._refObjectName : "";


                            var taskplanestimate = t["Estimate"];
                            task.PlanEstimate = taskplanestimate != null ? (int)taskplanestimate : 0;

                            var taskestimate = t["Estimate"];
                            task.TaskEstimateTotal = taskestimate != null ? (int)taskestimate : 0;

                            var taskremaining = t["ToDo"];
                            task.TaskRemainingTotal = taskremaining != null ? (int)taskremaining : 0;
                            var taskactual = t["Actuals"];
                            task.TaskActualTotal = taskactual != null ? (int)taskactual : 0;

                            task.ScheduleState = t["State"];

                            //var revhis = t["RevisionHistory"];
                            //string rev = revhis._ref;
                            //var tastTimeSpent = t["TimeSpent"];
                            //task.InProgressDate = Convert.ToDateTime(t["InProgressDate"]);

                            task.DaysSinceInProgress = task.ScheduleState.CompareTo("In-Progress") == 0 ?
                                        DaysLeft(task.InProgressDate, DateTime.Now, true, new List<DateTime>()) : 0;

                            task.Blocked = t["Blocked"];

                            stories.Add(task);
                        }
                    }
                }
            }

            return stories;
        }
        public static string GetWorkspace()
        {
            Request workspactRequest = new Request("Workspace");
            workspactRequest.Query = new Query("");
            QueryResult workspaceResults = RestApi.Query(workspactRequest);

            var workspace = workspaceResults.Results.FirstOrDefault();

            WorksspaceRef = workspace._ref;

            return WorksspaceRef;
        }

        public static string GetProject(string proj)
        {
            ProjectRef = "https://rally1.rallydev.com/slm/webservice/v2.0" + proj;

            return ProjectRef;
        }
        public static int DaysLeft(DateTime startDate, DateTime endDate, bool excludeWeekends, List<DateTime> excludeDates)
        {
            int count = 0;
            for (DateTime index = startDate; index < endDate; index = index.AddDays(1))
            {
                if (excludeWeekends && index.DayOfWeek != DayOfWeek.Sunday && index.DayOfWeek != DayOfWeek.Saturday)
                {
                    bool excluded = false; ;
                    for (int i = 0; i < excludeDates.Count; i++)
                    {
                        if (index.Date.CompareTo(excludeDates[i].Date) == 0)
                        {
                            excluded = true;
                            break;
                        }
                    }

                    if (!excluded)
                    {
                        count++;
                    }
                }
            }

            return count;
        }
        public static string EncryptString(string ClearText)
        {

            byte[] clearTextBytes = Encoding.UTF8.GetBytes(ClearText);

            System.Security.Cryptography.SymmetricAlgorithm rijn = SymmetricAlgorithm.Create();

            MemoryStream ms = new MemoryStream();
            byte[] rgbIV = Encoding.ASCII.GetBytes("ryojvlzmdalyglrj");
            byte[] key = Encoding.ASCII.GetBytes("hcxilkqbbhczfeultgbskdmaunivmfuo");
            CryptoStream cs = new CryptoStream(ms, rijn.CreateEncryptor(key, rgbIV),
       CryptoStreamMode.Write);

            cs.Write(clearTextBytes, 0, clearTextBytes.Length);

            cs.Close();

            return Convert.ToBase64String(ms.ToArray());
        }

        public static string DecryptString(string EncryptedText)
        {
            byte[] encryptedTextBytes = Convert.FromBase64String(EncryptedText);

            MemoryStream ms = new MemoryStream();

            System.Security.Cryptography.SymmetricAlgorithm rijn = SymmetricAlgorithm.Create();


            byte[] rgbIV = Encoding.ASCII.GetBytes("ryojvlzmdalyglrj");
            byte[] key = Encoding.ASCII.GetBytes("hcxilkqbbhczfeultgbskdmaunivmfuo");

            CryptoStream cs = new CryptoStream(ms, rijn.CreateDecryptor(key, rgbIV),
            CryptoStreamMode.Write);

            cs.Write(encryptedTextBytes, 0, encryptedTextBytes.Length);

            cs.Close();

            return Encoding.UTF8.GetString(ms.ToArray());

        }
    }
}
