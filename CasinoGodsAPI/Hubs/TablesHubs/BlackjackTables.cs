using Microsoft.AspNetCore.SignalR;

namespace CasinoGodsAPI.Hubs.TablesHubs
{
    public class BlackjackTables:Hub
    {
        public static Dictionary<string, string> BacarratTablesAvailable = new Dictionary<string, string>();
    }
    public class BlackjackTables1 : BlackjackTables { public BlackjackTables1() : base() { } }
    public class BlackjackTables2 : BlackjackTables { public BlackjackTables2() : base() { } }
    public class BlackjackTables3 : BlackjackTables { public BlackjackTables3() : base() { } }
}
