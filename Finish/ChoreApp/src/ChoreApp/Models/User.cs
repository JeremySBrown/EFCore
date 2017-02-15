using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ChoreApp.Models
{
    public class User
    {
        public User()
        {
            Chores = new List<Chore>();
        }
        public User(int id, string name):this()
        {
            Id = id;
            Name = name;
        }
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        public IList<Chore> Chores { get; set; }
    }
}