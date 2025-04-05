using BLComponent;
using BLComponent.Cards;
using BLComponent.InputPorts;
using Moq;

namespace BLUnitTests;

public class GameManagerTests
{
    private readonly Mock<ICardRepository> _cardRepoMock = new();
    private readonly Mock<IGet> _getMock = new();

    private void FillDeck(int cardCount = 80, CardSuit suit = CardSuit.Diamonds)
    {
        _cardRepoMock.Setup(repo => repo.GetAll()).Returns(() =>
        {
            var deck = new List<Card>();
            for (var i = 0; i < cardCount; ++i)
                deck.Add(CardFactory.CreateCard(CardName.Bang, suit, CardRank.Seven));
            return deck;
        });
    }
    
    [Fact]
    public void GameInit_PlayerCountTests()
    {
        FillDeck();
        var gameManager = new GameManager(_cardRepoMock.Object, _getMock.Object);
        var players = new List<int>([1, 2, 3]);
        
        Assert.Throws<WrongNumberOfPlayersException>(() => gameManager.GameInit(players));
        
        players = new List<int>([1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12]);
        
        Assert.Throws<WrongNumberOfPlayersException>(() => gameManager.GameInit(players));
    }
    
    [Fact]
    public void GameInit_NotUniqueIdsTest()
    {
        FillDeck();
        var gameManager = new GameManager(_cardRepoMock.Object, _getMock.Object);
        var players = new List<int>([2, 2, 3, 5, 5, 6]);
        
        Assert.Throws<NotUniqueIdsException>(() => gameManager.GameInit(players));
    }

    [Fact]
    public void GameInit_4RolesTests()
    {
        FillDeck();
        var gameManager = new GameManager(_cardRepoMock.Object, _getMock.Object);
        var players = new List<int>([1, 2, 3, 4]);
        var expectedRoles = new List<PlayerRole> { PlayerRole.Outlaw,
            PlayerRole.Outlaw, PlayerRole.Renegade, PlayerRole.Sheriff };
        
        gameManager.GameInit(players);
        Assert.Equal(expectedRoles, gameManager.Players.Select(p => p.Role).Order());
        foreach (var player in gameManager.Players)
        {
            if (player.Role == PlayerRole.Sheriff)
                Assert.Equal(player.Health + 2, player.CardsInHand.Count);
            else
                Assert.Equal(player.Health, player.CardsInHand.Count);
        }
    }

    [Fact]
    public void GameInit_5RolesTests()
    {
        FillDeck();
        var gameManager = new GameManager(_cardRepoMock.Object, _getMock.Object);
        var players = new List<int>([1, 2, 3, 4, 5]);
        var expectedRoles = new List<PlayerRole> { PlayerRole.DeputySheriff, PlayerRole.Outlaw,
            PlayerRole.Outlaw, PlayerRole.Renegade, PlayerRole.Sheriff };
        
        gameManager.GameInit(players);
        Assert.Equal(expectedRoles, gameManager.Players.Select(p => p.Role).Order());
        foreach (var player in gameManager.Players)
        {
            if (player.Role == PlayerRole.Sheriff)
                Assert.Equal(player.Health + 2, player.CardsInHand.Count);
            else
                Assert.Equal(player.Health, player.CardsInHand.Count);
        }
    }
    
    [Fact]
    public void GameInit_6RolesTests()
    {
        FillDeck();
        var gameManager = new GameManager(_cardRepoMock.Object, _getMock.Object);
        var players = new List<int>([1, 2, 3, 4, 5, 6]);
        var expectedRoles = new List<PlayerRole> { PlayerRole.DeputySheriff, PlayerRole.Outlaw,
            PlayerRole.Outlaw, PlayerRole.Outlaw, PlayerRole.Renegade, PlayerRole.Sheriff };
        
        gameManager.GameInit(players);
        Assert.Equal(expectedRoles, gameManager.Players.Select(p => p.Role).Order());
        foreach (var player in gameManager.Players)
        {
            if (player.Role == PlayerRole.Sheriff)
                Assert.Equal(player.Health + 2, player.CardsInHand.Count);
            else
                Assert.Equal(player.Health, player.CardsInHand.Count);
        }
    }
    
