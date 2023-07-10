using Microsoft.AspNetCore.SignalR;

namespace CasinoGodsAPI.Hubs.TablesHubs
{
    public class DragonTigerTables:Hub
    {
        public static Dictionary<string, string> BacarratTablesAvailable = new Dictionary<string, string>();
    }
    public class DragonTigerTables1 : DragonTigerTables { public DragonTigerTables1() : base() { } }
    public class DragonTigerTables2 : DragonTigerTables { public DragonTigerTables2() : base() { } }
    public class DragonTigerTables3 : DragonTigerTables { public DragonTigerTables3() : base() { } }
}
