using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace CasinoGodsAPI.Models
{
    public class RefreshToken
    {
        public string Token { get; set; }
        public DateTime Expires { get; set; }

        
    }
    public class ActivePlayers
    {
        [Key]
        public string username { get; set; } = string.Empty;
        public int bankroll { get; set; }
        public int profit { get; set; }
        public string RefreshToken { get; set; }
        public DateTime jwtExpires { get; set; }
        public string CreateToken(string uname, IConfiguration configuration)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, uname),
                new Claim(ClaimTypes.Role, "Player"),
            };
            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(configuration.GetSection("AppSettings:Token").Value));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddMinutes(5),
                signingCredentials: credentials
                );
            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            //string jsonString = JsonSerializer.Serialize(jwt);

            return jwt;
        }

    }
    public class ActivePlayerStats
    {

    }
    public class GamesDatabase
    {
        [Key]
        public string Name { get; set; }
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
}
