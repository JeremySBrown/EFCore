using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using ChoreApp.Contracts;
using ChoreApp.Exceptions;
using ChoreApp.Models;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SimpleAspNetCore
{
    public class ChoreRepositoryStub : IChoreRepository
    {
        private static int LOCK_TIMEOUT = 2 * 1000; //2 seconds
        private Dictionary<int, User> Users { get; set; }
        private Dictionary<int, Chore> Chores { get; set; }
        private Dictionary<int, CompletedChore> CompletedChores { get; set; }
        private int MaxUserId;
        private int MaxChoreId;
        private int MaxCompletedChoreId;
        private int WriteCount;
        private ReaderWriterLockSlim gl = new ReaderWriterLockSlim();
        private IHostingEnvironment HostingEnv;
        
        public ChoreRepositoryStub()
        {
            HostingEnv = null;
        }

        public ChoreRepositoryStub(IHostingEnvironment env)
        {
            HostingEnv = env;
            WriteCount = 0;
            Users = new Dictionary<int, User>();
            Chores = new Dictionary<int, Chore>();
            CompletedChores = new Dictionary<int, CompletedChore>();
            Initialize();
        }
        private static bool IsDateMatch(DateTime left, DateTime right)
        {
            if (left.Year == right.Year && left.Month == right.Month && left.Day == right.Day)
            {
                return true;
            }
            return false;
        }

        private static DateTime GetDateTimeThisWeek(DayOfWeek day)
        {
            var todayDateTime = DateTime.Now;
            var today = new DateTime(todayDateTime.Year, todayDateTime.Month, todayDateTime.Day);
            var diff = day - today.DayOfWeek;
            var desired = today.AddDays(diff);
            return desired;
        }

        private static bool CompletedOnDay(IEnumerable<CompletedChore> chores, DayOfWeek day)
        {
            return chores.Any(x => IsDateMatch(x.Date.Value, GetDateTimeThisWeek(day)));
        }

        public List<User> GetAllUsers()
        {
            if (!gl.TryEnterReadLock(LOCK_TIMEOUT))
            {
                throw new TimeoutException();
            }
            try
            {
                return Users.Values.OrderBy(x => x.Id).ToList();
            }
            finally
            {
                gl.ExitReadLock();
            }
        }

        public List<Chore> GetAllChores()
        {
            if (!gl.TryEnterReadLock(LOCK_TIMEOUT))
            {
                throw new TimeoutException();
            }
            try
            {
                return Chores.Values.Select(x => x.SetUser(Users[x.ChildId])).OrderBy(x => x.ChildId).ToList();
            }
            finally
            {
                gl.ExitReadLock();
            }
        }

        public List<AssignmentSummary> GetChildAssignmentsThisWeek(int childId)
        {
            if (!gl.TryEnterReadLock(LOCK_TIMEOUT))
            {
                throw new TimeoutException();
            }
            try
            {
                var assignments = new List<AssignmentSummary>();
                var childChores = Chores.Values.Where(x => x.ChildId == childId);
                var startOfWeek = GetDateTimeThisWeek(DayOfWeek.Sunday);
                var endOfWeek = GetDateTimeThisWeek(DayOfWeek.Saturday);
                var completedThisWeek = new Func<CompletedChore, bool>(completedChore =>
                {
                    if (!completedChore.Date.HasValue)
                    {
                        return false;
                    };
                    var date = completedChore.Date.Value;
                    if (date >= startOfWeek && date <= endOfWeek)
                    {
                        return true;
                    }
                    return false;
                });
                var completedByChild = CompletedChores.Values.Where(x => x.ChildId == childId).ToList();
                var completedThisWeekByChild = completedByChild.Where(completedThisWeek).ToList();
                foreach (var chore in childChores)
                {
                    var completedThisChoreThisWeekByChild = completedThisWeekByChild.Where(x => x.ChoreId == chore.Id);
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
            finally
            {
                gl.ExitReadLock();
            }
        }

        public User GetUser(int id)
        {
            if (!gl.TryEnterReadLock(LOCK_TIMEOUT))
            {
                throw new TimeoutException();
            }
            try
            {
                return Users[id];
            }
            finally
            {
                gl.ExitReadLock();
            }
        }

        public Chore GetChore(int id)
        {
            if (!gl.TryEnterReadLock(LOCK_TIMEOUT))
            {
                throw new TimeoutException();
            }
            try
            {
                return Chores[id];
            }
            finally
            {
                gl.ExitReadLock();
            }
        }

        public void AddUser(User value)
        {
            if (string.IsNullOrWhiteSpace(value.Name))
            {
                throw new InvalidRequestException();
            }
            if (!gl.TryEnterWriteLock(LOCK_TIMEOUT))
            {
                throw new TimeoutException();
            }
            try
            {
                var userId = ++MaxUserId;
                var user = new User(userId, value.Name);
                Users.Add(userId, user);
                PersistToDisk();
            }
            finally
            {
                gl.ExitWriteLock();
            }
        }

        public void EditUser(int id, User value)
        {
            if (string.IsNullOrWhiteSpace(value.Name))
            {
                throw new InvalidDataException();
            }
            if (!gl.TryEnterUpgradeableReadLock(LOCK_TIMEOUT))
            {
                throw new TimeoutException();
            }
            try
            {
                var user = Users[id];
                if (user == null)
                {
                    return;
                }
                var editUser = new User(id, value.Name);
                gl.TryEnterWriteLock(LOCK_TIMEOUT);
                try
                {
                    Users[id] = editUser;
                    PersistToDisk();
                }
                finally
                {
                    gl.ExitWriteLock();
                }
            }
            finally
            {
                gl.ExitUpgradeableReadLock();
            }
        }

        public void DeleteUser(int id)
        {
            if (!gl.TryEnterUpgradeableReadLock(LOCK_TIMEOUT))
            {
                throw new TimeoutException();
            }
            try
            {
                
                var hasExistingChores = Chores.Values.Any(x => x.ChildId == id);
                if (hasExistingChores)
                {
                    throw new DataConflictException();
                }
                
                var user = Users[id];
                if (user == null)
                {
                    throw new DataMissingException();
                }
                var completedToRemove = CompletedChores.Values.Where(x => x.ChildId == id).ToList();
                gl.TryEnterWriteLock(LOCK_TIMEOUT);
                try
                {
                    Users.Remove(id);
                    
                    foreach (var chore in completedToRemove)
                    {
                        CompletedChores.Remove(chore.Id);
                    }
                    PersistToDisk();
                }
                finally
                {
                    gl.ExitWriteLock();
                }
            }
            finally
            {
                gl.ExitUpgradeableReadLock();
            }
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
            if (!gl.TryEnterWriteLock(LOCK_TIMEOUT))
            {
                throw new TimeoutException();
            }
            try
            {
                var choreId = ++MaxChoreId;
                var chore = new Chore(choreId,
                    value.ChildId,
                    value.Description,
                    value.OnSunday,
                    value.OnMonday,
                    value.OnTuesday,
                    value.OnWednesday,
                    value.OnThursday,
                    value.OnFriday,
                    value.OnSaturday);
                Chores.Add(choreId, chore);
                PersistToDisk();
            }
            finally
            {
                gl.ExitWriteLock();
            }
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
            if (!gl.TryEnterUpgradeableReadLock(LOCK_TIMEOUT))
            {
                throw new TimeoutException();
            }
            try
            {
                var chore = Chores[id];
                if (chore == null)
                {
                    return;
                }
                var editChore = new Chore(id,
                    chore.ChildId, //can't change child
                    value.Description,
                    value.OnSunday,
                    value.OnMonday,
                    value.OnTuesday,
                    value.OnWednesday,
                    value.OnThursday,
                    value.OnFriday,
                    value.OnSaturday);
                gl.TryEnterWriteLock(LOCK_TIMEOUT);
                try
                {
                    Chores[id] = editChore;
                    PersistToDisk();
                }
                finally
                {
                    gl.ExitWriteLock();
                }
            }
            finally
            {
                gl.ExitUpgradeableReadLock();
            }
        }

        public void DeleteChore(int id)
        {
            if (!gl.TryEnterUpgradeableReadLock(LOCK_TIMEOUT))
            {
                throw new TimeoutException();
            }
            try
            {
                var chore = Chores[id];
                if (chore == null)
                {
                    throw new DataMissingException();
                }
                gl.TryEnterWriteLock(LOCK_TIMEOUT);
                try
                {
                    Chores.Remove(id);
                    foreach (var completedChores in CompletedChores.Values.Where(x => x.ChoreId == id).ToList())
                    {
                        CompletedChores.Remove(chore.Id);
                    }
                    PersistToDisk();
                }
                finally
                {
                    gl.ExitWriteLock();
                }
            }
            finally
            {
                gl.ExitUpgradeableReadLock();
            }
        }

        public void CompleteChore(CompleteChorePayload data)
        {
            if (!gl.TryEnterUpgradeableReadLock(LOCK_TIMEOUT))
            {
                throw new TimeoutException();
            }
            try
            {
                var dateToComplete = GetDateTimeThisWeek(data.Day);
                var completedChores = CompletedChores.Values;
                var alreadyCompleted = completedChores.Any(x => x.ChildId == data.ChildId && x.ChoreId == data.ChoreId && x.Date.HasValue && IsDateMatch(x.Date.Value, dateToComplete));
                if (alreadyCompleted)
                {
                    return;
                }
                gl.TryEnterWriteLock(LOCK_TIMEOUT);
                try
                {
                    var completedChoreId = ++MaxCompletedChoreId;
                    CompletedChores.Add(completedChoreId, new CompletedChore(completedChoreId, data.ChoreId, data.ChildId, GetDateTimeThisWeek(data.Day)));
                    PersistToDisk();
                }
                finally
                {
                    gl.ExitWriteLock();
                }
            }
            finally
            {
                gl.ExitUpgradeableReadLock();
            }
        }

        public void ClearChoreCompletion(CompleteChorePayload data)
        {
            if (!gl.TryEnterUpgradeableReadLock(LOCK_TIMEOUT))
            {
                throw new TimeoutException();
            }
            try
            {
                var dateToRemove = GetDateTimeThisWeek(data.Day);
                var completedChores = CompletedChores.Values;
                var completedRecords = completedChores.Where(x => x.ChildId == data.ChildId && x.ChoreId == data.ChoreId && x.Date.HasValue && IsDateMatch(x.Date.Value, dateToRemove)).ToList();
                if (!completedRecords.Any())
                {
                    return;
                }
                gl.TryEnterWriteLock(LOCK_TIMEOUT);
                try
                {
                    foreach (var completed in completedRecords)
                    {
                        CompletedChores.Remove(completed.Id);
                    }
                    PersistToDisk();
                }
                finally
                {
                    gl.ExitWriteLock();
                }
            }
            finally
            {
                gl.ExitUpgradeableReadLock();
            }
        }

        private void PersistToDisk()
        {
            try
            {
                if (HostingEnv == null) {
                    return;
                }
                var writeId = ++WriteCount;
                var obj = new
                {
                    MaxUserId = MaxUserId,
                    MaxChoreId = MaxChoreId,
                    MaxCompletedChoreId = MaxCompletedChoreId,
                    WriteId = writeId,
                    Users = Users.Values,
                    Chores = Chores.Values,
                    CompletedChores = CompletedChores.Values
                };
                var str = JsonConvert.SerializeObject(obj, Formatting.Indented);

                var appDataPath = HostingEnv.ContentRootFileProvider.GetFileInfo("/App_Data").PhysicalPath;
                var fileName = Guid.NewGuid().ToString() + ".json";
                var jsonFile = Path.Combine(appDataPath, fileName);
                using (var swriter = File.CreateText(jsonFile))
                {
                    swriter.Write(str);
                }
            }
            catch (Exception) { } //swallow exceptions - if write fails that is perfectly fine
        }

        private void Initialize()
        {
            Users.Clear();
            Chores.Clear();
            CompletedChores.Clear();
            MaxUserId = 0;
            MaxChoreId = 0;
            MaxCompletedChoreId = 0;
            try
            {
                if (InitializeFromDisk())
                {
                    return;
                }
            }
            catch (Exception ex) //swallow exceptions - if init from disk fails that is perfectly fine
            {
                Debug.Write(ex.ToString());
            }

            var userId = ++MaxUserId;
            var user = new User(userId, "John");
            Users.Add(userId, user);
            var choreId = ++MaxChoreId;
            var chore = new Chore(choreId, userId, "Do Dishes", onMonday: true, onWednesday: true, onFriday: true, onSaturday: true);
            Chores.Add(choreId, chore);
            var completedChoreId = ++MaxCompletedChoreId;
            CompletedChores.Add(completedChoreId, new CompletedChore(completedChoreId, choreId, userId, GetDateTimeThisWeek(DayOfWeek.Monday)));
            completedChoreId = ++MaxCompletedChoreId;
            CompletedChores.Add(completedChoreId, new CompletedChore(completedChoreId, choreId, userId, GetDateTimeThisWeek(DayOfWeek.Saturday)));
            choreId = ++MaxChoreId;
            chore = new Chore(choreId, userId, "Take Out Trash", onWednesday: true);
            Chores.Add(choreId, chore);
            choreId = ++MaxChoreId;
            chore = new Chore(choreId, userId, "Clean Room", onSunday: true);
            Chores.Add(choreId, chore);
            completedChoreId = ++MaxCompletedChoreId;
            CompletedChores.Add(completedChoreId, new CompletedChore(completedChoreId, choreId, userId, GetDateTimeThisWeek(DayOfWeek.Sunday)));

            userId = ++MaxUserId;
            user = new User(userId, "Mary");
            Users.Add(userId, user);
            choreId = ++MaxChoreId;
            chore = new Chore(choreId, userId, "Do Dishes", onTuesday: true, onThursday: true, onSunday: true);
            Chores.Add(choreId, chore);
            choreId = ++MaxChoreId;
            chore = new Chore(choreId, userId, "Clean Room", onSaturday: true);
            Chores.Add(choreId, chore);
        }

        private bool InitializeFromDisk()
        {
            if (HostingEnv == null) {
                return false;
            }
            var appDataPath = HostingEnv.ContentRootFileProvider.GetFileInfo("/App_Data").PhysicalPath;
            var appDataDir = new DirectoryInfo(appDataPath);
            var myFile = appDataDir.GetFiles().OrderByDescending(x => x.LastWriteTime).FirstOrDefault();
            if (myFile == null)
            {
                return false;
            }
            var textContent = File.ReadAllText(myFile.FullName);
            var jroot = JObject.Parse(textContent);
            var writeId = (int)jroot["WriteId"];
            var maxUserId = (int)jroot["MaxUserId"];
            var maxChoreId = (int)jroot["MaxChoreId"];
            var maxCompletedChoreId = (int)jroot["MaxCompletedChoreId"];
            var users = jroot["Users"].Select(x => (User)x.ToObject(typeof(User))).ToList();
            var chores = jroot["Chores"].Select(x => (Chore)x.ToObject(typeof(Chore))).ToList();
            var completedChores = jroot["CompletedChores"].Select(x => (CompletedChore)x.ToObject(typeof(CompletedChore))).ToList();

            MaxUserId = maxUserId;
            MaxChoreId = maxChoreId;
            MaxCompletedChoreId = maxCompletedChoreId;
            Users.Clear();
            foreach (var x in users)
            {
                Users.Add(x.Id, x);
            }
            Chores.Clear();
            foreach (var x in chores)
            {
                Chores.Add(x.Id, x);
            }
            CompletedChores.Clear();
            foreach (var x in completedChores)
            {
                CompletedChores.Add(x.Id, x);
            }
            return true;
        }

    }
}