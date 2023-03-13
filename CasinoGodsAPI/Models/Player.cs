using Microsoft.VisualBasic;
using System;
using System.Data.SqlTypes;

namespace CasinoGodsAPI.Models
{
    public class Player
    {
        public Guid Id { get; set; }
        public string username { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public int bankroll { get; set; }
        public int profit { get; set; }
        public DateTime birthdate { get; set; }

    }
}
