using System.Globalization;
using System.Text;
using BLComponent;
using BLComponent.InputPorts;
using BLComponent.OutputPort;
using DBComponent;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace CLComponent;

public enum EnterPlayerNamesRc
{
    Ok,
    NameError,
    AmountError,
    EmptyNameError,
    SameNameError,
}

public enum EGameLoopMenu
{
    PlayCard,
    DiscardCard,
    ShowInfo,
    EndTurn,
    SaveAndQuit,
    Error
}

public enum EGameMenu
{
    Start,
    Load,
    Exit,
}

public static class Program
{
    private static IGameManager _gameManager = null!;
    private static Utils _utils = null!;
    private static List<string> _playerNames = null!;
    
    private sealed class GameView : IGameView
    {
        private readonly Dictionary<CardName, Action<Guid, bool?, Guid?, Card?>> _cardHandlers = new()
        {
            [CardName.Bang] = (id, work, target, _) => BangResult(id, work!.Value, target!.Value),
            [CardName.Barrel] = (id, work, _, card) => BarrelResult(id, work, card!),
            [CardName.Missed] = (id, _, _, _) => 
                Console.WriteLine($"{_gameManager.Players.First(p => p.Id == id).Name} " +
                                  $"сбросил {_utils.CardNames[CardName.Missed]}."),
            [CardName.Panic] = (id, _, target, _) => 
                Console.WriteLine($"{_gameManager.Players.First(p => p.Id == id).Name} " +
                                  $"забрал карту у игрока " +
                                  $"{_gameManager.Players.First(p => p.Id == target!.Value).Name}."),
            [CardName.CatBalou] = (id, _, target, card) => 
                Console.WriteLine($"{_gameManager.Players.First(p => p.Id == id).Name} " +
                                  $"сбросил карту \"{_utils.CardToString(card!)}\" " +
                                  $"у игрока {_gameManager.Players.First(p => p.Id == target!.Value).Name}."),
            [CardName.GeneralStore] = (_, work, _, _) => GeneralStoreResult(work!.Value),
            [CardName.Indians] = (id, work, _, _) => DuelAndIndiansResult(id, CardName.Indians, work),
            [CardName.Duel] = (id, work, _, _) => DuelAndIndiansResult(id, CardName.Duel, work),
            [CardName.Beer] = (id, _, _, _) => HandleCommonCard(id, CardName.Beer),
            [CardName.Gatling] = (id, _, _, _) => HandleCommonCard(id, CardName.Gatling),
            [CardName.Saloon] = (id, _, _, _) => HandleCommonCard(id, CardName.Saloon),
            [CardName.Stagecoach] = (id, _, _, _) => HandleCommonCard(id, CardName.Stagecoach),
            [CardName.WellsFargo] = (id, _, _, _) => HandleCommonCard(id, CardName.WellsFargo),
            [CardName.Scope] = (id, _, _, _) => HandleCommonCard(id, CardName.Scope),
            [CardName.Mustang] = (id, _, _, _) => HandleCommonCard(id, CardName.Mustang),
            [CardName.Volcanic] = (id, work, _, _) => HandleWeapon(id, CardName.Volcanic, work!.Value),
            [CardName.Schofield] = (id, work, _, _) => HandleWeapon(id, CardName.Schofield, work!.Value),
            [CardName.Remington] = (id, work, _, _) => HandleWeapon(id, CardName.Remington, work!.Value),
            [CardName.Carabine] = (id, work, _, _) => HandleWeapon(id, CardName.Carabine, work!.Value),
            [CardName.Winchester] = (id, work, _, _) => HandleWeapon(id, CardName.Winchester, work!.Value),
            [CardName.Dynamite] = (id, work, _, card) => DynamiteResult(id, work, card!),
            [CardName.BeerBarrel] = (id, work, _, card) => BeerBarrelResult(id, work, card!),
            [CardName.Jail] = (id, work, target, card) => JailResult(id, work, target, card!)
        };
        
