using System.Security.Claims;

namespace CasinoGodsAPI.Models.SignalRHubModels
{
    public class ClaimsFields
    {
        public string Username { get; init; }
        public int Bankroll { get; init; }
        public int Profit { get; init; }
        public string Role { get; init; }
        public ClaimsFields(ClaimsIdentity claimsIdentity)
        {
            Username = claimsIdentity.Claims.FirstOrDefault(c => c.Type == "Username").Value; ;
            Bankroll = int.Parse(claimsIdentity.Claims.SingleOrDefault(c => c.Type == "Current bankroll").Value); ;
            Profit = Bankroll - int.Parse(claimsIdentity.Claims.SingleOrDefault(c => c.Type == "Initial bankroll").Value);
            Role = claimsIdentity.Claims.SingleOrDefault(c => c.Type == "Role").Value;
        }
    }
}
