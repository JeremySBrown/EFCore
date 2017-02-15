using System.Collections.Generic;
using ChoreApp.Models;

namespace ChoreApp.Contracts
{
    public interface IChoreRepository
    {
        List<User> GetAllUsers();
        List<Chore> GetAllChores();
        List<AssignmentSummary> GetChildAssignmentsThisWeek(int childId);
        User GetUser(int id);
        Chore GetChore(int id);
        void AddUser(User value);
        void EditUser(int id, User value);
        void DeleteUser(int id);
        void AddChore(Chore value);
        void EditChore(int id, Chore value);
        void DeleteChore(int id);
        void CompleteChore(CompleteChorePayload data);
        void ClearChoreCompletion(CompleteChorePayload data);
    }
}