        public Task<Guid> GetPlayerIdAsync(IReadOnlyList<Player> players, Guid currentPlayerId)
        {
            Console.Clear();
            var i = 1;
            foreach (var playerId in players.Select(p => p.Id))
            {
                var range = _gameManager.GetRange(currentPlayerId, playerId);
                Console.WriteLine($"{i++}. {_gameManager.Players.First(p => p.Id == playerId).Name} ({range})");
            }
            Console.WriteLine("Выберите игрока:");
            if (!int.TryParse(Console.ReadLine(), NumberStyles.Integer,
                    CultureInfo.InvariantCulture, out var chosen) || chosen - 1 < 0 || chosen - 1 >= players.Count)
            {
                return Task.FromResult(Guid.Empty);
            }
            Console.Clear();
            return Task.FromResult(players[chosen - 1].Id);
        }
    
        public Task<Guid> GetCardIdAsync(IReadOnlyList<Card?> cards, int unknownCardsCount, Guid? playerId)
        {
            Console.Clear();
            if (playerId.HasValue)
                Console.WriteLine($"Карту выбирает игрок " +
                                  $"{_gameManager.Players.First(p => p.Id == playerId.Value).Name}");
            for (var i = 0; i < unknownCardsCount; i++)
                Console.WriteLine($"{i + 1}. Неизвестная карта");
            for (var i = unknownCardsCount; i < cards.Count; i++)
                Console.WriteLine(cards[i] is not null ? $"{i + 1}. {_utils.CardToString(cards[i]!)}" : "Оружия нет");
        
            Console.WriteLine("Выберите карту:");
            if (!int.TryParse(Console.ReadLine(), NumberStyles.Integer,
                    CultureInfo.InvariantCulture, out var chosen) || chosen - 1 < 0 ||
                chosen - 1 >= cards.Count || cards[chosen - 1] is null)
            {
                return Task.FromResult(Guid.Empty);
            }
            Console.Clear();
            return Task.FromResult(cards[chosen - 1]!.Id);
        }

        public Task<bool> YesOrNoAsync(Guid playerId, CardName name)
        {
            Console.WriteLine($"{_gameManager.Players.First(p => p.Id == playerId).Name}, " +
                              $"вы хотите сбросить \"{_utils.CardNames[name]}\"?(д/н)");
            var input = Console.ReadLine() ?? string.Empty;
            return Task.FromResult(input == "д");
        }
        
        public void ShowCardResult(Guid curPlayerId, CardName name, bool? didWork, Guid? targetId, Card? card) => 
            _cardHandlers[name].Invoke(curPlayerId, didWork, targetId, card);

        public void CardAddedInHand(Guid cardId, Guid playerId) { }

        public void CardAddedOnBoard(Guid cardId, Guid playerId) { }

        public void WeaponAdded(Guid cardId, Guid playerId) { }

        public void CardDiscarded(Guid cardId) { }

        public void CardReturnedToDeck(Guid cardId) { }

        private static void BangResult(Guid curPlayerId, bool didWork, Guid targetId)
        {
            Console.WriteLine(didWork
                ? $"{_gameManager.Players.First(p => p.Id == targetId).Name} потерял одно здоровье."
                : $"{_gameManager.Players.First(p => p.Id == curPlayerId).Name} выстрелил в игрока " +
                  $"{_gameManager.Players.First(p => p.Id == targetId).Name}...");
        }

        private static void BarrelResult(Guid curPlayerId, bool? didWork, Card card)
        {
            string outString;
            if (didWork is null)
            {
                Console.Clear();
                outString = $"{_gameManager.Players.First(p => p.Id == curPlayerId).Name} разыграл бочку...";
            }
            else
            {
                outString = $"{_gameManager.Players.First(p => p.Id == curPlayerId).Name} проверил условие бочки...\n" +
                            $"Из колоды была получена карта \"{_utils.CardToString(card)}\"\n" +
                            (didWork.Value ? "Бочка сработала!" : "Бочка не сработала...");
            }
            Console.WriteLine(outString);
        }

        private static void GeneralStoreResult(bool didWork)
        {
            string outString;
            if (didWork)
                outString = "Все игроки получили по карте из магазина.";
            else
            {
                Console.Clear();
                outString = "Неверный номер карты.";
                EnterToContinue();
            }
            Console.WriteLine(outString);
        }

