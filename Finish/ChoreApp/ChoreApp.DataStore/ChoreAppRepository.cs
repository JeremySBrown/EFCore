using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ChoreApp.Contracts;
using ChoreApp.Exceptions;
using ChoreApp.Models;
using Microsoft.EntityFrameworkCore;

namespace ChoreApp.DataStore
{
    public class ChoreAppRepository : IChoreRepository
    {
        private readonly ChoreAppDbContext _dbContext;

        public ChoreAppRepository(ChoreAppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public List<User> GetAllUsers()
        {
            return _dbContext.Users.OrderBy(p => p.Name).ToList();
        }

        public List<Chore> GetAllChores()
        {
            return _dbContext.Chores.OrderBy(p=>p.Description).ToList();
        }

        public List<AssignmentSummary> GetChildAssignmentsThisWeek(int childId)
        {
            var assignments = new List<AssignmentSummary>();
            var childChores = _dbContext.Chores.Where(x => x.ChildId == childId).ToList();
            var startOfWeek = GetDateTimeThisWeek(DayOfWeek.Sunday);
            var endOfWeek = GetEndOfDay(GetDateTimeThisWeek(DayOfWeek.Saturday));

            var completedThisWeekByChild = _dbContext.CompletedChores
                .Where(x => x.ChildId == childId &&
                            x.Date.HasValue &&
                            x.Date.Value >= startOfWeek &&
                            x.Date.Value <= endOfWeek).ToList();

            foreach (var chore in childChores)
            {
                var completedThisChoreThisWeekByChild = completedThisWeekByChild.Where(x => x.ChoreId == chore.Id).ToList();
                if (chore.OnSunday)
                {
                    assignments.Add(new AssignmentSummary(chore.Id, childId, chore.Description, DayOfWeek.Sunday, CompletedOnDay(completedThisChoreThisWeekByChild, DayOfWeek.Sunday)));
                }
                if (chore.OnMonday)
                {
                    assignments.Add(new AssignmentSummary(chore.Id, childId, chore.Description, DayOfWeek.Monday, CompletedOnDay(completedThisChoreThisWeekByChild, DayOfWeek.Monday)));
                }
                if (chore.OnTuesday)
                {
                    assignments.Add(new AssignmentSummary(chore.Id, childId, chore.Description, DayOfWeek.Tuesday, CompletedOnDay(completedThisChoreThisWeekByChild, DayOfWeek.Tuesday)));
                }
                if (chore.OnWednesday)
                {
                    assignments.Add(new AssignmentSummary(chore.Id, childId, chore.Description, DayOfWeek.Wednesday, CompletedOnDay(completedThisChoreThisWeekByChild, DayOfWeek.Wednesday)));
                }
                if (chore.OnThursday)
                {
                    assignments.Add(new AssignmentSummary(chore.Id, childId, chore.Description, DayOfWeek.Thursday, CompletedOnDay(completedThisChoreThisWeekByChild, DayOfWeek.Thursday)));
                }
                if (chore.OnFriday)
                {
                    assignments.Add(new AssignmentSummary(chore.Id, childId, chore.Description, DayOfWeek.Friday, CompletedOnDay(completedThisChoreThisWeekByChild, DayOfWeek.Friday)));
                }
                if (chore.OnSaturday)
                {
                    assignments.Add(new AssignmentSummary(chore.Id, childId, chore.Description, DayOfWeek.Saturday, CompletedOnDay(completedThisChoreThisWeekByChild, DayOfWeek.Saturday)));
                }
            }

            return assignments.OrderBy(x => x.Day).ToList();
        }

        public User GetUser(int id)
        {
            return _dbContext.Users.Include(p => p.Chores).SingleOrDefault(u => u.Id == id);
        }

        public Chore GetChore(int id)
        {
            return _dbContext.Chores.Include(p=>p.Child).SingleOrDefault(c => c.Id == id);
        }

        public void AddUser(User value)
        {
            if (string.IsNullOrWhiteSpace(value.Name))
            {
                throw new InvalidRequestException();
            }
            _dbContext.Users.Add(value);
            _dbContext.SaveChanges();
        }

        public void EditUser(int id, User value)
        {
            if (string.IsNullOrWhiteSpace(value.Name))
            {
                throw new InvalidDataException();
            }
            var user = _dbContext.Users.FirstOrDefault(u => u.Id == id);
            if (user != null)
            {
                user.Name = value.Name;
                _dbContext.SaveChanges();
            }

            throw new InvalidRequestException();
        }

        public void DeleteUser(int id)
        {
            var hasExistingChores = _dbContext.Chores.Any(x => x.ChildId == id);
            if (hasExistingChores)
            {
                throw new DataConflictException();
            }

            var user = _dbContext.Users.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                throw new DataMissingException();
            }
            
            var completedToRemove = _dbContext.CompletedChores.Where(x => x.ChildId == id).ToList();
            if (completedToRemove.Any())
            {
                _dbContext.CompletedChores.RemoveRange(completedToRemove);
            }

            _dbContext.Users.Remove(user);
            _dbContext.SaveChanges();
        }

