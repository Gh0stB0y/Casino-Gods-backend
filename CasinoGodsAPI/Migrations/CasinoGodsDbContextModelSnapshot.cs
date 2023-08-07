﻿// <auto-generated />
using System;
using CasinoGodsAPI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace CasinoGodsAPI.Migrations
{
    [DbContext(typeof(CasinoGodsDbContext))]
    partial class CasinoGodsDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("CasinoGodsAPI.Models.ActiveTablesDatabase", b =>
                {
                    b.Property<Guid>("TableInstanceId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Game")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("actionTime")
                        .HasColumnType("int");

                    b.Property<int>("betTime")
                        .HasColumnType("int");

                    b.Property<int>("decks")
                        .HasColumnType("int");

                    b.Property<int>("maxBet")
                        .HasColumnType("int");

                    b.Property<int>("maxseats")
                        .HasColumnType("int");

                    b.Property<int>("minBet")
                        .HasColumnType("int");

                    b.Property<bool>("sidebet1")
                        .HasColumnType("bit");

                    b.Property<bool>("sidebet2")
                        .HasColumnType("bit");

                    b.HasKey("TableInstanceId");

                    b.ToTable("ActiveTables");
                });

            modelBuilder.Entity("CasinoGodsAPI.Models.GamePlusPlayer", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("draws")
                        .HasColumnType("int");

                    b.Property<string>("gameNameName")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("gamesPlayed")
                        .HasColumnType("int");

                    b.Property<int>("loses")
                        .HasColumnType("int");

                    b.Property<Guid?>("playerId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<float>("profit")
                        .HasColumnType("real");

                    b.Property<float>("winratio")
                        .HasColumnType("real");

                    b.Property<int>("wins")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("gameNameName");

                    b.HasIndex("playerId");

                    b.ToTable("GamePlusPlayersTable");
                });

            modelBuilder.Entity("CasinoGodsAPI.Models.GamesDatabase", b =>
                {
                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Name");

                    b.ToTable("GamesList");
                });

            modelBuilder.Entity("CasinoGodsAPI.Models.LobbyTableData", b =>
                {
                    b.Property<Guid>("TableInstanceId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("TablePath")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("TableTypeCKGame")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("TableTypeCKname")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("TableInstanceId");

                    b.HasIndex("TableTypeCKname", "TableTypeCKGame");

                    b.ToTable("LobbyTableData");
                });

            modelBuilder.Entity("CasinoGodsAPI.Models.Player", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("bankroll")
                        .HasColumnType("int");

                    b.Property<DateTime>("birthdate")
                        .HasColumnType("datetime2");

                    b.Property<string>("email")
                        .HasColumnType("nvarchar(max)");

                    b.Property<byte[]>("passHash")
                        .HasColumnType("varbinary(max)");

                    b.Property<byte[]>("passSalt")
                        .HasColumnType("varbinary(max)");

                    b.Property<string>("password")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("profit")
                        .HasColumnType("int");

                    b.Property<string>("username")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Players");
                });

            modelBuilder.Entity("CasinoGodsAPI.Models.TablesDatabase", b =>
                {
                    b.Property<string>("CKname")
                        .HasColumnType("nvarchar(450)")
                        .HasColumnOrder(0);

                    b.Property<string>("CKGame")
                        .HasColumnType("nvarchar(450)")
                        .HasColumnOrder(1);

                    b.Property<int>("actionTime")
                        .HasColumnType("int");

                    b.Property<int>("betTime")
                        .HasColumnType("int");

                    b.Property<int>("decks")
                        .HasColumnType("int");

                    b.Property<int>("maxBet")
                        .HasColumnType("int");

                    b.Property<int>("maxseats")
                        .HasColumnType("int");

                    b.Property<int>("minBet")
                        .HasColumnType("int");

                    b.Property<bool>("sidebet1")
                        .HasColumnType("bit");

                    b.Property<bool>("sidebet2")
                        .HasColumnType("bit");

                    b.HasKey("CKname", "CKGame");

                    b.HasIndex("CKGame");

                    b.ToTable("TablesList");
                });

            modelBuilder.Entity("CasinoGodsAPI.Models.GamePlusPlayer", b =>
                {
                    b.HasOne("CasinoGodsAPI.Models.GamesDatabase", "gameName")
                        .WithMany()
                        .HasForeignKey("gameNameName");

                    b.HasOne("CasinoGodsAPI.Models.Player", "player")
                        .WithMany()
                        .HasForeignKey("playerId");

                    b.Navigation("gameName");

                    b.Navigation("player");
                });

            modelBuilder.Entity("CasinoGodsAPI.Models.LobbyTableData", b =>
                {
                    b.HasOne("CasinoGodsAPI.Models.TablesDatabase", "TableType")
                        .WithMany("ActiveTables")
                        .HasForeignKey("TableTypeCKname", "TableTypeCKGame");

                    b.Navigation("TableType");
                });

            modelBuilder.Entity("CasinoGodsAPI.Models.TablesDatabase", b =>
                {
                    b.HasOne("CasinoGodsAPI.Models.GamesDatabase", "Game")
                        .WithMany("Tables")
                        .HasForeignKey("CKGame")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Game");
                });

            modelBuilder.Entity("CasinoGodsAPI.Models.GamesDatabase", b =>
                {
                    b.Navigation("Tables");
                });

            modelBuilder.Entity("CasinoGodsAPI.Models.TablesDatabase", b =>
                {
                    b.Navigation("ActiveTables");
                });
#pragma warning restore 612, 618
        }
    }
}
