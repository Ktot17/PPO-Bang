using BLComponent.InputPorts;
using BLComponent.OutputPort;
using Newtonsoft.Json;
using Serilog;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("BLUnitTests")]
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("DBUnitTests")]
namespace BLComponent;

public interface IGameView
{
    public Task<Guid> GetPlayerIdAsync(IReadOnlyList<Player> players, Guid currentPlayerId);
#if NET8_0
    public Task<Guid> GetCardIdAsync(IReadOnlyList<Card?> cards, int unknownCardsCount) => 
        GetCardIdAsync(cards, unknownCardsCount, null);
#else
    public Task<Guid> GetCardIdAsync(IReadOnlyList<Card?> cards, int unknownCardsCount);
#endif
    public Task<Guid> GetCardIdAsync(IReadOnlyList<Card?> cards, int unknownCardsCount, Guid? playerId);
    public Task<bool> YesOrNoAsync(Guid playerId, CardName name);
#if NET8_0
    public void ShowCardResult(Guid curPlayerId, CardName name, bool? didWork, Guid? targetId, Card? card);
    public void ShowCardResult(Guid curPlayerId, CardName name) => 
        ShowCardResult(curPlayerId, name, null, null, null);
    public void ShowCardResult(Guid curPlayerId, CardName name, Guid targetId) => 
        ShowCardResult(curPlayerId, name, null, targetId, null);
    public void ShowCardResult(Guid curPlayerId, CardName name, bool didWork) => 
        ShowCardResult(curPlayerId, name, didWork, null, null);
    public void ShowCardResult(Guid curPlayerId, CardName name, bool didWork, Guid target) => 
        ShowCardResult(curPlayerId, name, didWork, target, null);
    public void ShowCardResult(Guid curPlayerId, CardName name, bool didWork, Card card) => 
        ShowCardResult(curPlayerId, name, didWork, null, card);
    public void ShowCardResult(Guid curPlayerId, CardName name, Guid targetId, Card card) => 
        ShowCardResult(curPlayerId, name, null, targetId, card);
#else
    public void ShowCardResult(Guid curPlayerId, CardName name);
    public void ShowCardResult(Guid curPlayerId, CardName name, Guid targetId);
    public void ShowCardResult(Guid curPlayerId, CardName name, bool didWork);
    public void ShowCardResult(Guid curPlayerId, CardName name, bool didWork, Guid target);
    public void ShowCardResult(Guid curPlayerId, CardName name, bool didWork, Card card);
    public void ShowCardResult(Guid curPlayerId, CardName name, Guid targetId, Card card);
#endif
    public void CardAddedInHand(Guid cardId, Guid playerId);
    public void CardAddedOnBoard(Guid cardId, Guid playerId);
    public void WeaponAdded(Guid cardId, Guid playerId);
    public void CardDiscarded(Guid cardId);
    public void CardReturnedToDeck(Guid cardId);
}

public record GameStateDto
{
    public GameStateDto() {}
    
    internal GameStateDto(GameState state)
    {
        Players = state.Players.Select(p => new PlayerDto(p)).ToList();
        CardDeck = new DeckDto(state.CardDeck);
        CurrentPlayerId = state.CurrentPlayerId;
    }

    [JsonProperty]
    public IReadOnlyList<PlayerDto> Players { get; private set; } = [];
    [JsonProperty]
    public DeckDto CardDeck { get; private set; } = new();
    [JsonProperty]
    public Guid CurrentPlayerId { get; private set; }
}

public class GameState
{
    private int _currentPlayerIndex;
    internal List<Player> Players { get; }
    internal Deck CardDeck { get; }
    internal Guid CurrentPlayerId { get; private set; }
    internal Player CurrentPlayer => Players.First(p => p.Id == CurrentPlayerId);
    internal IGameView GameView { get; }
    internal IReadOnlyList<Player> LivePlayers => [.. Players.Where(p => !p.IsDead)];
    internal IReadOnlyList<Player> DeadPlayers => [.. Players.Where(p => p.IsDead)];