        private static void DuelAndIndiansResult(Guid curPlayerId, CardName name, bool? didWork)
        {
            string outString;
            if (didWork is null)
            {
                if (name is CardName.Indians)
                    Console.Clear();
                outString = $"{_gameManager.Players.First(p => p.Id == curPlayerId).Name} " +
                            $"разыграл карту {_utils.CardNames[name]}...";
            }
            else if (didWork.Value)
                outString = $"{_gameManager.Players.First(p => p.Id == curPlayerId).Name} потерял одно здоровье.";
            else
                outString = $"{_gameManager.Players.First(p => p.Id == curPlayerId).Name} " +
                            $"сбросил {_utils.CardNames[CardName.Bang]}.";
            Console.WriteLine(outString);
        }

        private static void DynamiteResult(Guid curPlayerId, bool? didWork, Card card)
        {
            string outString;
            if (didWork is null)
            {
                Console.Clear();
                outString = $"{_gameManager.Players.First(p => p.Id == curPlayerId).Name} " +
                            $"разыграл карту {_utils.CardNames[CardName.Dynamite]}...";
            }
            else
                outString = $"Из колоды получена карта \"{_utils.CardToString(card)}\"\n" +
                                  (didWork.Value
                                      ? $"Динамит взорвался! " +
                                        $"{_gameManager.Players.First(p => p.Id == curPlayerId).Name} " +
                                        $"потерял три единицы здоровья."
                                      : $"Динамит перешёл игроку " +
                                        $"{_gameManager.Players.First(p => p.Id == curPlayerId).Name}.");
            Console.WriteLine(outString);
        }

        private static void BeerBarrelResult(Guid curPlayerId, bool? didWork, Card card)
        {
            string outString;
            if (didWork is null)
            {
                Console.Clear();
                outString = $"{_gameManager.Players.First(p => p.Id == curPlayerId).Name} " +
                            $"разыграл карту {_utils.CardNames[CardName.BeerBarrel]}...";
            }
            else
                outString = $"Из колоды получена карта \"{_utils.CardToString(card)}\"\n" +
                            (didWork.Value
                                ? $"Пивная бочка открылась! {_gameManager.Players.First(p => p.Id == curPlayerId).Name} " +
                                  $"вылечил две единицы здоровья."
                                : $"Пивная бочка перешла игроку " +
                                  $"{_gameManager.Players.First(p => p.Id == curPlayerId).Name}.");
            Console.WriteLine(outString);
        }

        private static void JailResult(Guid curPlayerId, bool? didWork, Guid? targetId, Card card)
        {
            Console.WriteLine(didWork is null ? $"{_gameManager.Players.First(p => p.Id == curPlayerId).Name} " +
                                                $"разыграл карту {_utils.CardNames[CardName.Jail]} " +
                                                $"на игрока " +
                                                $"{_gameManager.Players.First(p => p.Id == targetId!.Value).Name}..." : 
                $"Из колоды получена карта \"{_utils.CardToString(card)}\"\n" +
                (didWork.Value
                    ? $"{_gameManager.Players.First(p => p.Id == curPlayerId).Name} пропускает ход в тюрьме."
                    : $"{_gameManager.Players.First(p => p.Id == curPlayerId).Name} не попал в тюрьму."));
        }
        
        private static void HandleCommonCard(Guid curPlayerId, CardName name)
        {
            Console.Clear();
            Console.WriteLine($"{_gameManager.Players.First(p => p.Id == curPlayerId).Name} " +
                              $"разыграл {_utils.CardNames[name]}...");
        }

        private static void HandleWeapon(Guid curPlayerId, CardName name, bool didWork)
        {
            Console.Clear();
            Console.WriteLine(didWork
                ? $"{_gameManager.Players.First(p => p.Id == curPlayerId).Name} " +
                  $"разыграл оружие {_utils.CardNames[name]}."
                : $"{_gameManager.Players.First(p => p.Id == curPlayerId).Name} " +
                  $"поменял оружие на {_utils.CardNames[name]}.");
        }
    }

    private static void EnterToContinue(bool isClear = false)
    {
        Console.WriteLine("Нажмите Enter, чтобы продолжить...");
        Console.ReadLine();
        if (!isClear)
            return;
        Console.Clear();
        Console.WriteLine("\x1b[3J");
        Console.Clear();
    }

