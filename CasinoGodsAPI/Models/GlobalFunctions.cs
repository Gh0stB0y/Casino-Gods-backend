using StackExchange.Redis;

namespace CasinoGodsAPI.Models
{
    public class GlobalFunctions
    {
        public static async Task<string> RefreshTokenGlobal(string jwt, IDatabase redisDbLogin, IDatabase redisDbJwt, IConfiguration _configuration)
        {
            var ActivePlayerCheck = await redisDbJwt.StringGetAsync(jwt);
            if (ActivePlayerCheck.IsNull) return "Session expired, log in again";
            else
            {
                string username = ActivePlayerCheck.ToString();
                var ActivePlayer = new ActivePlayers(username, jwt, _configuration);
                redisDbLogin.StringSetAsync(ActivePlayer.Name, ActivePlayer.jwt, new TimeSpan(0, 0, 5, 0), flags: CommandFlags.FireAndForget);
                redisDbJwt.StringSetAsync(ActivePlayer.jwt, ActivePlayer.Name, new TimeSpan(0, 0, 5, 0), flags: CommandFlags.FireAndForget);
                return ActivePlayer.jwt;
            }
        }
        public static async Task<string> RefreshTokenGlobal(string jwt,string UName, IDatabase redisDbLogin, IDatabase redisDbJwt, IConfiguration _configuration)
        {
            var ActivePlayerCheck = await redisDbJwt.StringGetAsync(jwt);
            if (ActivePlayerCheck.IsNull) return "Session expired, log in again";
            else
            {
                string username = ActivePlayerCheck.ToString();
                if (UName == username)
                {
                    var ActivePlayer = new ActivePlayers(username, jwt, _configuration);
                    redisDbLogin.StringSetAsync(ActivePlayer.Name, ActivePlayer.jwt, new TimeSpan(0, 0, 5, 0), flags: CommandFlags.FireAndForget);
                    redisDbJwt.StringSetAsync(ActivePlayer.jwt, ActivePlayer.Name, new TimeSpan(0, 0, 5, 0), flags: CommandFlags.FireAndForget);
                    return ActivePlayer.jwt;
                }
                else return "Redis data error, log in again";
            }
        }
    }
}
