﻿// <auto-generated />
using System;
using CasinoGodsAPI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace CasinoGodsAPI.Migrations
{
    [DbContext(typeof(CasinoGodsDbContext))]
    [Migration("20230821203722_updatedTables")]
    partial class updatedTables
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("CasinoGodsAPI.Models.DatabaseModels.ActiveTablesDB", b =>
                {
                    b.Property<Guid>("TableInstanceId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("ActionTime")
                        .HasColumnType("int");

                    b.Property<int>("BetTime")
                        .HasColumnType("int");

                    b.Property<int>("Decks")
                        .HasColumnType("int");

                    b.Property<string>("Game")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("MaxBet")
                        .HasColumnType("int");

                    b.Property<int>("Maxseats")
                        .HasColumnType("int");

                    b.Property<int>("MinBet")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("Sidebet1")
                        .HasColumnType("bit");

                    b.Property<bool>("Sidebet2")
                        .HasColumnType("bit");

                    b.HasKey("TableInstanceId");

                    b.ToTable("ActiveTables");
                });

            modelBuilder.Entity("CasinoGodsAPI.Models.DatabaseModels.GamePlayerTable", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("Draws")
                        .HasColumnType("int");

                    b.Property<string>("GameNameName")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("GamesPlayed")
                        .HasColumnType("int");

                    b.Property<int>("Loses")
                        .HasColumnType("int");

                    b.Property<Guid?>("PlayerId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<float>("Profit")
                        .HasColumnType("real");

                    b.Property<float>("Winratio")
                        .HasColumnType("real");

                    b.Property<int>("Wins")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("GameNameName");

                    b.HasIndex("PlayerId");

                    b.ToTable("GamePlusPlayersTable");
                });

            modelBuilder.Entity("CasinoGodsAPI.Models.DatabaseModels.Games", b =>
                {
                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Name");

                    b.ToTable("GamesList");
                });

            modelBuilder.Entity("CasinoGodsAPI.Models.DatabaseModels.Player", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("Bankroll")
                        .HasColumnType("int");

                    b.Property<DateTime>("Birthdate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Email")
                        .HasColumnType("nvarchar(max)");

                    b.Property<byte[]>("PassHash")
                        .HasColumnType("varbinary(max)");

                    b.Property<byte[]>("PassSalt")
                        .HasColumnType("varbinary(max)");

                    b.Property<string>("Password")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Profit")
                        .HasColumnType("int");

                    b.Property<string>("Username")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Players");
                });

            modelBuilder.Entity("CasinoGodsAPI.Models.DatabaseModels.Tables", b =>
                {
                    b.Property<string>("CKname")
                        .HasColumnType("nvarchar(450)")
                        .HasColumnOrder(0);

                    b.Property<string>("CKGame")
                        .HasColumnType("nvarchar(450)")
                        .HasColumnOrder(1);

                    b.Property<int>("ActionTime")
                        .HasColumnType("int");

                    b.Property<int>("BetTime")
                        .HasColumnType("int");

                    b.Property<int>("Decks")
                        .HasColumnType("int");

                    b.Property<int>("MaxBet")
                        .HasColumnType("int");

                    b.Property<int>("Maxseats")
                        .HasColumnType("int");

                    b.Property<int>("MinBet")
                        .HasColumnType("int");

                    b.Property<bool>("Sidebet1")
                        .HasColumnType("bit");

                    b.Property<bool>("Sidebet2")
                        .HasColumnType("bit");

                    b.HasKey("CKname", "CKGame");

                    b.HasIndex("CKGame");

                    b.ToTable("TablesList");
                });

            modelBuilder.Entity("CasinoGodsAPI.Models.DatabaseModels.GamePlayerTable", b =>
                {
                    b.HasOne("CasinoGodsAPI.Models.DatabaseModels.Games", "GameName")
                        .WithMany()
                        .HasForeignKey("GameNameName");

                    b.HasOne("CasinoGodsAPI.Models.DatabaseModels.Player", "Player")
                        .WithMany()
                        .HasForeignKey("PlayerId");

                    b.Navigation("GameName");

                    b.Navigation("Player");
                });

            modelBuilder.Entity("CasinoGodsAPI.Models.DatabaseModels.Tables", b =>
                {
                    b.HasOne("CasinoGodsAPI.Models.DatabaseModels.Games", "Game")
                        .WithMany("Tables")
                        .HasForeignKey("CKGame")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Game");
                });

            modelBuilder.Entity("CasinoGodsAPI.Models.DatabaseModels.Games", b =>
                {
                    b.Navigation("Tables");
                });
#pragma warning restore 612, 618
        }
    }
}