    [Fact]
    public void GameInit_7RolesTests()
    {
        FillDeck();
        var gameManager = new GameManager(_cardRepoMock.Object, _getMock.Object);
        var players = new List<int>([1, 2, 3, 4, 5, 6, 7]);
        var expectedRoles = new List<PlayerRole> { PlayerRole.DeputySheriff, PlayerRole.DeputySheriff, PlayerRole.Outlaw,
            PlayerRole.Outlaw, PlayerRole.Outlaw, PlayerRole.Renegade, PlayerRole.Sheriff };
        
        gameManager.GameInit(players);
        Assert.Equal(expectedRoles, gameManager.Players.Select(p => p.Role).Order());
        foreach (var player in gameManager.Players)
        {
            if (player.Role == PlayerRole.Sheriff)
                Assert.Equal(player.Health + 2, player.CardsInHand.Count);
            else
                Assert.Equal(player.Health, player.CardsInHand.Count);
        }
    }

    [Fact]
    public void PlayCard_NotInHandTest()
    {
        FillDeck();
        var gameManager = new GameManager(_cardRepoMock.Object, _getMock.Object);
        var players = new List<int>([1, 2, 3, 4, 5, 6, 7]);
        gameManager.GameInit(players);
        Assert.Throws<ArgumentOutOfRangeException>(() => gameManager.PlayCard(-1));
    }

    [Fact]
    public void PlayCard_TooFarTest()
    {
        FillDeck();
        var gameManager = new GameManager(_cardRepoMock.Object, _getMock.Object);
        var players = new List<int>([1, 2, 3, 4, 5, 6, 7]);
        gameManager.GameInit(players);
        var cardCount = gameManager.CurPlayer.CardsInHand.Count;
        _getMock.Setup(get => get.GetPlayerIndex(gameManager.LivePlayers(), 0)).Returns(2);

        var rc = gameManager.PlayCard(0);
        Assert.Equal(CardRc.TooFar, rc);
        Assert.Equal(cardCount, gameManager.CurPlayer.CardsInHand.Count);
    }
    
    [Fact]
    public void PlayCard_CantPlayTest()
    {
        FillDeck();
        var gameManager = new GameManager(_cardRepoMock.Object, _getMock.Object);
        var players = new List<int>([1, 2, 3, 4, 5, 6, 7]);
        gameManager.GameInit(players);
        var cardCount = gameManager.CurPlayer.CardsInHand.Count;
        _getMock.Setup(get => get.GetPlayerIndex(gameManager.LivePlayers(), 0)).Returns(1);

        var rc = gameManager.PlayCard(0);
        Assert.Equal(CardRc.Ok, rc);
        Assert.Equal(cardCount - 1, gameManager.CurPlayer.CardsInHand.Count);
        --cardCount;

        rc = gameManager.PlayCard(0);
        Assert.Equal(CardRc.CantPlay, rc);
        Assert.Equal(cardCount, gameManager.CurPlayer.CardsInHand.Count);

        gameManager.CurPlayer.ChangeWeapon((WeaponCard)CardFactory.CreateCard(CardName.Volcanic, CardSuit.Clubs, CardRank.Ace));
        
        rc = gameManager.PlayCard(0);
        Assert.Equal(CardRc.Ok, rc);
        Assert.Equal(cardCount - 1, gameManager.CurPlayer.CardsInHand.Count);
    }
    
    [Fact]
    public void PlayCard_OutlawDeathTest()
    {
        FillDeck();
        var gameManager = new GameManager(_cardRepoMock.Object, _getMock.Object);
        var players = new List<int>([1, 2, 3, 4, 5, 6, 7]);
        gameManager.GameInit(players);
        var cardCount = gameManager.CurPlayer.CardsInHand.Count;
        var id = gameManager.CurPlayer.Id;
        gameManager.CurPlayer.ChangeWeapon((WeaponCard)CardFactory.CreateCard(CardName.Winchester, CardSuit.Clubs, CardRank.Ace));
        var outlawIndex = gameManager.LivePlayers().FindIndex(p => p.Role == PlayerRole.Outlaw);
        _getMock.Setup(get => get.GetPlayerIndex(gameManager.LivePlayers(), 0)).Returns(outlawIndex);

        gameManager.LivePlayers()[outlawIndex].ApplyDamage(3, new GameContext(gameManager.LivePlayers(),
            null!, outlawIndex, _getMock.Object));
        var rc = gameManager.PlayCard(0);
        Assert.Equal(CardRc.Ok, rc);
        Assert.Equal(players.Count - 1, gameManager.LivePlayers().Count);
        Assert.Equal(cardCount + 2, gameManager.CurPlayer.CardsInHand.Count);
        Assert.Equal(id, gameManager.CurPlayer.Id);
    }
    
