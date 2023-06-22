using CasinoGodsAPI.TablesModel;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations;
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
    public class GamesDatabase
    {
        [Key]
        public string Name { get; set; }
        //public virtual ICollection<TablesDatabase> Tables { get; set; }
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

}