    private static void CreateGameManager()
    {
        Console.Clear();
        Console.OutputEncoding = Encoding.UTF8;
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var path = Path.Combine(appData, "Bang!");
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var config = new ConfigurationBuilder()
            .SetBasePath(path)
            .AddJsonFile("config.json")
            .Build();
        var logger = new LoggerConfiguration()
            .WriteTo.File(Path.Combine(path, "log.txt"))
            .CreateLogger();
        var serviceProvider = new ServiceCollection()
            .AddSingleton<IConfiguration>(config)
            .AddSingleton<ILogger>(logger)
            .AddTransient<ICardRepository, CardRepository>()
            .AddTransient<ISaveRepository, SaveRepository>()
            .AddTransient<IGameView, GameView>()
            .AddSingleton<IGameManager, GameManager>()
            .BuildServiceProvider();

        _gameManager = serviceProvider.GetService<IGameManager>()!;
    }

    private static bool GameLoad()
    {
        CreateGameManager();
        var saves = _gameManager.GetAllSaves.ToList();

        if (saves.Count == 0)
        {
            Console.WriteLine("Нет сохранений.");
            EnterToContinue();
            return false;
        }
        
        Console.WriteLine("Доступные сохранения:");
        for (var i = 0; i < saves.Count; i++)
            Console.WriteLine($"{i + 1}. {DateTimeOffset.FromUnixTimeSeconds(saves[i].Value).ToLocalTime()}");
        Console.WriteLine("Введите сохранение для загрузки:");
        if (!int.TryParse(Console.ReadLine(), NumberStyles.Integer,
                CultureInfo.InvariantCulture, out var chosen) || chosen < 1 || chosen > saves.Count)
        {
            Console.Clear();
            Console.WriteLine("Ошибка при вводе.");
            EnterToContinue();
            return false;
        }
        var stateId = saves[chosen - 1].Key;
        _gameManager.LoadState(stateId);
        return true;
    }

    private static void GameInit()
    {
        CreateGameManager();
        _gameManager.GameInit(_playerNames);
        _gameManager.GameStart();

        Console.WriteLine($"Шериф: {_gameManager.CurPlayer.Name}");
        EnterToContinue(true);
        for (var i = 0; i < _gameManager.Players.Count; ++i)
        {
            Console.WriteLine(_utils.PlayerToString(_gameManager.Players[i], true));
            EnterToContinue(true);
            if (i == _gameManager.Players.Count - 1)
                continue;
            Console.WriteLine($"Следующий игрок: {_gameManager.Players[i + 1].Name}");
            EnterToContinue(true);
        }
    }

    private static EGameLoopMenu GameLoopStart()
    {
        Console.Clear();
        var player = _gameManager.CurPlayer;
        Console.WriteLine($"Сейчас ходит {player.Name}");
        EnterToContinue(true);
        var role = _utils.RolesToString[player.Role];

        Console.WriteLine($"{player.Name}\n" +
                          $"Роль: " + role + "\n" +
                          $"Здоровье: {player.Health}/{player.MaxHealth}\n" +
                          "1. Разыграть карту.\n" +
                          "2. Сбросить карту.\n" +
                          "3. Вывести всю информацию.\n" +
                          "4. Закончить ход.\n" +
                          "5. Выйти и сохранить\n" +
                          "Введите пункт меню:");
        if (int.TryParse(Console.ReadLine(), NumberStyles.Integer,
                CultureInfo.InvariantCulture, out var menu))
        {
            Console.Clear();
            return (EGameLoopMenu)(menu - 1);
        }
        Console.Clear();
        Console.WriteLine("Введено не число");
        EnterToContinue();
        return EGameLoopMenu.Error;
    }

    private static void EndTurn()
    {
        Console.WriteLine("Ход закончен. Мёртвые игроки:");
        var i = 0;
        foreach (var p in _gameManager.DeadPlayers)
            Console.WriteLine($"  {++i}. {_utils.PlayerToString(p, false)}");
        EnterToContinue();
    }

