using System.Collections.Generic;
using System.Linq;
using ChoreApp.Models;

namespace ChoreApp.DataStore
{
    public class ChoreAppInitializer
    {
        private readonly ChoreAppDbContext _dbContext;

        public ChoreAppInitializer(ChoreAppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void Seed()
        {
            if (_dbContext.Users.Any())
                return;

            var children = new List<User>{
                new User
                {
                    Name = "John",
                    Chores = new List<Chore>{
                        new Chore
                        {
                            Description = "Do Dishes",
                            OnMonday = true,
                            OnWednesday = true,
                            OnFriday = true,
                            OnSaturday = true
                        },
                        new Chore
                        {
                            Description = "Take Out Trash",
                            OnWednesday = true
                        },
                        new Chore
                        {
                            Description = "Clean Room",
                            OnSaturday = true
                        },
                    }
                },
                new User
                {
                    Name = "Mary",
                    Chores = new List<Chore>(new[]
                    {
                        new Chore
                        {
                            Description = "Do Dishes",
                            OnSunday = true,
                            OnTuesday = true,
                            OnThursday = true
                        },
                        new Chore
                        {
                            Description = "Clean Room",
                            OnSaturday = true
                        },
                    })
                },
            };

            _dbContext.Users.AddRange(children);
            _dbContext.SaveChanges();
        }
    }
}