    [Fact]
    public void PlayCard_RenegadeDeathTest()
    {
        FillDeck();
        var gameManager = new GameManager(_cardRepoMock.Object, _getMock.Object);
        var players = new List<int>([1, 2, 3, 4, 5, 6, 7]);
        gameManager.GameInit(players);
        var cardCount = gameManager.CurPlayer.CardsInHand.Count;
        var id = gameManager.CurPlayer.Id;
        gameManager.CurPlayer.ChangeWeapon((WeaponCard)CardFactory.CreateCard(CardName.Winchester, CardSuit.Clubs, CardRank.Ace));
        var renegadeIndex = gameManager.LivePlayers().FindIndex(p => p.Role == PlayerRole.Renegade);
        _getMock.Setup(get => get.GetPlayerIndex(gameManager.LivePlayers(), 0)).Returns(renegadeIndex);

        gameManager.LivePlayers()[renegadeIndex].ApplyDamage(3, new GameContext(gameManager.LivePlayers(),
            null!, renegadeIndex, _getMock.Object));
        var rc = gameManager.PlayCard(0);
        Assert.Equal(CardRc.Ok, rc);
        Assert.Equal(players.Count - 1, gameManager.LivePlayers().Count);
        Assert.Equal(cardCount - 1, gameManager.CurPlayer.CardsInHand.Count);
        Assert.Equal(id, gameManager.CurPlayer.Id);
    }
    
    [Fact]
    public void PlayCard_DeputyDeathTest()
    {
        FillDeck();
        var gameManager = new GameManager(_cardRepoMock.Object, _getMock.Object);
        var players = new List<int>([1, 2, 3, 4, 5, 6, 7]);
        gameManager.GameInit(players);
        var id = gameManager.CurPlayer.Id;
        gameManager.CurPlayer.ChangeWeapon((WeaponCard)CardFactory.CreateCard(CardName.Winchester, CardSuit.Clubs, CardRank.Ace));
        var deputyIndex = gameManager.LivePlayers().FindIndex(p => p.Role == PlayerRole.DeputySheriff);
        _getMock.Setup(get => get.GetPlayerIndex(gameManager.LivePlayers(), 0)).Returns(deputyIndex);

        gameManager.LivePlayers()[deputyIndex].ApplyDamage(3, new GameContext(gameManager.LivePlayers(),
            null!, deputyIndex, _getMock.Object));
        var rc = gameManager.PlayCard(0);
        Assert.Equal(CardRc.Ok, rc);
        Assert.Equal(players.Count - 1, gameManager.LivePlayers().Count);
        Assert.Empty(gameManager.CurPlayer.CardsInHand);
        Assert.Equal(id, gameManager.CurPlayer.Id);
    }
    