    private static bool CardRcCheck(CardRc rc, bool isEndTurn)
    {
        var running = true;
        var player = _gameManager.CurPlayer;
        switch (rc)
        {
            case CardRc.CantPlay:
                Console.Clear();
                Console.WriteLine("Не удалось разыграть карту.");
                EnterToContinue();
                break;
            case CardRc.TooFar:
                Console.Clear();
                Console.WriteLine("Цель слишком далеко.");
                EnterToContinue();
                break;
            case CardRc.OutlawWin:
                Console.WriteLine("Бандиты выиграли.");
                EnterToContinue();
                running = false;
                break;
            case CardRc.RenegadeWin:
                Console.WriteLine("Ренегат выиграл.");
                EnterToContinue();
                running = false;
                break;
            case CardRc.SheriffWin:
                Console.WriteLine("Шериф и его помощники выиграли.");
                EnterToContinue();
                running = false;
                break;
            case CardRc.Ok:
                if (!isEndTurn)
                    EnterToContinue(true);
                else
                    EndTurn();
                break;
            case CardRc.CantEndTurn:
                Console.WriteLine($"В руке слишком много карт сбросьте или " + 
                                  $"разыграйте ещё {player.CardsInHand.Count - player.Health} карт.");
                EnterToContinue();
                break;
            default:
                Console.WriteLine("Неизвестный результат разыгрывания карты");
                break;
        }
        return running;
    }

    private static Guid ChooseCardFromHand()
    {
        var player = _gameManager.CurPlayer;
        Console.WriteLine("Карты в руке:");
        for (var i = 0; i < player.CardsInHand.Count; i++)
            Console.WriteLine($"{i + 1}. {_utils.CardToString(player.CardsInHand[i])}");
        Console.WriteLine("Выберите карту:");
        if (!int.TryParse(Console.ReadLine(), NumberStyles.Integer,
                CultureInfo.InvariantCulture, out var cardIndex))
        {
            Console.Clear();
            Console.WriteLine("Введено не число");
            EnterToContinue();
            return Guid.Empty;
        }
        
        if (cardIndex > 0 && cardIndex <= player.CardsInHand.Count)
            return player.CardsInHand[cardIndex - 1].Id;
        Console.Clear();
        Console.WriteLine("Неверный индекс карты");
        EnterToContinue();
        return Guid.Empty;
    }

    private static void ShowInfo()
    {
        if (_gameManager.TopDiscardedCard is not null)
            Console.WriteLine($"Последняя сброшенная карта: {_utils.CardToString(_gameManager.TopDiscardedCard!)}");
        for (var i = 0; i < _gameManager.Players.Count; i++)
            Console.WriteLine($"{i + 1}) " + _utils.PlayerToString(_gameManager.Players[i],
                _gameManager.Players[i].Id == _gameManager.CurPlayer.Id));
        EnterToContinue(true);
    }

    private static async Task<bool> PlayOrDiscard(EGameLoopMenu menu)
    {
        var running = true;
        var cardId = ChooseCardFromHand();
        if (cardId == Guid.Empty)
            return running;
        if (menu == EGameLoopMenu.PlayCard)
        {
            CardRc rc;
            try
            {
                rc = await _gameManager.PlayCard(cardId);
            }
            catch (NotExistingGuidException)
            {
                Console.Clear();
                Console.WriteLine("Неверный id карты/игрока");
                EnterToContinue();
                return running;
            }
            running = CardRcCheck(rc, false);
        }
        else
        {
            try
            {
                _gameManager.DiscardCard(cardId);
            }
            catch (NotExistingGuidException)
            {
                Console.Clear();
                Console.WriteLine("Неверный id карты");
                EnterToContinue();
                return running;
            }
        }
        if (running)
            Console.Clear();
        return running;
    }

