namespace BLComponent.OutputPort;

public interface IGameManager
{
    private const int MinPlayersCountConst = 4;
    private const int MaxPlayersCountConst = 7;
    public static int MinPlayersCount => MinPlayersCountConst;
    public static int MaxPlayersCount => MaxPlayersCountConst;
    
    public void GameInit(IEnumerable<Guid> playerIds);
    public CardRc PlayCard(Guid cardId);
    public void DiscardCard(Guid cardId);
    public CardRc EndTurn();
    public Player CurPlayer { get; }
    public IReadOnlyList<Player> Players { get; }
    public IReadOnlyList<Player> DeadPlayers { get; }
    public Card? TopDiscardedCard { get; }
    public int GetRange(Guid playerId, Guid targetId);
}
