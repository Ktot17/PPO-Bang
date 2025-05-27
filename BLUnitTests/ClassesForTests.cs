using BLComponent;
using BLComponent.InputPorts;
using Serilog;

namespace BLUnitTests;

internal sealed class DeckForUnitTest : Deck
{
    internal DeckForUnitTest()
    {
        DrawPileP.Push(CardFactory.CreateCard(CardName.Bang, CardSuit.Clubs, CardRank.Seven));
        DrawPileP.Push(CardFactory.CreateCard(CardName.Bang, CardSuit.Spades, CardRank.Seven));
    }
}

internal sealed class GameManagerForUnitTest(ICardRepository cardRepository, 
    ISaveRepository saveRepository, IGameView gameView, ILogger logger) : 
    GameManager(cardRepository, saveRepository, gameView, logger)
{
    internal void ForUnitTestWithDynamiteAndBeerBarrel() => 
        GameState = new GameState(GameState.Players, new DeckForUnitTest(), GameState.CurrentPlayerId, GameState.GameView);
}