    [Fact]
    public void PlayCard_IndiansTest()
    {
        FillDeck();
        var gameManager = new GameManager(_cardRepoMock.Object, _getMock.Object);
        var players = new List<int>([1, 2, 3, 4, 5, 6, 7]);
        gameManager.GameInit(players);
        var id = gameManager.CurPlayer.Id;
        gameManager.CurPlayer.AddCardInHand(CardFactory.CreateCard(CardName.Indians, CardSuit.Clubs, CardRank.Ace));
        var deputyIndex = gameManager.LivePlayers().FindIndex(p => p.Role == PlayerRole.DeputySheriff);
        _getMock.Setup(get => get.GetPlayerIndex(gameManager.LivePlayers(), 0)).Returns(deputyIndex);

        gameManager.LivePlayers()[deputyIndex].ApplyDamage(3, new GameContext(gameManager.LivePlayers(),
            null!, deputyIndex, _getMock.Object));
        var deputyCardCount = gameManager.LivePlayers()[deputyIndex].CardsInHand.Count;
        for (var i = 0; i < deputyCardCount; ++i)
            gameManager.LivePlayers()[deputyIndex].RemoveCard(0);
        var cardCount = gameManager.CurPlayer.CardsInHand.Count;
        var rc = gameManager.PlayCard(cardCount - 1);
        Assert.Equal(CardRc.Ok, rc);
        Assert.Equal(players.Count - 1, gameManager.LivePlayers().Count);
        Assert.Equal(cardCount - 1, gameManager.CurPlayer.CardsInHand.Count);
        Assert.Equal(id, gameManager.CurPlayer.Id);
    }

    [Fact]
    public void DiscardCard_NotInHandTest()
    {
        FillDeck();
        var gameManager = new GameManager(_cardRepoMock.Object, _getMock.Object);
        var players = new List<int>([1, 2, 3, 4, 5, 6, 7]);
        gameManager.GameInit(players);
        Assert.Throws<DiscardNotExistingCardException>(() => gameManager.DiscardCard(-1));
    }

    [Fact]
    public void DiscardCard_Test()
    {
        FillDeck();
        var gameManager = new GameManager(_cardRepoMock.Object, _getMock.Object);
        var players = new List<int>([1, 2, 3, 4, 5, 6, 7]);
        gameManager.GameInit(players);
        var cardCount = gameManager.CurPlayer.CardsInHand.Count;
        gameManager.DiscardCard(0);
        Assert.Equal(cardCount - 1, gameManager.CurPlayer.CardsInHand.Count);
    }

    [Fact]
    public void EndTurn_TooMuchCardsTest()
    {
        FillDeck();
        var gameManager = new GameManager(_cardRepoMock.Object, _getMock.Object);
        var players = new List<int>([1, 2, 3, 4, 5, 6, 7]);
        gameManager.GameInit(players);
        var playerId = gameManager.CurPlayer.Id;
        var rc = gameManager.EndTurn();
        Assert.Equal(CardRc.CantEndTurn, rc);
        Assert.Equal(playerId, gameManager.CurPlayer.Id);
    }
    
    [Fact]
    public void EndTurn_NoCardsOnBoardTest()
    {
        FillDeck();
        var gameManager = new GameManager(_cardRepoMock.Object, _getMock.Object);
        var players = new List<int>([1, 2, 3, 4, 5, 6, 7]);
        gameManager.GameInit(players);
        var playerId = gameManager.LivePlayers()[1].Id;
        var playerCardCount = gameManager.LivePlayers()[1].CardsInHand.Count;
        gameManager.CurPlayer.RemoveCard(0);
        gameManager.CurPlayer.RemoveCard(0);
        var rc = gameManager.EndTurn();
        Assert.Equal(CardRc.Ok, rc);
        Assert.Equal(playerId, gameManager.CurPlayer.Id);
        Assert.Equal(playerCardCount + 2, gameManager.CurPlayer.CardsInHand.Count);
    }
    
    [Fact]
    public void EndTurn_DynamiteSheriffDeathTest()
    {
        FillDeck(suit: CardSuit.Spades);
        var gameManager = new GameManager(_cardRepoMock.Object, _getMock.Object);
        var players = new List<int>([1, 2, 3, 4]);
        gameManager.GameInit(players);
        gameManager.CurPlayer.ApplyDamage(3, new GameContext(gameManager.LivePlayers(),
            null!, 0, _getMock.Object));
        for (var i = 0; i < 5; ++i)
            gameManager.CurPlayer.RemoveCard(0);
        gameManager.CurPlayer.AddCardOnBoard(CardFactory.CreateCard(CardName.Dynamite, CardSuit.Clubs, CardRank.Ace));
        for (var i = 0; i < 3; ++i)
        {
            gameManager.EndTurn();
            gameManager.CurPlayer.RemoveCard(0);
            gameManager.CurPlayer.RemoveCard(0);
        }
        var rc = gameManager.EndTurn();
        Assert.Equal(CardRc.OutlawWin, rc);
    }
    