    internal GameState(IReadOnlyList<Player> players, Deck deck, Guid currentPlayerId, IGameView gameView)
    {
        _currentPlayerIndex = players.ToList().FindIndex(p => p.Id == currentPlayerId);
        Players = [.. players];
        CardDeck = deck;
        CurrentPlayerId = currentPlayerId;
        GameView = gameView;
    }
    
    internal GameState(GameStateDto dto, IGameView gameView)
    {
        Players = [];
        foreach (var playerDto in dto.Players)
            Players.Add(new Player(playerDto));
        CardDeck = new Deck(dto.CardDeck);
        CurrentPlayerId = dto.CurrentPlayerId;
        GameView = gameView;
    }

    internal int GetRange(Guid playerId, Guid targetId)
    {
        var playerIndex = LivePlayers.ToList().FindIndex(p => p.Id == playerId);
        var targetIndex = LivePlayers.ToList().FindIndex(p => p.Id == targetId);
        var add = LivePlayers[targetIndex].CardsOnBoard.Any(c => c.Name == CardName.Mustang) ? 1 : 0;
        var sub = LivePlayers[playerIndex].CardsOnBoard.Any(c => c.Name == CardName.Scope) ? 1 : 0;
#if NET8_0
        return int.Min(LivePlayers.Count - int.Abs(playerIndex - targetIndex), int.Abs(playerIndex - targetIndex)) + add - sub;
#else
        return Math.Min(LivePlayers.Count - Math.Abs(playerIndex - targetIndex), 
            Math.Abs(playerIndex - targetIndex)) + add - sub;
#endif
    }

    internal Player GetNextPlayer()
    {
        var index = _currentPlayerIndex;
        do
        {
            index = (index + 1) % Players.Count;
        } while (Players[index].IsDead);
        return Players[index];
    }

    internal void NextPlayer()
    {
        do
        {
            _currentPlayerIndex = (_currentPlayerIndex + 1) % Players.Count;
        } while (Players[_currentPlayerIndex].IsDead);
        CurrentPlayerId = Players[_currentPlayerIndex].Id;
    }
}

