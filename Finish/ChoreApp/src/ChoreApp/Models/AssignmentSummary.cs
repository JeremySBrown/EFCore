using System;

namespace ChoreApp.Models
{
    public class AssignmentSummary
    {
        public AssignmentSummary()
        {

        }
        public AssignmentSummary(int choreId, int childId, string description, DayOfWeek day, bool completed)
        {
            ChoreId = choreId;
            ChildId = childId;
            Description = description;
            Day = day;
            Completed = completed;
        }
        public AssignmentSummary Clone()
        {
            return (AssignmentSummary)this.MemberwiseClone();
        }
        public int ChoreId { get; set; }
        public int ChildId { get; set; }
        public string Description { get; set; }
        public string DayFormatted
        {
            get
            {
                return Day.ToString();
            }
        }
        public DayOfWeek Day { get; set; }
        public bool Completed { get; set; }
        public bool Overdue
        {
            get
            {
                var todayDay = DateTime.Now.DayOfWeek;
                //var todayDay = DayOfWeek.Friday;
                return !Completed && todayDay > Day;
            }
        }
    }
}