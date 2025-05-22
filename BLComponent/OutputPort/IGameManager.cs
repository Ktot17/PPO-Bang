namespace BLComponent.OutputPort;

public interface IGameManager
{
    public void GameInit(IEnumerable<string> playerNames);
    public void GameStart();
    public Task<CardRc> PlayCard(Guid cardId);
    public void DiscardCard(Guid cardId);
    public Task<CardRc> EndTurn();
    public Player CurPlayer { get; }
    public IReadOnlyList<Player> Players { get; }
    public IReadOnlyList<Player> DeadPlayers { get; }
    public Card? TopDiscardedCard { get; }
    public IReadOnlyList<Card> CardsInDeck { get; }
    public int GetRange(Guid playerId, Guid targetId);
    public void SaveState();
    public void LoadState(int stateId);
    public Dictionary<int, long> GetAllSaves { get; }
    public IList<Card> GetAllCards { get; }
}
