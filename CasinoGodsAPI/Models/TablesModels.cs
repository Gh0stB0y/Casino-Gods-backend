using CasinoGodsAPI.Controllers;
using CasinoGodsAPI.Data;
using CasinoGodsAPI.Migrations;
using CasinoGodsAPI.TablesModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Numerics;


namespace CasinoGodsAPI.Models
{
    public class TableDTO  //DTO do zarzadzania stolem przez admina
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

    public class ManageTableClass
    {
        //kazdy obiekt reprezentuje stol do gry

        public string Id=string.Empty;
        private readonly IHubContext<Hub> hubContext;
        public ConcurrentDictionary<int,string> UsersAtSeat = new ConcurrentDictionary<int,string>();//represents which user is sitting at specific seat
        public ConcurrentDictionary<string, int> SeatsTakenByUser = new ConcurrentDictionary<string, int>();//represent how many seats are taken by a specific user
        public ConcurrentDictionary<int, List<int>> BetAtSeat = new ConcurrentDictionary<int, List<int>>();//represent how much is bet on specific seat
        public ConcurrentDictionary<int,bool>SeatActiveInCurrentGame=new ConcurrentDictionary<int,bool>();//represent which seats are taken 
        public ConcurrentStack<bool>AllBetsPlaced=new ConcurrentStack<bool>();//represents if all players agree to play 

        public static ConcurrentDictionary<string, bool> ExistingTableThreads = new ConcurrentDictionary<string, bool>();
        public static ConcurrentDictionary<string, ManualResetEventSlim> IsTableActive = new ConcurrentDictionary<string, ManualResetEventSlim>();

        public ManageTableClass(string TableId)
        {
            Id = TableId;
            for (int i = 0; i < LobbyHub.ActiveTables.Where(t => t.TableInstanceId.ToString() == TableId).FirstOrDefault().maxseats; i++)
            {

                UsersAtSeat.TryAdd(i,"Empty Seat");
                BetAtSeat.TryAdd(i, new List<int>());
                SeatActiveInCurrentGame.TryAdd(i,false);
                AllBetsPlaced.Push(false);
            }
        }
        ~ManageTableClass()
        {
            UsersAtSeat.Clear();
            SeatsTakenByUser.Clear();
            BetAtSeat.Clear();
        }
        
    }

}
