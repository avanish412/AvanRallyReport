using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvanRallyReport.Model
{
    public enum ReportType
    {
        DAILY_ASSIGNMENT,
        WORK_LOG,
        SPRINT_TASKS,
        PERFORMANCE
    }
    public class Story
    {
        public string FormattedID { get; set; }
        public bool Spillover { get; set; }
        public int SpilloverCount { get; set; }
        public string Name{ get; set; }
        public string Owner{ get; set; }
        public string ReleaseName{ get; set; }
        public string IterationName{ get; set; }
        public int PlanEstimate{ get; set; }
        public int TaskEstimateTotal{ get; set; }
        public int TaskRemainingTotal{ get; set; }
        public int TaskActualTotal{ get; set; }
        public string ScheduleState{ get; set; }
        public DateTime InProgressDate{ get; set; }

        public int DaysSinceInProgress { get; set; }
        public bool Blocked{ get; set; }
    }
}