public class GameManager(ICardRepository cardRepository, ISaveRepository saveRepository,
    IGameView gameView, ILogger logger)
    : IGameManager
{
    private const int MinPlayersCountConst = 4;
    private const int MaxPlayersCountConst = 7;
    private const int OutlawRewardCardCount = 3;
    
    protected GameState GameState = null!;
    private readonly Random _random = new();
    private readonly List<PlayerRole> _roles =
        [PlayerRole.Renegade, PlayerRole.Outlaw,
            PlayerRole.Outlaw, PlayerRole.DeputySheriff,
            PlayerRole.Outlaw, PlayerRole.DeputySheriff];

    public static int MinPlayersCount => MinPlayersCountConst;
    public static int MaxPlayersCount => MaxPlayersCountConst;
    public IReadOnlyList<Player> Players => GameState.Players;
    public Player CurPlayer => GameState.CurrentPlayer;
    public Card? TopDiscardedCard => GameState.CardDeck.TopDiscardedCard;
    public IReadOnlyList<Player> LivePlayers => GameState.LivePlayers;
    public IReadOnlyList<Player> DeadPlayers => GameState.DeadPlayers;
    public IReadOnlyList<Card> CardsInDeck => GameState.CardDeck.DrawPile;

    public void GameInit(IEnumerable<string> playerNames)
    {
        List<string> enumerable = [.. playerNames];
        logger.Information("Инициализация игры.");
        if (enumerable.Count is < MinPlayersCountConst or > MaxPlayersCountConst)
            throw new WrongNumberOfPlayersException(enumerable.Count);
        if (enumerable.Distinct().Count() != enumerable.Count)
            throw new NotUniqueNamesException();
        var n = enumerable.Count;
        while (n > 1)
        {
            --n;
            var k = _random.Next(n + 1);
            (enumerable[n], enumerable[k]) = (enumerable[k], enumerable[n]);
        }
        var players = new List<Player> { new(Guid.NewGuid(), enumerable[0], PlayerRole.Sheriff, 5) };
        logger.Information($"Игрок {enumerable[0]}. Роль {PlayerRole.Sheriff}.");
        for (var i = 1; i < enumerable.Count; ++i)
        {
            var k = _random.Next(enumerable.Count - i);
            players.Add(new Player(Guid.NewGuid(), enumerable[i], _roles[k], 4));
            logger.Information($"Игрок {enumerable[i]}. Роль {_roles[k]}.");
            _roles.RemoveAt(k);
        }

        try
        {
            GameState = new GameState(players, new Deck(cardRepository), players[0].Id, gameView);
        }
        catch (WrongConnectionStringException)
        {
            logger.Fatal("Неправильная строка подключения к базе данных карт.");
            throw;
        }
    }

    public void GameStart()
    {
        foreach (var player in GameState.Players)
            for (var i = 0; i < player.MaxHealth; ++i)
                player.AddCardInHand(GameState.CardDeck.Draw(GameState.GameView), GameState.GameView);
        CurPlayer.AddCardInHand(GameState.CardDeck.Draw(GameState.GameView), GameState.GameView);
        CurPlayer.AddCardInHand(GameState.CardDeck.Draw(GameState.GameView), GameState.GameView);
    }

    private void PlayerClear(Guid playerId)
    {
        var player = Players.First(p => p.Id == playerId);
        var cardsInHandCount = player.CardsInHand.Count;
        var cardsOnBoardCount = player.CardsOnBoard.Count;
        for (var i = 0; i < cardsInHandCount; ++i)
            GameState.CardDeck.Discard(player.RemoveCard(player.CardsInHand[0].Id), GameState.GameView);
        for (var i = 0; i < cardsOnBoardCount; ++i)
            GameState.CardDeck.Discard(player.RemoveCard(player.CardsOnBoard[0].Id), GameState.GameView);
        if (player.Weapon is not null)
            GameState.CardDeck.Discard(player.RemoveCard(player.Weapon.Id), GameState.GameView);
    }

    public async Task<CardRc> PlayCard(Guid cardId)
    {
        var curCard = CurPlayer.CardsInHand.FirstOrDefault(c => c.Id == cardId);
        if (curCard is null)
        {
            logger.Error($"Игрок {CurPlayer.Name} разыграл несуществующую карту.");
            throw new NotExistingGuidException();
        }
        logger.Information($"Игрок {CurPlayer.Name} разыграл карту {curCard.Name}.");
        var isIndians = curCard.Name is CardName.Indians;
        var rc = await curCard.Play(GameState);
        if (rc is not CardRc.Ok)
        {
            logger.Information($"Игрок {CurPlayer.Name} не смог разыграть карту {curCard.Name}.({rc})");
            return rc;
        }
        if (curCard.Type is CardType.Instant)
            DiscardCard(curCard.Id);
        else
            CurPlayer.RemoveCard(curCard.Id);
        if (isIndians)
            return await CheckEndGame();
        foreach (var playerRole in DeadPlayers.Where(p => p.IsDeadOnThisTurn).Select(p => p.Role))
            switch (playerRole)
            {
                case PlayerRole.Outlaw:
                    for (var i = 0; i < OutlawRewardCardCount; ++i)
                        CurPlayer.AddCardInHand(GameState.CardDeck.Draw(GameState.GameView), GameState.GameView);
                    break;
                case PlayerRole.DeputySheriff when CurPlayer.Role is PlayerRole.Sheriff:
                    PlayerClear(CurPlayer.Id);
                    break;
                case PlayerRole.Renegade:
                case PlayerRole.Sheriff:
                case PlayerRole.DeputySheriff:
                    break;
                default:
                    logger.Fatal("Несуществующая роль.");
                    throw new NotExistingRoleException();
            }
        return await CheckEndGame();
    }

    public void DiscardCard(Guid cardId)
    {
        try
        {
            GameState.CardDeck.Discard(CurPlayer.RemoveCard(cardId), GameState.GameView);
        }
        catch (NotExistingGuidException)
        {
            logger.Error($"Игрок {CurPlayer.Name} сбросил несуществующую карту.");
            throw;
        }
        logger.Information($"Игрок {CurPlayer.Name} сбросил карту.");
    }

    public async Task<CardRc> EndTurn()
    {
        var isFirstIteration = true;
        while (true)
        {
            if (isFirstIteration && CurPlayer.CardsInHand.Count > CurPlayer.Health)
            {
                logger.Information($"Игрок {CurPlayer.Name} не смог закончить ход.");
                return CardRc.CantEndTurn;
            }
            logger.Information($"Игрок {CurPlayer.Name} закончил ход.");
            isFirstIteration = false;
            CurPlayer.EndTurn();
            GameState.NextPlayer();
            Card? card;
            if ((card = CurPlayer.CardsOnBoard.FirstOrDefault(c => c.Name == CardName.Dynamite)) is not null)
            {
                logger.Information($"У игрока {CurPlayer.Name} взорвался динамит.");
                await ((Dynamite)card).ApplyEffect(GameState);
            }

            if ((card = CurPlayer.CardsOnBoard.FirstOrDefault(c => c.Name == CardName.BeerBarrel)) is not null)
            {
                logger.Information($"У игрока {CurPlayer.Name} открылась бочка с пивом.");
                ((BeerBarrel)card).ApplyEffect(GameState);
            }

            var rc = await CheckEndGame();
            if (rc is not CardRc.Ok)
                return rc;
            if (CurPlayer.IsDead)
                continue;

            if ((card = CurPlayer.CardsOnBoard.FirstOrDefault(c => c.Name == CardName.Jail)) is not null &&
                ((Jail)card).ApplyEffect(GameState))
            {
                logger.Information($"Игрока {CurPlayer.Name} пропускает ход в тюрьме.");
                continue;
            }

            CurPlayer.AddCardInHand(GameState.CardDeck.Draw(GameState.GameView), GameState.GameView);
            CurPlayer.AddCardInHand(GameState.CardDeck.Draw(GameState.GameView), GameState.GameView);
            return CardRc.Ok;
        }
    }

    internal async Task<CardRc> CheckEndGame()
    {
        if (Players[0].IsDead)
        {
            var rc = Players.Any(p => p.Role != PlayerRole.Renegade && !p.IsDead) ? CardRc.OutlawWin : CardRc.RenegadeWin;
            logger.Information("Игра закончилась победой " + (rc is CardRc.OutlawWin ? "бандитов." : "ренегата."));
            return rc;
        }

        if (!Players.Any(p => p is { Role: PlayerRole.Outlaw, IsDead: false }) &&
            !Players.Any(p => p is { Role: PlayerRole.Renegade, IsDead: false }))
        {
            logger.Information("Игра закончилась победой шерифа.");
            return CardRc.SheriffWin;
        }

        foreach (var player in DeadPlayers.Where(p => p.IsDeadOnThisTurn))
        {
            PlayerClear(player.Id);
            player.DeadEarlier();
        }

        if (CurPlayer.IsDead)
            await EndTurn();

        return CardRc.Ok;
    }

    public int GetRange(Guid playerId, Guid targetId) => GameState.GetRange(playerId, targetId);
    
    public void SaveState()
    {
        saveRepository.SaveState(new GameStateDto(GameState));
        logger.Information("Игра сохранена.");
    }

    public void LoadState(int stateId)
    {
        var gameState = saveRepository.FindState(stateId);
        GameState = new GameState(gameState, gameView);
        logger.Information("Игра загружена.");
    }

    public Dictionary<int, long> GetAllSaves => saveRepository.GetAll;

    public IList<Card> GetAllCards
    {
        get
        {
            var cards = new List<Card>();
            cards.AddRange(GameState.CardDeck.DrawPile);
            cards.AddRange(GameState.CardDeck.DiscardPile);
            foreach (var player in Players)
            {
                cards.AddRange(player.CardsInHand);
                cards.AddRange(player.CardsOnBoard);
                if (player.Weapon is not null)
                    cards.Add(player.Weapon);
            }

            return cards;
        }
    }
}
