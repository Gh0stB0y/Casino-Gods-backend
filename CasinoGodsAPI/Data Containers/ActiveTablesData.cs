using CasinoGodsAPI.Models.DatabaseModels;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using CasinoGodsAPI.Services;
using CasinoGodsAPI.Models;

namespace CasinoGodsAPI.Data_Containers
{
    public static class ActiveTablesData
    {
        public static ConcurrentDictionary<string, PlayingTable> ManagedTablesObj = new();
        public static ConcurrentDictionary<string, int> UserCountAtTablesDictionary = new();
        public static ConcurrentDictionary<string, string> UserGroupDictionary = new();
        public static ConcurrentDictionary<string, HubCallerContext> UserContextDictionary = new();
        public static List<ActiveTablesDB> ActiveTables = new();

        public static void AddNewTableConditions(string TableId)
        {
            var ParentTable = ActiveTables.SingleOrDefault(t => t.TableInstanceId.ToString() == TableId);
            if (ParentTable != null)
            {
                var ChildTable = new ActiveTablesDB();
                ChildTable.AddProperties(ParentTable, Guid.NewGuid());

                UserCountAtTablesDictionary.TryAdd(ChildTable.TableInstanceId.ToString(), 0);
                ActiveTables.Add(ChildTable);
                LobbyService.AddTable(ChildTable.TableInstanceId.ToString(), ChildTable.Game);
            }
            else Console.WriteLine("Parent Table not found");
        }
        public static void DeleteTableConditions(string TableId)
        {
            var Table = ActiveTables.SingleOrDefault(i => i.TableInstanceId.ToString() == TableId);
            if (Table != null)
            {
                bool AllTablesFull = true;
                var SameTypeInstances = ActiveTables.Where(t => t.Game == Table.Game && t.Name == Table.Name).ToList();
                int count = 0;
                foreach (var Instance in SameTypeInstances)
                {
                    if (UserCountAtTablesDictionary[Instance.TableInstanceId.ToString()] < Instance.Maxseats) count++;
                }
                if (count > 1) AllTablesFull = false;
                if (SameTypeInstances.Count > 1 && AllTablesFull == false)
                {                   
                    ActiveTables.Remove(Table);
                    LobbyService.DeleteTable(TableId);
                    var UsersOfDeletedTable = UserGroupDictionary.Where(u => u.Value == TableId).ToList();

                    if (UsersOfDeletedTable.Count > 0)
                    {
                        foreach (var User in UsersOfDeletedTable)
                        {
                            UserGroupDictionary.TryRemove(User.Key, out _);
                        }
                    }
                }
                else
                {
                    LobbyService.MakeTableInactive(TableId);
                }
            }
        }

    }
}
