using System;

namespace ChoreApp.Models
{
    public class CompletedChore
    {
        public CompletedChore()
        {

        }
        public CompletedChore(int id, int choreId, int childId, DateTime date)
        {
            Id = id;
            ChoreId = choreId;
            ChildId = childId;
            Date = date;
        }
        public int Id { get; set; }
        public int ChoreId { get; set; }
        public int ChildId { get; set; }
        public DateTime? Date { get; set; }
    }
}