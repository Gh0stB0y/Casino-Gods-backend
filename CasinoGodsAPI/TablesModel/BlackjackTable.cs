using CasinoGodsAPI.Controllers;
using CasinoGodsAPI.Data;
using CasinoGodsAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Numerics;


namespace CasinoGodsAPI.TablesModel
{
   
    
    public class TablesDatabase
    {

        [Key, Column(Order = 1)]
        public GamesDatabase game{ get; set;}
        [Key, Column(Order =0)]
        public string name { get; set; }

        public int minBet { get; set; } = 0;
        public int maxBet { get; set; } = 100000;
        public int betTime { get; set; } = 30;    
    } //MODEL OGOLNEGO STOLU
    public class TableDataDTO   //DTO DO PRZESYLANIA INFO O DOSTEPNYCH GRACH I STOLACH
    {
        public string gameNames { get; set; } = string.Empty;
        public List<string> tableNames { get; set; } = new List<string>();
        public List<int> minBets { get; set; } = new List<int>();
        public List<int> maxBets { get; set; } = new List<int>();
    }
    public class BlackjackTablesDatabase //MODEL STOLU BLACKJACKA DO BAZY DANYCH
    {
        [Key, Column(Order = 0)]
        public int Id { get; set; }
        [Key, Column(Order = 0)]
        public TablesDatabase table { get; set; }
        public int actionTime { get; set; } = 15;
        public bool sidebet1 { get; set; } = true;
        public bool sidebet2 { get; set; } = true;
        public int decks { get; set; } = 6;
        public int seatsCount { get; set; } = 6;

        public bool CheckTable(BlackjackTablesDatabase newTable)
        {
            if (
                newTable.table.name==null||
                newTable.table.minBet < 0 ||
                newTable.table.maxBet < newTable.table.minBet ||
                newTable.table.betTime < 0 ||
                newTable.actionTime < 0 ||
                newTable.decks < 1 ||
                newTable.decks > 10 ||
                newTable.seatsCount < 1 ||
                newTable.seatsCount > 10) return false;
            else return true;
        }
    }
    public class BlackjackTableDTO  //DTO do zarzadzania stolem przez admina
    {
        public string actionType { get; set; } = string.Empty;
        public string name { get; set; }
        public int minBet { get; set; } = 0;
        public int maxBet { get; set; } = 100000;
        public int betTime { get; set; } = 30;
        public int actionTime { get; set; } = 15;
        public bool sidebet1 { get; set; } = true;
        public bool sidebet2 { get; set; } = true;
        public int decks { get; set; } = 6;
        public int seatsCount { get; set; } = 6;
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
                seatsCount < 1 ||
                seatsCount > 10) return false;
            else return true;
        }
    }

    public class BlackjackTable //MODEL DO OBSLUGI HUBA
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


        public BlackjackTable(BlackjackTablesDatabase input, CasinoGodsDbContext _casinoGodsDbContext)
        {
            name = input.table.name;
            minBet = input.table.minBet;
            maxBet = input.table.maxBet;
            betTime = input.table.betTime;
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
                else { currD.active = false; Dealer.freeDealers++; _casinoGodsDbContext.SaveChangesAsync(); }
            }


            if (Dealer.freeDealers == -10) Dealer.freeDealers = _casinoGodsDbContext.Dealers.Count(d => d.active == false);
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
                    else { ok = true; Dealer.freeDealers--; newD.active = true; }
                }
                while (!ok);
                _casinoGodsDbContext.SaveChangesAsync();
                return newD;
            }
        }
    }

    public static class BlackjackActiveTables
    {
        public static List<BlackjackTable> ActiveTables { get; set; }
    } //LISTA AKTYWNYCH STOLOW DO HUBA
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
}
