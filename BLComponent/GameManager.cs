using BLComponent.InputPorts;
using BLComponent.OutputPort;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("BLUnitTests")]
namespace BLComponent;

public interface IGet
{
    public Guid GetPlayerId(IReadOnlyList<Player> players, Guid currentPlayerId);
    public Guid GetCardId(IReadOnlyList<Card?> cards, int unknownCardsCount, Guid? playerId = null);
}

internal class GameState(List<Player> players, Deck deck, Guid currentPlayerId, IGet get)
{
    private int _currentPlayerIndex;
    public List<Player> Players { get; } = players;
    public Deck CardDeck { get; } = deck;
    public Guid CurrentPlayerId { get; private set; } = currentPlayerId;
    public Player CurrentPlayer => Players.First(p => p.Id == CurrentPlayerId);
    public IGet Get { get; } = get;
    public List<Player> LivePlayers => [.. Players.Where(p => !p.IsDead)];
    public IReadOnlyList<Player> DeadPlayers => [.. Players.Where(p => p.IsDead)];

    public int GetRange(Guid playerId, Guid targetId)
    {
        var playerIndex = Players.FindIndex(p => p.Id == playerId);
        var targetIndex = Players.FindIndex(p => p.Id == targetId);
        var add = Players[targetIndex].CardsOnBoard.Any(c => c.Name == CardName.Mustang) ? 1 : 0;
        var sub = Players[playerIndex].CardsOnBoard.Any(c => c.Name == CardName.Scope) ? 1 : 0;
        return int.Min(Players.Count - int.Abs(playerIndex - targetIndex), int.Abs(playerIndex - targetIndex)) + add - sub;
    }

    public Player GetNextPlayer()
    {
        var index = _currentPlayerIndex;
        do
        {
            index = (index + 1) % Players.Count;
        } while (Players[index].IsDead);
        return Players[index];
    }

    public void NextPlayer()
    {
        do
        {
            _currentPlayerIndex = (_currentPlayerIndex + 1) % Players.Count;
        } while (Players[_currentPlayerIndex].IsDead);
        CurrentPlayerId = Players[_currentPlayerIndex].Id;
    }

    public void FixIndex() => _currentPlayerIndex = Players.FindIndex(p => p.Id == CurrentPlayerId);
}

public sealed class GameManager(ICardRepository cardRepository, IGet get) : IGameManager
{
    private GameState _gameState = null!;
    private readonly Random _random = new();
    private readonly List<PlayerRole> _roles =
        [PlayerRole.Renegade, PlayerRole.Outlaw,
            PlayerRole.Outlaw, PlayerRole.DeputySheriff,
            PlayerRole.Outlaw, PlayerRole.DeputySheriff];

    public IReadOnlyList<Player> Players => _gameState.Players;
    public Player CurPlayer => _gameState.CurrentPlayer;
    public Card? TopDiscardedCard => _gameState.CardDeck.TopDiscardedCard;
    public List<Player> LivePlayers => _gameState.LivePlayers;
    public IReadOnlyList<Player> DeadPlayers => _gameState.DeadPlayers;

    public void GameInit(IEnumerable<Guid> playerIds)
    {
        var enumerable = playerIds.ToList();
        if (enumerable.Count is < 4 or > 7)
            throw new WrongNumberOfPlayersException(enumerable.Count);
        if (enumerable.Distinct().Count() != enumerable.Count)
            throw new NotUniqueIdsException();
        var n = enumerable.Count;
        while (n > 1)
        {
            --n;
            var k = _random.Next(n + 1);
            (enumerable[n], enumerable[k]) = (enumerable[k], enumerable[n]);
        }
        var players = new List<Player> { new(enumerable[0], PlayerRole.Sheriff, 5) };
        for (var i = 1; i < enumerable.Count; ++i)
        {
            var k = _random.Next(enumerable.Count - i);
            players.Add(new Player(enumerable[i], _roles[k], 4));
            _roles.RemoveAt(k);
        }
        
        _gameState = new GameState(players, new Deck(cardRepository), players[0].Id, get);

        foreach (var player in _gameState.Players)
            for (var i = 0; i < player.MaxHealth; ++i)
                player.AddCardInHand(_gameState.CardDeck.Draw());
        CurPlayer.AddCardInHand(_gameState.CardDeck.Draw());
        CurPlayer.AddCardInHand(_gameState.CardDeck.Draw());
    }

