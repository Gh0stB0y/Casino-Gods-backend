using CasinoGodsAPI.TablesModel;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using StackExchange.Redis;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace CasinoGodsAPI.Models
{
    public class ActivePlayers
    {
        public string Name { get; set; } = string.Empty;
        public string jwt { get; set; } = string.Empty;
        public ActivePlayers(string uname,string jwt, IConfiguration configuration)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtSecurityToken = tokenHandler.ReadJwtToken(jwt);
            var oldClaims = jwtSecurityToken.Claims;
            var userClaim = oldClaims.SingleOrDefault(c => c.Type == ClaimTypes.Role);
            if (userClaim != null) {
            List<Claim> claims = new List<Claim>();
            if (userClaim.Value == "Guest") {
                claims.Add(new Claim(ClaimTypes.Name, uname));
                claims.Add(new Claim(ClaimTypes.Role, "Guest"));
            }
            else  {
                claims.Add(new Claim(ClaimTypes.Name, uname));
                claims.Add(new Claim(ClaimTypes.Role, "Player"));
            }
                var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(configuration.GetSection("AppSettings:Token").Value));
                var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
                var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddMinutes(5),
                signingCredentials: credentials
                );
            var jwt_string = new JwtSecurityTokenHandler().WriteToken(token);
            this.jwt = jwt_string;
            Name = uname;
            }
        }

    }
    public class GamePlusPlayer
    {
        public int Id { get; set; }
        public GamesDatabase gameName { get; set; }
        public Player player { get; set; }
        public int gamesPlayed { get; set; } = 0;
        public int wins { get; set; } = 0;
        public int loses { get; set; } = 0;
        public int draws { get; set; } = 0;
        public float winratio { get; set; } = 0;
        public float profit { get; set; } = 0;

    }
    public class ActivePlayerDTO
    {
        public string username { get; set; } = string.Empty;
        public int bankroll { get; set; } = 0;
        public int profit { get; set; } = 0;
        public string jwt { get; set; } = string.Empty;
    }
    public class GamesDatabase
    {
        public string Name { get; set; }
        public ICollection<TablesDatabase> Tables { get; set; }
        public GamesDatabase()
        {
            Tables = new HashSet<TablesDatabase>();
        }
    }  //MODEL BAZY DANYCH DOSTEPNYCH GIER
    public class TablesDatabase
    {
        [Key, Column(Order = 0)]
        public string CKname { get; set; }
        [Key, Column(Order = 1)]
        public string CKGame { get; set; }
        public GamesDatabase Game { get; set; }
        public int minBet { get; set; } = 0;
        public int maxBet { get; set; } = 100000;
        public int betTime { get; set; } = 30;  
        public int maxseats { get; set; } = 5;
        public int actionTime { get; set; } = 15;
        public bool sidebet1 { get; set; } = true;
        public bool sidebet2 { get; set; } = true;
        public int decks { get; set; } = 6;

        public ICollection<LobbyTableData> ActiveTables { get; set; }
        public TablesDatabase()
        {
            ActiveTables = new HashSet<LobbyTableData>();
        }


    } //MODEL STOLOW DO GRY
    public class ActiveTablesDatabase
    {
        [Key]
        public Guid TableInstanceId { get; set; } = new Guid();
        public string TablePath { get; set; } = string.Empty;

        public string Name { get; set; }
        public string Game { get; set; }
        public int minBet { get; set; } = 0;
        public int maxBet { get; set; } = 100000;
        public int betTime { get; set; } = 30;
        public int maxseats { get; set; } = 5;
        public int actionTime { get; set; } = 15;
        public bool sidebet1 { get; set; } = true;
        public bool sidebet2 { get; set; } = true;
        public int decks { get; set; } = 6;
        
    }

}
