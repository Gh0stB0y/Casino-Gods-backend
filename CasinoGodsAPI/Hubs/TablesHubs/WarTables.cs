using Microsoft.AspNetCore.SignalR;

namespace CasinoGodsAPI.Hubs.TablesHubs
{
    public class WarTables:Hub
    {
        public static Dictionary<string, string> BacarratTablesAvailable = new Dictionary<string, string>();
    }
    public class WarTables1 : WarTables { public WarTables1() : base() { } }
    public class WarTables2 : WarTables { public WarTables2() : base() { } }
    public class WarTables3 : WarTables { public WarTables3() : base() { } }
}
