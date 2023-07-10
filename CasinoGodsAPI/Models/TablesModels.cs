using CasinoGodsAPI.Controllers;
using CasinoGodsAPI.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Numerics;


namespace CasinoGodsAPI.Models
{
    public class BlackjackTableDTO  //DTO do zarzadzania stolem przez admina
    {
        public string gameType { get; set; } = string.Empty;
        public string actionType { get; set; } = string.Empty;
        public string name { get; set; }
        public int minBet { get; set; } = 0;
        public int maxBet { get; set; } = 100000;
        public int betTime { get; set; } = 30;
        public int actionTime { get; set; } = 15;
        public bool sidebet1 { get; set; } = true;
        public bool sidebet2 { get; set; } = true;
        public int decks { get; set; } = 6;
        public int maxSeats { get; set; } = 6;
        public bool CheckTable()
        {
            if (
                name == null ||
                minBet < 0 ||
                maxBet < minBet ||
                betTime < 0 ||
                actionTime < 0 ||
                decks < 1 ||
                decks > 10 ||
                maxSeats < 1 ||
                maxSeats > 10) return false;
            else return true;
        }
    }
    
    
}