        public void AddChore(Chore value)
        {
            if (string.IsNullOrWhiteSpace(value.Description))
            {
                throw new InvalidDataException();
            }
            if (!value.OnSunday &&
                !value.OnMonday &&
                !value.OnTuesday &&
                !value.OnWednesday &&
                !value.OnThursday &&
                !value.OnFriday &&
                !value.OnSaturday)
            {
                throw new InvalidDataException();
            }

            _dbContext.Chores.Add(value);
            _dbContext.SaveChanges();
        }

        public void EditChore(int id, Chore value)
        {
            if (string.IsNullOrWhiteSpace(value.Description))
            {
                throw new InvalidDataException();
            }
            if (!value.OnSunday &&
                !value.OnMonday &&
                !value.OnTuesday &&
                !value.OnWednesday &&
                !value.OnThursday &&
                !value.OnFriday &&
                !value.OnSaturday)
            {
                throw new InvalidDataException();
            }

            var chore = _dbContext.Chores.FirstOrDefault(c => c.Id == id);
            if (chore != null)
            {
                chore.Description = value.Description;
                chore.OnSunday = value.OnSunday;
                chore.OnMonday = value.OnMonday;
                chore.OnTuesday = value.OnTuesday;
                chore.OnWednesday = value.OnWednesday;
                chore.OnThursday = value.OnThursday;
                chore.OnFriday = value.OnFriday;
                chore.OnFriday = value.OnSaturday;

                _dbContext.SaveChanges();
            }
        }

        public void DeleteChore(int id)
        {
            var chore = _dbContext.Chores.FirstOrDefault(c => c.Id == id);
            if (chore == null)
            {
                throw new DataMissingException();
            }
            
            var completedChores = _dbContext.CompletedChores.Where(cc => cc.ChoreId == id).ToList();
            if (completedChores.Any())
            {
                _dbContext.CompletedChores.RemoveRange(completedChores);
            }

            _dbContext.Chores.Remove(chore);
            _dbContext.SaveChanges();
        }

        public void CompleteChore(CompleteChorePayload data)
        {
            var dateToComplete = GetDateTimeThisWeek(data.Day);
            var endOfDay = GetEndOfDay(dateToComplete);

            var alreadyCompleted =
                _dbContext.CompletedChores.Any(x => x.ChildId == data.ChildId &&
                x.ChoreId == data.ChoreId &&
                x.Date.HasValue &&
                x.Date.Value >= dateToComplete && x.Date.Value <= endOfDay);

            if (alreadyCompleted)
            {
                return;
            }

            var completedChore = new CompletedChore
            {
                ChoreId = data.ChoreId,
                ChildId = data.ChildId,
                Date = dateToComplete
            };

            _dbContext.CompletedChores.Add(completedChore);
            _dbContext.SaveChanges();
        }

        public void ClearChoreCompletion(CompleteChorePayload data)
        {
            var dateToComplete = GetDateTimeThisWeek(data.Day);
            var endOfDay = GetEndOfDay(dateToComplete);

            var completedChores =
                _dbContext.CompletedChores.Where(x => x.ChildId == data.ChildId &&
                x.ChoreId == data.ChoreId &&
                x.Date.HasValue &&
                x.Date.Value >= dateToComplete && x.Date.Value <= endOfDay).ToList();

            if (!completedChores.Any())
            {
                return;
            }

            _dbContext.CompletedChores.RemoveRange(completedChores);
            _dbContext.SaveChanges();
        }

        private static DateTime GetDateTimeThisWeek(DayOfWeek day)
        {
            var todayDateTime = DateTime.Now;
            var today = new DateTime(todayDateTime.Year, todayDateTime.Month, todayDateTime.Day);
            var diff = day - today.DayOfWeek;
            var desired = today.AddDays(diff);
            return desired;
        }

        private static DateTime GetEndOfDay(DateTime date)
        {
            return date.Date.AddDays(1).AddMilliseconds(-1);
        }

        private static bool IsDateMatch(DateTime left, DateTime right)
        {
            if (left.Year == right.Year && left.Month == right.Month && left.Day == right.Day)
            {
                return true;
            }
            return false;
        }

        private static bool CompletedOnDay(IEnumerable<CompletedChore> chores, DayOfWeek day)
        {
            return chores.Any(x => x.Date.HasValue && IsDateMatch(x.Date.Value, GetDateTimeThisWeek(day)));
        }
    }
}
