using Microsoft.AspNetCore.SignalR;

namespace CasinoGodsAPI.Hubs.TablesHubs
{
    public class RouletteTables:Hub
    {
        public static Dictionary<string, string> BacarratTablesAvailable = new Dictionary<string, string>();
    }
    public class RouletteTables1 : RouletteTables { public RouletteTables1() : base() { } }
    public class RouletteTables2 : RouletteTables { public RouletteTables2() : base() { } }
    public class RouletteTables3 : RouletteTables { public RouletteTables3() : base() { } }
}
