using BLComponent;
using BLComponent.InputPorts;
using Moq;

namespace BLUnitTests;

public class DeckTests
{
    [Fact]
    public void Deck_Tests()
    {
        var cardRepo = new Mock<ICardRepository>();
        cardRepo.Setup(repo => repo.GetAll).Returns([
            CardFactory.CreateCard(CardName.Bang, CardSuit.Hearts, CardRank.Ace),
            CardFactory.CreateCard(CardName.Bang, CardSuit.Spades, CardRank.Seven)
        ]);
        var gameView = new Mock<IGameView>();
        gameView.Setup(get => get.CardDiscarded(It.IsAny<Guid>()));
        var deck = new Deck(cardRepo.Object);
        Assert.Equal(2, deck.DrawPile.Count);
        Assert.Null(deck.TopDiscardedCard);
        var card1 = deck.Draw(gameView.Object);
        Assert.Single(deck.DrawPile);
        Assert.Null(deck.TopDiscardedCard);
        var card2 = deck.Draw(gameView.Object);
        Assert.Empty(deck.DrawPile);
        Assert.Null(deck.TopDiscardedCard);
        deck.Discard(card1, gameView.Object);
        Assert.Empty(deck.DrawPile);
        Assert.NotNull(deck.TopDiscardedCard);
        deck.Discard(card2, gameView.Object);
        Assert.Empty(deck.DrawPile);
        Assert.NotNull(deck.TopDiscardedCard);
        deck.Draw(gameView.Object);
        Assert.Single(deck.DrawPile);
        Assert.Null(deck.TopDiscardedCard);
    }
}
