namespace BLComponent.OutputPort;

public interface IGameManager
{
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