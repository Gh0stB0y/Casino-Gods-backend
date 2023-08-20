﻿using CasinoGodsAPI.Models.DatabaseModels;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;

namespace CasinoGodsAPI.Models
{

    public class LobbyTableData
    {
        [Key]
        public Guid TableInstanceId { get; set; } = new Guid();
        public string TablePath { get; set; }=string.Empty;
        public Tables TableType { get; set; }

    }
    public class LobbyTableDataDTO
    {
        public string Id { get; set;} = string.Empty;
        public string TablePath { get; set;} = string.Empty;
        public string Name { get; set; }=string.Empty;
        public string Game { get; set; }=string.Empty;  
        public int minBet { get; set; } = 0;
        public int maxBet { get; set; } = 0;
        public int betTime { get; set; } = 0;
        public bool sidebets { get; set; } = true;
        public bool sidebet1 { get; set; } = true;
        public bool sidebet2 { get; set; } = true;
        public int currentSeats { get; set; } = 0;
        public int maxSeats { get; set; } = 0;
        public LobbyTableDataDTO(ActiveTablesDB table)
        {
            Id = table.TableInstanceId.ToString();
            Name = table.Name;
            Game = table.Game;
            minBet = table.MinBet;
            maxBet = table.MaxBet;
            betTime = table.BetTime;
            maxSeats = table.Maxseats;
            sidebet1 = table.Sidebet1;
            sidebet2 = table.Sidebet2;
            if (table.Sidebet1 == true || table.Sidebet2 == true) sidebets = true;
            else sidebets = false;
        }
        public LobbyTableDataDTO(LobbyTableData baseObj)
        {
            if (baseObj == null) throw new ArgumentNullException();

            Id = baseObj.TableInstanceId.ToString();
            TablePath = baseObj.TablePath;
            Name = baseObj.TableType.CKname;
            Game = baseObj.TableType.CKGame;
            minBet = baseObj.TableType.MinBet;
            maxBet = baseObj.TableType.MaxBet;
            betTime = baseObj.TableType.BetTime;
            maxSeats = baseObj.TableType.Maxseats;
            sidebet1= baseObj.TableType.Sidebet1;
            sidebet2 = baseObj.TableType.Sidebet2;
            if (baseObj.TableType.Sidebet1 == true || baseObj.TableType.Sidebet2 == true) sidebets = true;
            else sidebets=false;
        }
    }
    public class PlayerLobbyData
    {
        public string connectionID { get; set; } = string.Empty;
        public string username { get; set; } = string.Empty;
        public string jwt { get; set; } =string.Empty;
        public int bankroll { get; set; } = 0;
    }
    public class TableInitialDataDTO   //DTO DO PRZESYLANIA INFO O DOSTEPNYCH GRACH
    {
        public string jwt { get; set; } = string.Empty;
        public List<string> gameNames { get; set; }
    }
}
