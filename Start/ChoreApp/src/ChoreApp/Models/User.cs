using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ChoreApp.Models
{
    public class User
    {
        public User()
        {
        }
        public User(int id, string name):this()
        {
            Id = id;
            Name = name;
        }
        public int Id { get; set; }

        public string Name { get; set; }
    }
}