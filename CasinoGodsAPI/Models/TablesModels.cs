using CasinoGodsAPI.Controllers;
using CasinoGodsAPI.Data;
using CasinoGodsAPI.Migrations;
using CasinoGodsAPI.Services;
using CasinoGodsAPI.TablesModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Identity.Client;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Crypto;
using StackExchange.Redis;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Numerics;
using System.Reflection;
using System.Security.Claims;
using System.Threading;

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
        public static readonly Dictionary<string, Type> Types = new Dictionary<string, Type>() { { "Bacarrat", typeof(BacarratLobby) }, { "Blackjack", typeof(BlackjackLobby) }, { "Dragon Tiger", typeof(DragonTigerLobby) }, { "Roulette", typeof(RouletteLobby) }, { "War", typeof(WarLobby) } };
        
        public string Id = string.Empty;
        public Type TableType;
        public bool IsActive=false;
        public bool BetsClosed=false;
        public string ClosedBetsToken = string.Empty;
        public  CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        private ConcurrentDictionary<int, string> UsersAtSeat = new ConcurrentDictionary<int, string>();//represents which user is sitting at specific seat NOT USED IN ROULETTE,DRAGON TIGER, WAR
        private ConcurrentDictionary<string, int> SeatsTakenByUser = new ConcurrentDictionary<string, int>();//represent how many seats are taken by a specific user NOT USED IN ROULETTE,DRAGON TIGER, WAR
        private ConcurrentDictionary<int, List<int>> BetAtSeat = new ConcurrentDictionary<int, List<int>>();//represent how much is bet on specific seat NOT USED IN ROULETTE,DRAGON TIGER, WAR
        private ConcurrentDictionary<int, bool> SeatActiveInCurrentGame = new ConcurrentDictionary<int, bool>();//represent which seats are taken NOT USED IN ROULETTE,DRAGON TIGER, WAR

        public ConcurrentDictionary<string, bool> AllBetsPlaced = new ConcurrentDictionary<string, bool>();//represents if all players agree to play 
        public ConcurrentDictionary<string,List<int>>UserInitialBetsDictionary=new ConcurrentDictionary<string, List<int>>(); //represent list sent by users, those list are one before converting
        public ConcurrentDictionary<string, List<int>> UserFinalBets = new ConcurrentDictionary<string, List<int>>();//represents final betting list, used in roulette games index from 0 to 36
        public ConcurrentDictionary<string, int> UserWinnings = new ConcurrentDictionary<string, int>(); //represents how much user won during a single game
        private ActiveTablesDatabase ActiveTable= new ActiveTablesDatabase();

        //private List<int> ShuffledDeck = new List<int>();
        private Stack<int> ShuffledDeck = new Stack<int>();
        private int CardsPlayed=0;
        private int RedCard=0;

        public bool GameInProgress = false;

        private Dictionary<Type, Func<Task>> PlayingPhaseMethod = new Dictionary<Type, Func<Task>>();
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase redisDbLogin, redisDbJwt;
        public bool GameIsBeingPlayedRightNow=false;
        public ManageTableClass(string TableId,string Game, IServiceProvider serviceProvider, IConfiguration configuration, IConnectionMultiplexer redis)
        {
            Id = TableId;
            ActiveTable = LobbyHub.ActiveTables.FirstOrDefault(t => t.TableInstanceId.ToString() == TableId);
            for (int i = 0; i < ActiveTable.maxseats; i++)
            {
                var mytype = Types.FirstOrDefault(i => i.Key == Game).Value;                
                if (mytype != null) TableType = mytype;
                UsersAtSeat.TryAdd(i,"Empty Seat");
                BetAtSeat.TryAdd(i, new List<int>());
                SeatActiveInCurrentGame.TryAdd(i,false);               
            }
            _serviceProvider = serviceProvider;
            _configuration = configuration;
            _redis = redis;
            redisDbLogin = _redis.GetDatabase(0);
            redisDbJwt = _redis.GetDatabase(1);
            //redisGuestBankrolls = _redis.GetDatabase(2);
            PlayingPhaseMethod = new Dictionary<Type, Func<Task>>() {
            { typeof(BacarratLobby),BacarratPlayingPhase },{ typeof(BlackjackLobby),BlackjackPlayingPhase }, { typeof(DragonTigerLobby), DragonTigerPlayingPhase },
            { typeof(RouletteLobby),RoulettePlayingPhase}, { typeof(WarLobby),WarPlayingPhase } };           
        }
        private void ShuffleDecks(int NumberOfDecks)
        {
            ShuffledDeck.Clear();
            //List<int> CardsStorage = Enumerable.Repeat(NumberOfDecks, 10).ToList();
            List<int> CardsStorage = Enumerable.Repeat(NumberOfDecks, 52).ToList();
            List<int> AvailableCards = new List<int>();
            for(int j=0;j<52;j++)AvailableCards.Add(j);
            Random RandomObj= new Random();
            int RandomCardValue;


            //Console.WriteLine("Storage:");
            //int iterator = 0;
            //foreach(var storage  in CardsStorage) { Console.WriteLine(iterator+": "+storage);iterator++; }
            //Console.WriteLine("Available Card Values:");
            //foreach(var CardValues in AvailableCards) { Console.WriteLine(CardValues); }
            if (NumberOfDecks > 0) {
                //Console.WriteLine("LOOP START");
                for (int j = 0; j < 52*NumberOfDecks; j++)
                {
                    //Console.WriteLine("COUNT: " + AvailableCards.Count);
                    RandomCardValue = RandomObj.Next(0, AvailableCards.Count);
                    CardsStorage[AvailableCards[RandomCardValue]]--;
                    //ShuffledDeck.Add(AvailableCards[RandomCardValue]);
                    ShuffledDeck.Push(AvailableCards[RandomCardValue]);
                    //Console.WriteLine(ShuffledDeck.Last().ToString());
                    //Console.WriteLine(ShuffledDeck.Peek().ToString());
                    if (CardsStorage[AvailableCards[RandomCardValue]] == 0) AvailableCards.RemoveAt(RandomCardValue);

                    //Console.WriteLine("Storage:");
                    //iterator = 0;
                    //foreach (var storage in CardsStorage) { Console.WriteLine(iterator + ": " + storage); iterator++; }
                    //Console.WriteLine("Available Card Values:");
                    //foreach (var CardValues in AvailableCards) { Console.WriteLine(CardValues); }
                }
                CardsPlayed = 0;
                double RedCardPercent = RandomObj.Next(600, 850) * 52 * NumberOfDecks / 1000;// * 52 * NumberOfDecks;
                RedCard = (int)Math.Floor(RedCardPercent);
                Console.WriteLine("RedCard: " + RedCard);
                //foreach (var Cards in ShuffledDeck)Console.WriteLine(Cards);
            }
        }
        private List<int> CardsOnBoards(int howManyCards)
        {
            List<int>ListToSend= new List<int>();
            
            for (int i = 0; i < howManyCards; i++)
            {
                ListToSend.Add(ShuffledDeck.Peek());
                CardsPlayed++;
                ShuffledDeck.Pop();
            }
            return ListToSend;
        }
        ~ManageTableClass()
        {
            UsersAtSeat.Clear();
            SeatsTakenByUser.Clear();
            BetAtSeat.Clear();
            SeatActiveInCurrentGame.Clear();
        }
        public async Task StartGame()
        {
            if (TableType == typeof(BacarratLobby) || TableType == typeof(BlackjackLobby)|| TableType == typeof(DragonTigerLobby)) ShuffleDecks(ActiveTable.decks);
            while (IsActive) {                
                Console.WriteLine("New game:");
                GameInProgress = true;
                await BettingPhase();
                await PlayingPhaseMethod[TableType]();
                await ResultPhase();
   
                GameInProgress=false;
            } 
        }

        //////////////////////////////////////////////////////////////// BETTING PHASE ///////////////////////////////////////////////////////////
        private async Task BettingPhase()
        {
            ClearData();
            cancellationTokenSource = new CancellationTokenSource();
            ClosedBetsToken = GenerateToken();
            BetsClosed = false;
            List<int> BettingTimes = new List<int>();
            
            if (TableType == typeof(RouletteLobby)) BettingTimes = SplitBettingTime(ActiveTable.betTime);
            else BettingTimes.Add(ActiveTable.betTime);
            
            await TableService._hubContexts[TableType].Clients.Group(Id).SendAsync("ToggleBetting",true,""); //odblokowuje betowanie, restuje layout,startuje betowanie

            LookForInactivePlayers(Id); //Find JWT of user sitting at the table in Redis. If not found, kick user out
            
            if (TableType == typeof(RouletteLobby))
            {
                await Task.Delay(TimeSpan.FromSeconds(BettingTimes[0]), cancellationTokenSource.Token).ContinueWith(t => { });
                if (!BetsClosed)await TableService._hubContexts[TableType].Clients.Group(Id).SendAsync("BetsAreClosing");
                await Task.Delay(TimeSpan.FromSeconds(BettingTimes[1]), cancellationTokenSource.Token).ContinueWith(t => { });
                await TableService._hubContexts[TableType].Clients.Group(Id).SendAsync("ToggleBetting", false, ClosedBetsToken);//blokuje betowanie, pokazuje animacje losowania 
            }
            else
            {
                await Task.Delay(TimeSpan.FromSeconds(BettingTimes[0]), cancellationTokenSource.Token).ContinueWith(t => { });                
                await TableService._hubContexts[TableType].Clients.Group(Id).SendAsync("ToggleBetting", false, ClosedBetsToken);//blokuje betowanie, pokazuje animacje losowania 
            }
            GameIsBeingPlayedRightNow = true;
            
        }
        

        private List<int> SplitBettingTime(int betTime)
        {
            int RoundingInterval = 5;
            double AferBetsClosing = betTime * 0.25;
            double Re = AferBetsClosing % RoundingInterval;
            int AferBetsClosingRoundedUp = Convert.ToInt32(AferBetsClosing + RoundingInterval - Re);
            int BeforeBetsClosing = betTime - AferBetsClosingRoundedUp;                     
            return new List<int>() { BeforeBetsClosing, AferBetsClosingRoundedUp };
        }
        private string GenerateToken()
        {
           Random RandomNumber = new Random();
            int MyNumber = RandomNumber.Next(0, 0xFFFFFF);
            string Token = MyNumber.ToString("X").PadLeft(Int32.MaxValue.ToString("X").Length, '0').ToLower();
            return Token;
        }
        private async Task LookForInactivePlayers(string TableId)
        {
            List<bool> ActivePlayers = Enumerable.Repeat(true, ActiveTable.maxseats).ToList();

            var users = TableService.UserGroupDictionary.Where(t => t.Value == Id).Select(u=>u.Key).ToList();
            if (users.Count > ActivePlayers.Count) { Console.WriteLine("Too many players"); }
            else
            {
                int iterator = 0;
                foreach (var user in users)
                {
                    bool UserisActive=await GlobalFunctions.LookForTokenGlobal(user,redisDbLogin,redisDbJwt,_configuration);
                    if (!UserisActive)
                    {
                        var claimsIdentity = (ClaimsIdentity)TableService.UserContextDictionary[user].User.Identity;
                        var username = claimsIdentity.Claims.FirstOrDefault(c => c.Type == "Username").Value;
                        if (user != username) Console.WriteLine("Passed username and username from claim indentity do not match, username(from claims):"+username +", user:"+user);

                        else
                        {
                            var ConnectionId = TableService.UserContextDictionary[user].ConnectionId;
                            await TableService._hubContexts[TableType].Clients.Client(ConnectionId).SendAsync("Disconnect", "Session expired, log in again");
                        }
                    }
                    iterator++;
                }
            }                            
        }
        public bool IsBettingListVoid(List<int> Bets)
        {
            if(TableType==typeof(RouletteLobby)|| TableType == typeof(WarLobby)|| TableType == typeof(DragonTigerLobby)) //games which dont require taking a seat
            { 
                foreach(var bet in Bets) { if (bet >= ActiveTable.minBet && bet <= ActiveTable.maxBet) return false; } //at least one bet is valid 
                return true;
            }
            else //games which require taking a seat
            {

            }

            return false;
        }
       
        
        ///////////////////////////////////////////////////////////////////// PLAYING PHASE ////////////////////////////////////////////////////////////
        private async Task BacarratPlayingPhase()
        {

        }
        private async Task BlackjackPlayingPhase()
        {

        }
        private async Task DragonTigerPlayingPhase()
        {
            await TableService._hubContexts[TableType].Clients.Group(Id).SendAsync("TableChatReports", "Game Starts!");
            Random random = new Random();
            List<int> Cards = CardsOnBoards(2);
            List<double> CardsVal =new List<double>() { Math.Floor((double)Cards[0] / 4), Math.Floor((double)Cards[1] / 4) };
            List<int> CardsColor = new List<int>() { Cards[0] % 4, Cards[1] % 4 };


            int WinningPlace;
            string report = string.Empty;
            if (CardsVal[0] > CardsVal[1]) { report = "Dragon Wins!"; WinningPlace = 0; }
            else if (CardsVal[1] > CardsVal[0]) { report = "Tiger Wins!"; WinningPlace = 1; }
            else
            {
                if (CardsColor[0] == CardsColor[1]) { report = "Suited Tie!"; WinningPlace = 3; }
                else { report = "Tie!"; WinningPlace = 2; }
            }
            await Task.Delay(TimeSpan.FromSeconds(ActiveTable.actionTime));
            await TableService._hubContexts[TableType].Clients.Group(Id).SendAsync("Cards", Cards, report);
            DragonTigerConvertBettingLists();
            //await TableService._hubContexts[TableType].Clients.Group(Id).SendAsync("TableChatReports", report);


            foreach (var player in UserFinalBets) UserWinnings.TryAdd(player.Key, player.Value[WinningPlace]);
            await Task.Delay(TimeSpan.FromSeconds(5));
        }
        public async Task RoulettePlayingPhase()
        {
            await TableService._hubContexts[TableType].Clients.Group(Id).SendAsync("TableChatReports", "Game Starts! Ball is released!");
            Random random = new Random();
            int WinningNumber = random.Next(0, 37);
            await Task.Delay(TimeSpan.FromSeconds(10)); //symulacja krecenia sie kulki
            await TableService._hubContexts[TableType].Clients.Group(Id).SendAsync("TableChatReports", "Winning number: "+ WinningNumber);
            //await TableService._hubContexts[TableType].Clients.Group(Id).SendAsync("Winning number",WinningNumber);
            RouletteConvertBettingLists();
            Console.WriteLine("Wylosowana liczba: " + WinningNumber);
            foreach (var player in UserFinalBets) UserWinnings.TryAdd(player.Key, player.Value[WinningNumber]);            
        }
        private async Task WarPlayingPhase()
        {

        }

        private void RouletteConvertBettingLists()
        {            
            foreach (var BetList in UserInitialBetsDictionary)
            {
                if (BetList.Value.Count == 157)
                {
                    List<int>FinalList=new List<int>();
                    FinalList= Enumerable.Repeat(0, 37).ToList();
                    int i = 0;

                    for (i = 0; i < 37; i++)
                    {
                        FinalList[i] += 36 * BetList.Value[i];
                    }//STRAIGHT
                    for (i = 37; i < 49; i++)
                    {
                        FinalList[(i - 37) * 3] += 18 * BetList.Value[i];
                        FinalList[(i - 36) * 3] += 18 * BetList.Value[i];
                    }//SPLIT VERTICAL UPPER
                    for (i = 49; i < 50; i++)
                    {
                        FinalList[0] += 18 * BetList.Value[i];
                        FinalList[2] += 18 * BetList.Value[i];
                    }//SPLIT 0/2
                    for (i = 50; i < 61; i++)
                    {
                        FinalList[(i - 50) * 3 + 2] += 18 * BetList.Value[i];
                        FinalList[(i - 50) * 3 + 5] += 18 * BetList.Value[i];
                    }//SPLIT VERTICAL MID
                    for (i = 61; i < 62; i++)
                    {
                        FinalList[0] += 18 * BetList.Value[i];
                        FinalList[1] += 18 * BetList.Value[i];
                    }//SPLIT 0/1
                    for (i = 62; i < 73; i++)
                    {
                        FinalList[(i - 62) * 3 + 1] += 18 * BetList.Value[i];
                        FinalList[(i - 62) * 3 + 4] += 18 * BetList.Value[i];
                    }//SPLIT VERTICAL LOWER                 
                    for (i = 73; i < 85; i++)
                    {
                        FinalList[(i - 73) * 3 + 1] += 12 * BetList.Value[i];
                        FinalList[(i - 73) * 3 + 2] += 12 * BetList.Value[i];
                        FinalList[(i - 73) * 3 + 3] += 12 * BetList.Value[i];
                    }//STREET
                    for (i = 85; i < 97; i++)
                    {
                        FinalList[(i - 85) * 3 + 2] += 18 * BetList.Value[i];
                        FinalList[(i - 85) * 3 + 3] += 18 * BetList.Value[i];
                    }//SPLIT HORIZONTAL MID-UPPER
                    for (i = 97; i < 109; i++)
                    {
                        FinalList[(i - 97) * 3 + 1] += 18 * BetList.Value[i];
                        FinalList[(i - 97) * 3 + 2] += 18 * BetList.Value[i];
                    }//SPLIT HORIZONTAL MID-LOWER
                    for (i = 109; i < 110; i++)
                    {
                        FinalList[0] += 9 * BetList.Value[i];
                        FinalList[1] += 9 * BetList.Value[i];
                        FinalList[2] += 9 * BetList.Value[i];
                        FinalList[3] += 9 * BetList.Value[i];
                    }//CORNER 0/1/2/3
                    for (i = 110; i < 121; i++)
                    {
                        FinalList[(i - 110) * 3 + 1] += 6 * BetList.Value[i];
                        FinalList[(i - 110) * 3 + 2] += 6 * BetList.Value[i];
                        FinalList[(i - 110) * 3 + 3] += 6 * BetList.Value[i];
                        FinalList[(i - 110) * 3 + 4] += 6 * BetList.Value[i];
                        FinalList[(i - 110) * 3 + 5] += 6 * BetList.Value[i];
                        FinalList[(i - 110) * 3 + 6] += 6 * BetList.Value[i];
                    }//DOUBLE-STRAIGHT
                    for (i = 121; i < 122; i++)
                    {
                        FinalList[(i - 121) * 3 + 0] += 9 * BetList.Value[i];
                        FinalList[(i - 121) * 3 + 2] += 9 * BetList.Value[i];
                        FinalList[(i - 121) * 3 + 3] += 9 * BetList.Value[i];
                    }//TRIO 0/2/3
                    for (i = 122; i < 133; i++)
                    {
                        FinalList[(i - 122) * 3 + 2] += 9 * BetList.Value[i];
                        FinalList[(i - 122) * 3 + 3] += 9 * BetList.Value[i];
                        FinalList[(i - 122) * 3 + 5] += 9 * BetList.Value[i];
                        FinalList[(i - 122) * 3 + 6] += 9 * BetList.Value[i];
                    }//CORNER MID-UPPER
                    for (i = 133; i < 134; i++)
                    {
                        FinalList[(i - 133) * 3 + 0] += 9 * BetList.Value[i];
                        FinalList[(i - 133) * 3 + 1] += 9 * BetList.Value[i];
                        FinalList[(i - 133) * 3 + 2] += 9 * BetList.Value[i];
                    }//TRIO 0/1/2                    
                    for (i = 134; i < 145; i++)
                    {
                        FinalList[(i - 134) * 3 + 1] += 9 * BetList.Value[i];
                        FinalList[(i - 134) * 3 + 2] += 9 * BetList.Value[i];
                        FinalList[(i - 134) * 3 + 4] += 9 * BetList.Value[i];
                        FinalList[(i - 134) * 3 + 5] += 9 * BetList.Value[i];
                    }//CORNER MID-LOWER                    
                    for (i = 145; i < 148; i++)
                    {
                        for (int j = 0; j < 12; j++) FinalList[j * 3 + 3 - (i - 145)] += 3 * BetList.Value[i];
                    }//ROWS                    
                    for (i = 148; i < 151; i++)
                    {
                        for (int j = 0; j < 12; j++) FinalList[((i - 148) * 12) + (j + 1)] += 3 * BetList.Value[i];
                    }//DOZENS                    
                    for (i = 151; i < 153; i++)
                    {
                        for (int j = 0; j < 18; j++) FinalList[((i - 151) * 18) + (j + 1)] += 2 * BetList.Value[i];
                    }//HALFS                  
                    for (i = 153; i < 155; i++)
                    {
                        for (int j = 1; j < 19; j++) FinalList[j * 2 - (i - 153)] += 2 * BetList.Value[i];
                    }//EVEN -> ODD
                    for (i = 155; i < 156; i++)
                    {
                        int j;
                        for (j = 0; j < 5; j++) FinalList[j * 2 + 1] += 2 * BetList.Value[i];
                        for (j = 5; j < 9; j++) FinalList[j * 2 + 2] += 2 * BetList.Value[i];
                        for (j = 9; j < 14; j++) FinalList[j * 2 + 1] += 2 * BetList.Value[i];
                        for (j = 14; j < 18; j++) FinalList[j * 2 + 2] += 2 * BetList.Value[i];
                    }//RED
                    for (i = 156; i < 157; i++)
                    {
                        int j;
                        for (j = 0; j < 5; j++) FinalList[j * 2 + 2] += 2 * BetList.Value[i];
                        for (j = 5; j < 9; j++) FinalList[j * 2 + 1] += 2 * BetList.Value[i];
                        for (j = 9; j < 14; j++) FinalList[j * 2 + 2] += 2 * BetList.Value[i];
                        for (j = 14; j < 18; j++) FinalList[j * 2 + 1] += 2 * BetList.Value[i];
                    }//BLACK

                    UserFinalBets.TryAdd(BetList.Key, FinalList);
                    /*Console.WriteLine(BetList.Key);
                    Console.WriteLine("Jaka jest wygrana za konkretne pole");
                    int m = 0;
                    foreach (var bet in FinalList) { Console.Write(m + ": " + bet + ", "); m++; }
                    Console.WriteLine("\n");*/
                }
            }
        }
        private void DragonTigerConvertBettingLists()
        {
            foreach (var BetList in UserInitialBetsDictionary)
            {
                if (BetList.Value.Count == 4)
                {
                    List<int> FinalList = new List<int>();
                    FinalList = Enumerable.Repeat(0, 4).ToList();
                    int i = 0;

                    for(i=0; i < 2; i++) FinalList[i] += 2 * BetList.Value[i];
                    for (i = 2; i < 3; i++) FinalList[i] += 12 * BetList.Value[i];
                    for (i = 3; i < 4; i++) FinalList[i] += 51 * BetList.Value[i];
                    UserFinalBets.TryAdd(BetList.Key, FinalList);
                }

            }
        }
        /////////////////////////////////////////////////////////////////// RESULT PHASE /////////////////////////////////////////////////////////////////////////
        private async Task ResultPhase()
        {
            await UserClaimUpdate();
            int MaxWin = 0;
            List<string>MaxPlayers=new List<string>();
            foreach(var player in UserWinnings)
            {
                if (player.Value > 0) 
                { 
                    var ConnectionId = TableService.UserContextDictionary[player.Key].ConnectionId;
                    await TableService._hubContexts[TableType].Clients.Client(ConnectionId).SendAsync("Win","Congratulations! You win "+player.Value, player.Value);
                    if (player.Value > MaxWin) { MaxWin = player.Value; MaxPlayers.Clear(); MaxPlayers.Add(player.Key); }
                    else if (player.Value == MaxWin) MaxPlayers.Add(player.Key);
                    else { }                    
                }
                else { Console.WriteLine(player.Key + " chuja wygral"); }
            }
            await TableService._hubContexts[TableType].Clients.Group(Id).SendAsync("TableChatReports", "Players with biggest winnings this round:" + string.Join(", ", MaxPlayers));
            if (CardsPlayed >= RedCard) 
            {
                await TableService._hubContexts[TableType].Clients.Group(Id).SendAsync("TableChatReports", "Shuffling cards, please wait...");
                ShuffleDecks(ActiveTable.decks);
                await Task.Delay(TimeSpan.FromSeconds(5));
            }
            GameIsBeingPlayedRightNow = false;
            if (TableService.UserCountAtTablesDictionary[Id] < 1) { TableService.MakeTableInactive(Id); LobbyHub.DeleteTableInstance(Id); }
            else IsActive = true;
        }
        private async Task UserClaimUpdate()
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<CasinoGodsDbContext>();
            Dictionary<string,int>Profit= new Dictionary<string,int>();
           foreach (var player in UserInitialBetsDictionary) 
           {
                Profit.Add(player.Key,-1*player.Value.Sum());
           }
           foreach(var winner in UserWinnings)
           {
                Profit[winner.Key] = Profit[winner.Key]+winner.Value;
           }
           foreach(var player in Profit)
           {
                var ConnectionId = TableService.UserContextDictionary[player.Key].ConnectionId;
                var claimsIdentity = (ClaimsIdentity)TableService.UserContextDictionary[player.Key].User.Identity;

                var RoleClaim = claimsIdentity.FindFirst("Role");
                var BankrollClaim= claimsIdentity.FindFirst("Current bankroll");
                var InitialBankrollClaim = claimsIdentity.FindFirst("Initial bankroll");
                if (BankrollClaim == null|| InitialBankrollClaim==null|| RoleClaim==null) await TableService._hubContexts[TableType].Clients.Client(ConnectionId).SendAsync("Disconnect", "Claims Error");
                else
                {
                    int BankrollClaim_int = int.Parse(BankrollClaim.Value);
                    if (RoleClaim.Value != "Guest")
                    {                        
                      var PlayerDB =await dbContext.Players.FirstOrDefaultAsync(u => u.username == player.Key);
                        if (PlayerDB != null)
                        {
                            PlayerDB.profit += Profit[player.Key];
                            PlayerDB.bankroll = BankrollClaim_int + player.Value;
                            await dbContext.SaveChangesAsync();
                        }
                        else Console.WriteLine("Cant find user in database");
                    }

                    var newBankrollClaim = new Claim(BankrollClaim.Type, (BankrollClaim_int+player.Value).ToString()) ;
                    
                    claimsIdentity.RemoveClaim(BankrollClaim);
                    claimsIdentity.AddClaim(newBankrollClaim);
                    int ProfitToSent = BankrollClaim_int + player.Value - int.Parse(InitialBankrollClaim.Value);
                    await TableService._hubContexts[TableType].Clients.Client(ConnectionId).SendAsync("Bankroll", newBankrollClaim.Value, ProfitToSent.ToString());
                }
           }
        }
        private void ClearData()
        {
            UserFinalBets.Clear();
            UserInitialBetsDictionary.Clear();
            AllBetsPlaced.Clear();
            UserWinnings.Clear();
        }
    }
}
