namespace BLComponent.OutputPort;

public interface IGameManager
{
    public void GameInit(IEnumerable<int> playerIds);
    public CardRc PlayCard(int cardIndex);
    public void DiscardCard(int cardIndex);
    public CardRc EndTurn();
    public CardRc CheckEndGame();
}