using BLComponent.Cards;
using BLComponent.InputPorts;
using BLComponent.OutputPort;
using Newtonsoft.Json;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("BLUnitTests")]
namespace BLComponent;

public interface IGet
{
    public int GetPlayerIndex(IReadOnlyList<Player> players, int currentPlayer);
    public int GetCardIndex(IReadOnlyList<Card?> cards, int unknownCardsCount, int? playerId = null);
}

public class GameContext(List<Player> players, Deck deck, int curPlayer, IGet get)
{
    public List<Player> Players { get; } = players;
    public Deck CardDeck { get; } = deck;
    public int CurrentPlayer { get; } = curPlayer;
    public IGet Get { get; } = get;

    public int GetRange(int player, int target)
    {
        var add = Players[target].CardsOnBoard.Any(c => c.Name == CardName.Mustang) ? 1 : 0;
        var sub = Players[player].CardsOnBoard.Any(c => c.Name == CardName.Scope) ? 1 : 0;
        return int.Min(Players.Count - int.Abs(player - target), int.Abs(player - target)) + add - sub;
    }
}

[Serializable]
public sealed class GameManager(ICardRepository cardRepository, IGet get) : IGameManager
{
    [JsonProperty]
    private readonly List<Player> _players = [];
    private readonly Deck _cardDeck = new(cardRepository);
    [JsonProperty]
    private int _currentPlayer;
    private int _currentPlayerId;
    private readonly Random _random = new();
    private readonly List<PlayerRole> _roles =
        [PlayerRole.Renegade, PlayerRole.Outlaw,
            PlayerRole.Outlaw, PlayerRole.DeputySheriff,
            PlayerRole.Outlaw, PlayerRole.DeputySheriff];

    public IReadOnlyList<Player> Players => _players;
    public Player CurPlayer => LivePlayers()[_currentPlayer];
    public Card? TopDiscardedCard => _cardDeck.TopDiscardedCard;

    public void GameInit(IEnumerable<int> playerIds)
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
        _players.Add(new Player(enumerable[0], PlayerRole.Sheriff, 5));
        for (var i = 1; i < enumerable.Count; ++i)
        {
            var k = _random.Next(enumerable.Count - i);
            _players.Add(new Player(enumerable[i], _roles[k], 4));
            _roles.RemoveAt(k);
        }

        foreach (var player in _players)
            for (var i = 0; i < player.MaxHealth; ++i)
                player.AddCardInHand(_cardDeck.Draw());
        CurPlayer.AddCardInHand(_cardDeck.Draw());
        CurPlayer.AddCardInHand(_cardDeck.Draw());
    }

    public CardRc PlayCard(int cardIndex)
    {
        _currentPlayerId = CurPlayer.Id;
        var isIndians = CurPlayer.CardsInHand[cardIndex].Name is CardName.Indians;
        var rc = CurPlayer.CardsInHand[cardIndex]
            .Play(new GameContext(LivePlayers(), _cardDeck, _currentPlayer, get));
        if (rc is not CardRc.Ok) return rc;
        DiscardCard(cardIndex);
        if (!isIndians)
            foreach (var player in DeadPlayers().Where(p => p.IsDeadOnThisTurn))
                switch (player.Role)
                {
                    case PlayerRole.Outlaw:
                    {
                        for (var i = 0; i < 3; ++i)
                            CurPlayer.AddCardInHand(_cardDeck.Draw());
                        break;
                    }
                    case PlayerRole.DeputySheriff when CurPlayer.Role is PlayerRole.Sheriff:
                    {
                        var cardCount = CurPlayer.CardCount;
                        for (var i = 0; i < cardCount; ++i)
                            _cardDeck.Discard(CurPlayer.RemoveCard(0));
                        break;
                    }
                    case PlayerRole.Renegade:
                    case PlayerRole.Sheriff:
                    default:
                        break;
                }
        rc = CheckEndGame();
        return rc;
    }

    public void DiscardCard(int cardIndex) => _cardDeck.Discard(CurPlayer.RemoveCard(cardIndex));

    public CardRc EndTurn()
    {
        var i = 0;
        while (true)
        {
            if (i == 0 && CurPlayer.CardsInHand.Count > CurPlayer.Health) return CardRc.CantEndTurn;
            ++i;
            CurPlayer.EndTurn();
            _currentPlayer = (_currentPlayer + 1) % LivePlayers().Count;
            _currentPlayerId = LivePlayers()[(_currentPlayer + 1) % LivePlayers().Count].Id;
            var cur = CurPlayer;
            var cardIndex = cur.CardsOnBoard.ToList().FindIndex(c => c.Name == CardName.Dynamite);
            if (cardIndex != -1)
                ((Dynamite)cur.CardsOnBoard[cardIndex]).ApplyEffect(
                    new GameContext(LivePlayers(), _cardDeck, _currentPlayer, get));

            cardIndex = cur.CardsOnBoard.ToList().FindIndex(c => c.Name == CardName.BeerBarrel);
            if (cardIndex != -1)
                ((BeerBarrel)cur.CardsOnBoard[cardIndex]).ApplyEffect(
                    new GameContext(_players.Where(p => !p.IsDead || p.IsDeadOnThisTurn).ToList(),
                        _cardDeck, _currentPlayer, get));

            if (cur.IsDead)
            {
                var rc = CheckEndGame();
                if (rc is not CardRc.Ok) return rc;
                continue;
            }
            
            cardIndex = cur.CardsOnBoard.ToList().FindIndex(c => c.Name == CardName.Jail);
            if (cardIndex != -1 && ((Jail)cur.CardsOnBoard[cardIndex]).ApplyEffect(
                    new GameContext(LivePlayers(), _cardDeck, _currentPlayer, get)))
                continue;

            cur.AddCardInHand(_cardDeck.Draw());
            cur.AddCardInHand(_cardDeck.Draw());
            return CardRc.Ok;
        }
    }
    
    
    public List<Player> LivePlayers() => _players.Where(p => !p.IsDead).ToList();

    public IReadOnlyList<Player> DeadPlayers() => _players.Where(p => p.IsDead).ToList();

    public CardRc CheckEndGame()
    {
        if (_players[0].IsDead)
            return _players.Any(p => p.Role != PlayerRole.Renegade && !p.IsDead) ? 
                CardRc.OutlawWin : CardRc.RenegadeWin;

        if (!_players.Any(p => p is { Role: PlayerRole.Outlaw, IsDead: false }) &&
            !_players.Any(p => p is { Role: PlayerRole.Renegade, IsDead: false }))
            return CardRc.SheriffWin;

        foreach (var player in DeadPlayers().Where(p => p.IsDeadOnThisTurn))
        {
            var cardCount = player.CardCount;
            for (var i = 0; i < cardCount; ++i)
                _cardDeck.Discard(player.RemoveCard(0));
            player.DeadEarlier();
        }
        
        _currentPlayer = LivePlayers().FindIndex(p => p.Id == _currentPlayerId);

        return CardRc.Ok;
    }
    
    internal void ForUnitTestWithDynamiteAndBeerBarrel()
    {
        _cardDeck.ForUnitTestWithDynamiteAndBeerBarrel();
    }
}