using System.Collections.Generic;
using BLComponent;
using BLComponent.OutputPort;
using UnityEngine;
using System;
using System.IO;
using BLComponent.InputPorts;
using DBComponent;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Bang
{
    public static class DataCarrier
    {
        private static IGameManager CreateGameManager()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var config = new ConfigurationBuilder()
                .SetBasePath(Application.streamingAssetsPath)
                .AddJsonFile("appsettings.json")
                .Build();
            var path = Path.Combine(appData, config["SavesFileName"]!);
            path = Path.GetDirectoryName(path)!;
            Directory.CreateDirectory(path);
            var logger = new LoggerConfiguration()
                .ReadFrom.Configuration(config)
                .WriteTo.File(Path.Combine(path, "log.txt"))
                .CreateLogger();
            var serviceProvider = new ServiceCollection()
                .AddSingleton<IConfiguration>(config)
                .AddSingleton<Serilog.ILogger>(logger)
                .AddTransient<ICardRepository, CardRepository>()
                .AddTransient<ISaveRepository, SaveRepository>()
                .AddTransient<IGameView, GameView>()
                .AddSingleton<IGameManager, BLComponent.GameManager>()
                .BuildServiceProvider();
            return serviceProvider.GetService<IGameManager>();
        }
        
        public static IGameManager GameManager = CreateGameManager();
        
        public static List<string> Players = new();
        
        private static readonly Dictionary<CardName, string> CardNamesP = new()
        {
            [CardName.Bang] = "Бэнг!",
            [CardName.Beer] = "Пиво",
            [CardName.Missed] = "Мимо",
            [CardName.Panic] = "Паника",
            [CardName.GeneralStore] = "Магазин",
            [CardName.Indians] = "Индейцы",
            [CardName.Duel] = "Дуэль",
            [CardName.Gatling] = "Гатлинг",
            [CardName.CatBalou] = "Красотка",
            [CardName.Saloon] = "Салун",
            [CardName.Stagecoach] = "Диллижанс",
            [CardName.WellsFargo] = "Уэллс\nФарго",
            [CardName.Barrel] = "Бочка",
            [CardName.Scope] = "Прицел",
            [CardName.Mustang] = "Мустанг",
            [CardName.Dynamite] = "Динамит",
            [CardName.BeerBarrel] = "Бочка с пивом",
            [CardName.Jail] = "Тюрьма",
            [CardName.Volcanic] = "Волканик",
            [CardName.Schofield] = "Скофилд",
            [CardName.Remington] = "Ремингтон",
            [CardName.Carabine] = "Карабин",
            [CardName.Winchester] = "Винчестер"
        };
        
        public static IReadOnlyDictionary<CardName, string> CardNames => CardNamesP;

        private static readonly Dictionary<CardRank, string> CardRanksP = new()
        {
            [CardRank.Ace] = "A",
            [CardRank.King] = "K",
            [CardRank.Queen] = "Q",
            [CardRank.Jack] = "J",
            [CardRank.Ten] = "10",
            [CardRank.Nine] = "9",
            [CardRank.Eight] = "8",
            [CardRank.Seven] = "7",
            [CardRank.Six] = "6",
            [CardRank.Five] = "5",
            [CardRank.Four] = "4",
            [CardRank.Three] = "3",
            [CardRank.Two] = "2",
        };
        
        public static IReadOnlyDictionary<CardRank, string> CardRanks => CardRanksP;

        private static readonly Dictionary<CardSuit, Sprite> CardSuitsP = new()
        {
            [CardSuit.Spades] = Resources.Load<Sprite>("Spades"),
            [CardSuit.Hearts] = Resources.Load<Sprite>("Hearts"),
            [CardSuit.Diamonds] = Resources.Load<Sprite>("Diamonds"),
            [CardSuit.Clubs] = Resources.Load<Sprite>("Clubs"),
        };
        
        public static IReadOnlyDictionary<CardSuit, Sprite> CardSuits => CardSuitsP;

        private static readonly Dictionary<CardType, Sprite> CardWrapsP = new()
        {
            [CardType.Instant] = Resources.Load<Sprite>("InstantCardWrap"),
            [CardType.Equipment] = Resources.Load<Sprite>("EquipmentCardWrap"),
            [CardType.Weapon] = Resources.Load<Sprite>("WeaponCardWrap"),
        };
        
        public static IReadOnlyDictionary<CardType, Sprite> CardWraps => CardWrapsP;

        private static readonly Dictionary<PlayerRole, string> PlayerRolesP = new()
        {
            [PlayerRole.Outlaw] = "Бандит",
            [PlayerRole.Renegade] = "Ренегат",
            [PlayerRole.Sheriff] = "Шериф",
            [PlayerRole.DeputySheriff] = "Помощник шерифа"
        };
        
        public static IReadOnlyDictionary<PlayerRole, string> PlayerRoles => PlayerRolesP;
    }
}
