using BLComponent;
using BLComponent.InputPorts;
using Microsoft.Extensions.Configuration;
using Moq;

namespace DBUnitTests;

public class SavesUnitTests
{
    private static bool CompareCards(Card? c1, Card? c2)
    {
        if ((c1 is null || c2 is null) && c1 != c2)
            return false;
        if (c1 is null && c2 is null)
            return true;
        if (c1!.Name != c2!.Name)
            return false;
        if (c1.Rank != c2.Rank)
            return false;
        return c1.Suit == c2.Suit;
    }
    
    private static bool ComparePlayers(Player player1, Player player2)
    {
        if (player1.Id != player2.Id)
            return false;
        if (player1.Name != player2.Name)
            return false;
        if (player1.Health != player2.Health)
            return false;
        if (player1.MaxHealth != player2.MaxHealth)
            return false;
        if (player1.Role != player2.Role)
            return false;
        if (player1.IsDeadOnThisTurn != player2.IsDeadOnThisTurn)
            return false;
        if (player1.IsBangPlayed != player2.IsBangPlayed)
            return false;
        if ((player1.Weapon is null || player2.Weapon is null) && player1.Weapon != player2.Weapon)
            return false;
        if (player1.Weapon is not null && !CompareCards(player1.Weapon, player2.Weapon))
            return false;
        if (player1.CardsInHand.Count != player2.CardsInHand.Count)
            return false;
        if (player1.CardsOnBoard.Count != player2.CardsOnBoard.Count)
            return false;
        if (player1.CardsInHand.Where((t, i) => !CompareCards(t, player2.CardsInHand[i])).Any())
            return false;
        return !player1.CardsOnBoard.Where((t, i) => !CompareCards(t, player2.CardsOnBoard[i])).Any();
    }

    private static void AssertGameManagers(GameManager manager1, GameManager manager2)
    {
        Assert.Equal(manager1.CurPlayer.Id, manager2.CurPlayer.Id);
        Assert.True(CompareCards(manager1.TopDiscardedCard, manager2.TopDiscardedCard));
        Assert.Equal(manager1.LivePlayers.Count, manager2.LivePlayers.Count);
        for (var i = 0; i < manager1.Players.Count; i++)
            Assert.True(ComparePlayers(manager1.Players[i], manager2.Players[i]));
    }
    
    [Fact]
    public async Task Save_Test()
    {
        var configMock = new Mock<IConfiguration>();
        configMock.Setup(x => x["SavesFileName"]).Returns("Bang!/test.db");
        var cardsMock = new Mock<ICardRepository>();
        var cards = new List<Card>();
        for (var i = 0; i < 50; i++)
            cards.Add(CardFactory.CreateCard(CardName.Bang, CardSuit.Diamonds, CardRank.Jack));
        cardsMock.Setup(repo => repo.GetAll).Returns(cards);
        var gameViewMock = new Mock<IGameView>();
        var saveRepo = new SaveRepositoryForTests(configMock.Object);
        var gameManager = new GameManager(cardsMock.Object, saveRepo, gameViewMock.Object);
        var players = new List<string> { "1", "2", "3", "4", "5", "6", "7" };
        gameManager.GameInit(players);
        gameManager.SaveState();
        await Task.Delay(1000);
        var newGameManager = new GameManager(cardsMock.Object, saveRepo, gameViewMock.Object);
        newGameManager.LoadState(gameManager.GetAllSaves.Keys.First());
        AssertGameManagers(gameManager, newGameManager);
        await newGameManager.EndTurn();
        await newGameManager.EndTurn();
        newGameManager.CurPlayer.AddCardInHand(CardFactory.CreateCard(CardName.Bang, CardSuit.Diamonds, CardRank.Jack),
            gameViewMock.Object);
        newGameManager.CurPlayer.AddCardInHand(CardFactory.CreateCard(CardName.Dynamite, CardSuit.Spades, CardRank.Three),
            gameViewMock.Object);
        await newGameManager.EndTurn();
        newGameManager.CurPlayer.AddCardInHand(CardFactory.CreateCard(CardName.Scope, CardSuit.Hearts, CardRank.Five),
            gameViewMock.Object);
        await newGameManager.PlayCard(newGameManager.CurPlayer.CardsInHand[^1].Id);
        await newGameManager.EndTurn();
        newGameManager.CurPlayer.AddCardInHand(
            CardFactory.CreateCard(CardName.Volcanic, CardSuit.Diamonds, CardRank.Three),
            gameViewMock.Object
        );
        newGameManager.CurPlayer.AddCardInHand(
            CardFactory.CreateCard(CardName.Winchester, CardSuit.Diamonds, CardRank.Three),
            gameViewMock.Object
        );
        await newGameManager.PlayCard(newGameManager.CurPlayer.CardsInHand[^1].Id);
        await newGameManager.PlayCard(newGameManager.CurPlayer.CardsInHand[^1].Id);
        newGameManager.SaveState();
        gameManager = new GameManager(cardsMock.Object, saveRepo, gameViewMock.Object);
        gameManager.LoadState(newGameManager.GetAllSaves.Keys.First());
        AssertGameManagers(gameManager, newGameManager);
    }
}