    private void PlayerClear(Guid playerId)
    {
        var player = Players.First(p => p.Id == playerId);
        var cardsInHandCount = player.CardsInHand.Count;
        var cardsOnBoardCount = player.CardsOnBoard.Count;
        for (var i = 0; i < cardsInHandCount; ++i)
            _gameState.CardDeck.Discard(player.RemoveCard(player.CardsInHand[0].Id));
        for (var i = 0; i < cardsOnBoardCount; ++i)
            _gameState.CardDeck.Discard(player.RemoveCard(player.CardsOnBoard[0].Id));
        if (player.Weapon != null)
            _gameState.CardDeck.Discard(player.RemoveCard(player.Weapon.Id));
    }

    public CardRc PlayCard(Guid cardId)
    {
        Card? curCard;
        try
        {
            curCard = CurPlayer.CardsInHand.First(c => c.Id == cardId);
        }
        catch (InvalidOperationException)
        {
            throw new NotExistingGuidException();
        }
        var isIndians = curCard.Name is CardName.Indians;
        var rc = curCard.Play(_gameState);
        if (rc is not CardRc.Ok) return rc;
        DiscardCard(curCard.Id);
        if (isIndians)
            return CheckEndGame();
        foreach (var player in DeadPlayers.Where(p => p.IsDeadOnThisTurn))
            switch (player.Role)
            {
                case PlayerRole.Outlaw:
                    for (var i = 0; i < 3; ++i)
                        CurPlayer.AddCardInHand(_gameState.CardDeck.Draw());
                    break;
                case PlayerRole.DeputySheriff when CurPlayer.Role is PlayerRole.Sheriff:
                    PlayerClear(CurPlayer.Id);
                    break;
                case PlayerRole.Renegade:
                case PlayerRole.Sheriff:
                default:
                    break;
            }
        return CheckEndGame();
    }

    public void DiscardCard(Guid cardId) => _gameState.CardDeck.Discard(CurPlayer.RemoveCard(cardId));

    public CardRc EndTurn()
    {
        var i = 0;
        while (true)
        {
            if (i == 0 && CurPlayer.CardsInHand.Count > CurPlayer.Health)
                return CardRc.CantEndTurn;
            ++i;
            CurPlayer.EndTurn();
            _gameState.NextPlayer();
            Card? card;
            if ((card = CurPlayer.CardsOnBoard.FirstOrDefault(c => c.Name == CardName.Dynamite)) is not null)
                ((Dynamite)card).ApplyEffect(_gameState);

            if ((card = CurPlayer.CardsOnBoard.FirstOrDefault(c => c.Name == CardName.BeerBarrel)) is not null)
                ((BeerBarrel)card).ApplyEffect(_gameState);

            if (CurPlayer.IsDead)
            {
                var rc = CheckEndGame();
                if (rc is not CardRc.Ok) return rc;
                continue;
            }
            
            if ((card = CurPlayer.CardsOnBoard.FirstOrDefault(c => c.Name == CardName.Jail)) is not null && 
                ((Jail)card).ApplyEffect(_gameState))
                continue;

            CurPlayer.AddCardInHand(_gameState.CardDeck.Draw());
            CurPlayer.AddCardInHand(_gameState.CardDeck.Draw());
            return CardRc.Ok;
        }
    }

    internal CardRc CheckEndGame()
    {
        if (Players[0].IsDead)
            return Players.Any(p => p.Role != PlayerRole.Renegade && !p.IsDead) ? 
                CardRc.OutlawWin : CardRc.RenegadeWin;

        if (!Players.Any(p => p is { Role: PlayerRole.Outlaw, IsDead: false }) &&
            !Players.Any(p => p is { Role: PlayerRole.Renegade, IsDead: false }))
            return CardRc.SheriffWin;

        foreach (var player in DeadPlayers.Where(p => p.IsDeadOnThisTurn))
        {
            PlayerClear(player.Id);
            player.DeadEarlier();
        }
        
        _gameState.FixIndex();

        return CardRc.Ok;
    }

    public int GetRange(Guid playerId, Guid targetId) => _gameState.GetRange(playerId, targetId);
    
    internal void ForUnitTestWithDynamiteAndBeerBarrel() => _gameState.CardDeck.ForUnitTestWithDynamiteAndBeerBarrel();
}