    [Fact]
    public void EndTurn_DynamiteDeathTest()
    {
        FillDeck(suit: CardSuit.Spades);
        var gameManager = new GameManager(_cardRepoMock.Object, _getMock.Object);
        var players = new List<int>([1, 2, 3, 4]);
        gameManager.GameInit(players);
        gameManager.CurPlayer.RemoveCard(0);
        gameManager.CurPlayer.RemoveCard(0);
        gameManager.EndTurn();
        var playerId = gameManager.CurPlayer.Id;
        gameManager.CurPlayer.ApplyDamage(3, new GameContext(gameManager.LivePlayers(),
            null!, 0, _getMock.Object));
        for (var i = 0; i < 5; ++i)
            gameManager.CurPlayer.RemoveCard(0);
        gameManager.CurPlayer.AddCardOnBoard(CardFactory.CreateCard(CardName.Dynamite, CardSuit.Clubs, CardRank.Ace));
        for (var i = 0; i < 3; ++i)
        {
            gameManager.EndTurn();
            gameManager.CurPlayer.RemoveCard(0);
            gameManager.CurPlayer.RemoveCard(0);
        }
        var rc = gameManager.EndTurn();
        Assert.Equal(CardRc.Ok, rc);
        Assert.Equal(players.Count - 1, gameManager.LivePlayers().Count);
        Assert.NotEqual(playerId, gameManager.CurPlayer.Id);
    }
    
    [Fact]
    public void EndTurn_DynamiteWithoutDeathTest()
    {
        FillDeck(suit: CardSuit.Spades);
        var gameManager = new GameManager(_cardRepoMock.Object, _getMock.Object);
        var players = new List<int>([1, 2, 3, 4]);
        gameManager.GameInit(players);
        for (var i = 0; i < 5; ++i)
            gameManager.CurPlayer.RemoveCard(0);
        var playerId = gameManager.CurPlayer.Id;
        gameManager.CurPlayer.AddCardOnBoard(CardFactory.CreateCard(CardName.Dynamite, CardSuit.Clubs, CardRank.Ace));
        for (var i = 0; i < 3; ++i)
        {
            gameManager.EndTurn();
            gameManager.CurPlayer.RemoveCard(0);
            gameManager.CurPlayer.RemoveCard(0);
        }
        var rc = gameManager.EndTurn();
        Assert.Equal(CardRc.Ok, rc);
        Assert.Equal(players.Count, gameManager.LivePlayers().Count);
        Assert.Equal(playerId, gameManager.CurPlayer.Id);
    }
    
    [Fact]
    public void EndTurn_DynamiteReviveTest()
    {
        FillDeck(26, CardSuit.Spades);
        var gameManager = new GameManager(_cardRepoMock.Object, _getMock.Object);
        var players = new List<int>([1, 2, 3, 4]);
        gameManager.GameInit(players);
        gameManager.CurPlayer.ApplyDamage(3, new GameContext(gameManager.LivePlayers(),
            null!, 0, _getMock.Object));
        for (var i = 0; i < 5; ++i)
            gameManager.CurPlayer.RemoveCard(0);
        var playerId = gameManager.CurPlayer.Id;
        gameManager.CurPlayer.AddCardOnBoard(CardFactory.CreateCard(CardName.Dynamite, CardSuit.Clubs, CardRank.Seven));
        gameManager.CurPlayer.AddCardOnBoard(CardFactory.CreateCard(CardName.BeerBarrel, CardSuit.Clubs, CardRank.Ace));
        for (var i = 0; i < 3; ++i)
        {
            gameManager.EndTurn();
            gameManager.CurPlayer.RemoveCard(0);
            gameManager.CurPlayer.RemoveCard(0);
        }
        gameManager.ForUnitTestWithDynamiteAndBeerBarrel();
        var rc = gameManager.EndTurn();
        Assert.Equal(CardRc.Ok, rc);
        Assert.Equal(players.Count, gameManager.LivePlayers().Count);
        Assert.Equal(playerId, gameManager.CurPlayer.Id);
        Assert.Equal(1, gameManager.CurPlayer.Health);
    }
    
