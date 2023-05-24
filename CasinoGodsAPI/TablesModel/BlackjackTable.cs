using CasinoGodsAPI.Controllers;
using CasinoGodsAPI.Data;
using CasinoGodsAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Numerics;


namespace CasinoGodsAPI.BlackjackTableModel
{
   public static class BlackjackActiveTables
    {
        public static List<BlackjackTable> ActiveTables { get; set; }
    }
    public class BlackjackSeat
    {
        public int ID { get; set; }
        public Player currentPlayer { get; set; } = null;
        public string playerJWT { get; set; } = string.Empty;
        public bool status { get; set; } = false;
        public int currentBet { get; set; } = 0;
        public int currentSidebet1 { get; set; } = 0;
        public int currentSidebet2 { get; set; } = 0;
        public BlackjackSeat(int input) { ID = input; }
    }
    public class BlackjackTableDatabase
    {
        [Key]
        public string name { get; set; }

        public int minBet { get; set; } = 0;
        public int maxBet { get; set; } = 100000;
        public int betTime { get; set; } = 30;
        public int actionTime { get; set; } = 15;
        public bool sidebet1 { get; set; } = true;
        public bool sidebet2 { get; set; } = true;
        public int decks { get; set; } = 6;
        public int seatsCount { get; set; } = 6;


        public bool CheckTable(BlackjackTableDatabase newTable)
        {
            if (
                newTable.minBet < 0 ||
                newTable.maxBet < newTable.minBet ||
                newTable.betTime < 0 ||
                newTable.actionTime < 0 ||
                newTable.decks < 1 ||
                newTable.decks>10||
                newTable.seatsCount < 1 ||
                newTable.seatsCount>10) return false;
            else return true;
        }
    }
    public class BlackjackTable
    {
        private Guid ID { get; set; } = Guid.NewGuid();
        public string name { get; set; } = string.Empty;
        private int minBet { get; set; } = 0;
        private int maxBet { get; set; } = 100000;
        private int betTime { get; set; } = 30;
        private int actionTime { get; set; } = 15;
        private bool sidebet1 { get; set; } = true;
        private bool sidebet2 { get; set; } = true;
        private int decks { get; set; } = 6;
        private int seatsCount { get; set; } = 6;
        private Dealer currentDealer { get; set; } = null;
        private List<BlackjackSeat> seats { get; set; } = null;


        public BlackjackTable(BlackjackTableDatabase input, CasinoGodsDbContext _casinoGodsDbContext)
        {
            name = input.name;
            minBet = input.minBet;
            maxBet = input.maxBet;
            betTime = input.betTime;
            actionTime = input.actionTime;
            sidebet1 = input.sidebet1;
            sidebet2 = input.sidebet2;
            decks = input.decks;
            seatsCount = input.seatsCount;
            currentDealer = ChangeDealer(_casinoGodsDbContext);
            seats = new List<BlackjackSeat>(seatsCount);
            for (int i = 0; i < seatsCount; i++) seats[i] = new BlackjackSeat(i);
        }
        public void deleteTable()
        {
            //freeDealer(){ }
            //remove table from active tables list
        }

        public Dealer ChangeDealer(CasinoGodsDbContext _casinoGodsDbContext)
        {
            Dealer currD = _casinoGodsDbContext.Dealers.SingleOrDefault(d => d.Name == currentDealer.Name);
            if (currentDealer != null)
            {
                if (currD == null) { Console.WriteLine("Current dealer not found in the database"); }
                else { currD.active = false;Dealer.freeDealers++; _casinoGodsDbContext.SaveChangesAsync(); }
            }


            if (Dealer.freeDealers == -10) Dealer.freeDealers =  _casinoGodsDbContext.Dealers.Count(d => d.active == false);
            if (Dealer.freeDealers < 1) { return null; }
            else if (Dealer.freeDealers == 1) { return currD; }
            else
            {
                Dealer newD = new Dealer();
                int newDealerID;
                bool ok = true;
                do
                {
                    newDealerID = new Random().Next(0, _casinoGodsDbContext.Dealers.Count());
                    newD = _casinoGodsDbContext.Dealers.SingleOrDefault(d => d.ID == newDealerID);
                    if (newD.active == true) ok = false;
                    else {ok = true;  Dealer.freeDealers--; newD.active = true;}
                }
                while (!ok);
                _casinoGodsDbContext.SaveChangesAsync();
                return newD;    
            }
        }
    }

    
}