    private static async Task<bool> GameLoopMenu()
    {
        var running = true;
        var menu = GameLoopStart();
        switch (menu)
        {
            case EGameLoopMenu.PlayCard:
            case EGameLoopMenu.DiscardCard:
                running = await PlayOrDiscard(menu);
                break;
            case EGameLoopMenu.ShowInfo:
                ShowInfo();
                break;
            case EGameLoopMenu.EndTurn:
                var check = await _gameManager.EndTurn();
                running = CardRcCheck(check, true);
                break;
            case EGameLoopMenu.SaveAndQuit:
                Console.Clear();
                _gameManager.SaveState();
                Console.WriteLine("Игра сохранена!\n" +
                                  "Вы хотите выйти?(д/н)");
                var input = Console.ReadLine() ?? string.Empty;
                if (input == "д")
                    running = false;
                Console.WriteLine("Выход из игры...");
                break;
            case EGameLoopMenu.Error:
                Console.Clear();
                Console.WriteLine("Введено не число");
                EnterToContinue();
                break;
            default:
                Console.Clear();
                Console.WriteLine("Неизвестная ошибка");
                EnterToContinue();
                break;
        }
        return running;
    }

    private static async Task GameLoop(bool isLoad)
    {
        var running = true;
        _utils = new Utils();
        if (isLoad)
        {
            if (!GameLoad())
                return;
        }
        else
            GameInit();

        while (running)
            running = await GameLoopMenu();
    }

    private static EnterPlayerNamesRc EnterPlayerNames()
    {
        Console.Clear();
        _playerNames = [];
        Console.WriteLine("Введите количество игроков в игре(от 4 до 7):");
        if (int.TryParse(Console.ReadLine(), NumberStyles.Integer,
                CultureInfo.InvariantCulture, out var playerCount) && playerCount >= GameManager.MinPlayersCount
                && playerCount <= GameManager.MaxPlayersCount)
        {
            for (var i = 0; i < playerCount; ++i)
            {
                Console.WriteLine($"Введите имя {i + 1}-го игрока:");
                var name = Console.ReadLine();
                if (name is null)
                    return EnterPlayerNamesRc.NameError;
                name = name.Trim();
                if (name == string.Empty)
                    return EnterPlayerNamesRc.EmptyNameError;

                if (_playerNames.Contains(name))
                    return EnterPlayerNamesRc.SameNameError;
                _playerNames.Add(name);
            }
        }
        else
            return EnterPlayerNamesRc.AmountError;

        return EnterPlayerNamesRc.Ok;
    }

    private static async Task GameStart()
    {
        switch (EnterPlayerNames())
        {
            case EnterPlayerNamesRc.Ok:
                await GameLoop(false);
                break;
            case EnterPlayerNamesRc.NameError:
                Console.Clear();
                Console.WriteLine("Ошибка при вводе имён игроков");
                EnterToContinue();
                break;
            case EnterPlayerNamesRc.AmountError:
                Console.Clear();
                Console.WriteLine("Ошибка при вводе количества игроков");
                EnterToContinue();
                break;
            case EnterPlayerNamesRc.EmptyNameError:
                Console.Clear();
                Console.WriteLine("Ошибка. Пустое имя игрока");
                EnterToContinue();
                break;
            case EnterPlayerNamesRc.SameNameError:
                Console.Clear();
                Console.WriteLine("Ошибка. У игроков одинаковые имена");
                EnterToContinue();
                break;
            default:
                Console.Clear();
                Console.WriteLine("Неизвестная ошибка");
                EnterToContinue();
                break;
        }
    }

    private static async Task MainMenu()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("Бэнг!\n" +
                              "1. Начать игру\n" +
                              "2. Загрузить игру\n" +
                              "3. Выход\n" +
                              "Введите пункт меню:");
            if (int.TryParse(Console.ReadLine(), NumberStyles.Integer,
                    CultureInfo.InvariantCulture, out var menu))
            {
                switch ((EGameMenu)(menu - 1))
                {
                    case EGameMenu.Start:
                        await GameStart();
                        break;
                    case EGameMenu.Load:
                        await GameLoop(true);
                        break;
                    case EGameMenu.Exit:
                        return;
                    default:
                        Console.Clear();
                        Console.WriteLine("Неизвестный пункт меню.");
                        EnterToContinue();
                        break;
                }
            }
            else
            {
                Console.Clear();
                Console.WriteLine("Введено не число.");
                EnterToContinue();
            }
        }
    }

    public static async Task<int> Main()
    {
        await MainMenu();
        return 0;
    }
}