    [Fact]
    public void EndTurn_BeerBarrelTest()
    {
        FillDeck(suit: CardSuit.Clubs);
        var gameManager = new GameManager(_cardRepoMock.Object, _getMock.Object);
        var players = new List<int>([1, 2, 3, 4]);
        gameManager.GameInit(players);
        gameManager.CurPlayer.ApplyDamage(3, new GameContext(gameManager.LivePlayers(),
            null!, 0, _getMock.Object));
        for (var i = 0; i < 5; ++i)
            gameManager.CurPlayer.RemoveCard(0);
        gameManager.CurPlayer.AddCardOnBoard(CardFactory.CreateCard(CardName.BeerBarrel, CardSuit.Clubs, CardRank.Ace));
        for (var i = 0; i < 3; ++i)
        {
            gameManager.EndTurn();
            gameManager.CurPlayer.RemoveCard(0);
            gameManager.CurPlayer.RemoveCard(0);
        }
        var rc = gameManager.EndTurn();
        Assert.Equal(CardRc.Ok, rc);
        Assert.Equal(4, gameManager.CurPlayer.Health);
    }
    
    [Fact]
    public void EndTurn_JailTest()
    {
        FillDeck();
        var gameManager = new GameManager(_cardRepoMock.Object, _getMock.Object);
        var players = new List<int>([1, 2, 3, 4]);
        gameManager.GameInit(players);
        for (var i = 0; i < 2; ++i)
            gameManager.CurPlayer.RemoveCard(0);
        var playerId = gameManager.CurPlayer.Id;
        gameManager.CurPlayer.AddCardOnBoard(CardFactory.CreateCard(CardName.Jail, CardSuit.Clubs, CardRank.Ace));
        for (var i = 0; i < 3; ++i)
        {
            gameManager.EndTurn();
            gameManager.CurPlayer.RemoveCard(0);
            gameManager.CurPlayer.RemoveCard(0);
        }
        var rc = gameManager.EndTurn();
        Assert.Equal(CardRc.Ok, rc);
        Assert.NotEqual(playerId, gameManager.CurPlayer.Id);
    }

    [Fact]
    public void CheckEndGame_SheriffWinTest()
    {
        FillDeck();
        var gameManager = new GameManager(_cardRepoMock.Object, _getMock.Object);
        var players = new List<int>([1, 2, 3, 4, 5]);
        gameManager.GameInit(players);
        foreach (var player in gameManager.Players.Where(p => p.Role is not PlayerRole.Sheriff and
                     not PlayerRole.DeputySheriff))
            player.ApplyDamage(4,
                new GameContext(gameManager.LivePlayers(), null!, 0, _getMock.Object));
        var rc = gameManager.CheckEndGame();
        Assert.Equal(CardRc.SheriffWin, rc);
        Assert.Equal(2, gameManager.LivePlayers().Count);
    }
    
    [Fact]
    public void CheckEndGame_RenegadeWinTest()
    {
        FillDeck();
        var gameManager = new GameManager(_cardRepoMock.Object, _getMock.Object);
        var players = new List<int>([1, 2, 3, 4]);
        gameManager.GameInit(players);
        foreach (var player in gameManager.Players.Where(p => p.Role is not PlayerRole.Renegade))
            player.ApplyDamage(5,
                new GameContext(gameManager.LivePlayers(), null!, 0, _getMock.Object));
        var rc = gameManager.CheckEndGame();
        Assert.Equal(CardRc.RenegadeWin, rc);
        Assert.Single(gameManager.LivePlayers());
    }

    [Fact]
    public void TopDiscardedCard_Test()
    {
        FillDeck();
        var gameManager = new GameManager(_cardRepoMock.Object, _getMock.Object);
        var players = new List<int>([1, 2, 3, 4]);
        gameManager.GameInit(players);
        gameManager.DiscardCard(0);
        Assert.Equivalent(CardFactory.CreateCard(CardName.Bang, CardSuit.Diamonds, CardRank.Seven),
            gameManager.TopDiscardedCard);
    }
}