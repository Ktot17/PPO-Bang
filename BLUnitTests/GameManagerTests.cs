using BLComponent;
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
        var players = new List<Guid>([Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()]);
        
        Assert.Throws<WrongNumberOfPlayersException>(() => gameManager.GameInit(players));
        
        players = new List<Guid>([Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()]);
        
        Assert.Throws<WrongNumberOfPlayersException>(() => gameManager.GameInit(players));
    }
    
    [Fact]
    public void GameInit_NotUniqueIdsTest()
    {
        FillDeck();
        var gameManager = new GameManager(_cardRepoMock.Object, _getMock.Object);
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var players = new List<Guid>([id1, id1, Guid.NewGuid(), id2, id2, Guid.NewGuid()]);
        
        Assert.Throws<NotUniqueIdsException>(() => gameManager.GameInit(players));
    }

    [Fact]
    public void GameInit_4RolesTests()
    {
        FillDeck();
        var gameManager = new GameManager(_cardRepoMock.Object, _getMock.Object);
        var players = new List<Guid>([Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()]);
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
        var players = new List<Guid>([Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()]);
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
        var players = new List<Guid>([Guid.NewGuid(), Guid.NewGuid(),
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()]);
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
        var players = new List<Guid>([Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()]);
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
        var players = new List<Guid>([Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
                Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()]);
        gameManager.GameInit(players);
        Assert.Throws<NotExistingGuidException>(() => gameManager.PlayCard(Guid.NewGuid()));
    }

    [Fact]
    public void PlayCard_TooFarTest()
    {
        FillDeck();
        var gameManager = new GameManager(_cardRepoMock.Object, _getMock.Object);
        var players = new List<Guid>([Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
                Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()]);
        gameManager.GameInit(players);
        var id = gameManager.CurPlayer.Id;
        var cardCount = gameManager.CurPlayer.CardsInHand.Count;
        _getMock.Setup(get => get.GetPlayerId(gameManager.LivePlayers.Skip(1).ToList(), id)).Returns(gameManager.LivePlayers[2].Id);

        var rc = gameManager.PlayCard(gameManager.CurPlayer.CardsInHand[0].Id);
        Assert.Equal(CardRc.TooFar, rc);
        Assert.Equal(cardCount, gameManager.CurPlayer.CardsInHand.Count);
        Assert.Equal(2, gameManager.GetRange(id, gameManager.LivePlayers[2].Id));
    }
    
    [Fact]
    public void PlayCard_CantPlayTest()
    {
        FillDeck();
        var gameManager = new GameManager(_cardRepoMock.Object, _getMock.Object);
        var players = new List<Guid>([Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
                Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()]);
        gameManager.GameInit(players);
        var id = gameManager.CurPlayer.Id;
        var cardCount = gameManager.CurPlayer.CardsInHand.Count;
        _getMock.Setup(get => get.GetPlayerId(gameManager.LivePlayers.Skip(1).ToList(), id)).Returns(gameManager.LivePlayers[1].Id);

        var rc = gameManager.PlayCard(gameManager.CurPlayer.CardsInHand[0].Id);
        Assert.Equal(CardRc.Ok, rc);
        Assert.Equal(cardCount - 1, gameManager.CurPlayer.CardsInHand.Count);
        --cardCount;

        rc = gameManager.PlayCard(gameManager.CurPlayer.CardsInHand[0].Id);
        Assert.Equal(CardRc.CantPlay, rc);
        Assert.Equal(cardCount, gameManager.CurPlayer.CardsInHand.Count);

        gameManager.CurPlayer.ChangeWeapon((WeaponCard)CardFactory.CreateCard(CardName.Volcanic, CardSuit.Clubs, CardRank.Ace));
        
        rc = gameManager.PlayCard(gameManager.CurPlayer.CardsInHand[0].Id);
        Assert.Equal(CardRc.Ok, rc);
        Assert.Equal(cardCount - 1, gameManager.CurPlayer.CardsInHand.Count);
    }
    
    [Fact]
    public void PlayCard_OutlawDeathTest()
    {
        FillDeck();
        var gameManager = new GameManager(_cardRepoMock.Object, _getMock.Object);
        var players = new List<Guid>([Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
                Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()]);
        gameManager.GameInit(players);
        var cardCount = gameManager.CurPlayer.CardsInHand.Count;
        var id = gameManager.CurPlayer.Id;
        gameManager.CurPlayer.ChangeWeapon((WeaponCard)CardFactory.CreateCard(CardName.Winchester, CardSuit.Clubs, CardRank.Ace));
        var outlawId = gameManager.LivePlayers.First(p => p.Role == PlayerRole.Outlaw).Id;
        _getMock.Setup(get => get.GetPlayerId(gameManager.LivePlayers.Skip(1).ToList(), id)).Returns(outlawId);

        gameManager.LivePlayers.First(p => p.Id == outlawId).ApplyDamage(3,
            new GameState(gameManager.LivePlayers, null!, outlawId, _getMock.Object));
        var rc = gameManager.PlayCard(gameManager.CurPlayer.CardsInHand[0].Id);
        Assert.Equal(CardRc.Ok, rc);
        Assert.Equal(players.Count - 1, gameManager.LivePlayers.Count);
        Assert.Equal(cardCount + 2, gameManager.CurPlayer.CardsInHand.Count);
        Assert.Equal(id, gameManager.CurPlayer.Id);
    }
    
    [Fact]
    public void PlayCard_RenegadeDeathTest()
    {
        FillDeck();
        var gameManager = new GameManager(_cardRepoMock.Object, _getMock.Object);
        var players = new List<Guid>([Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
                Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()]);
        gameManager.GameInit(players);
        var cardCount = gameManager.CurPlayer.CardsInHand.Count;
        var id = gameManager.CurPlayer.Id;
        gameManager.CurPlayer.ChangeWeapon((WeaponCard)CardFactory.CreateCard(CardName.Winchester, CardSuit.Clubs, CardRank.Ace));
        var renegadeId = gameManager.LivePlayers.First(p => p.Role == PlayerRole.Renegade).Id;
        _getMock.Setup(get => get.GetPlayerId(gameManager.LivePlayers.Skip(1).ToList(), id)).Returns(renegadeId);

        gameManager.LivePlayers.First(p => p.Id == renegadeId).ApplyDamage(3,
            new GameState(gameManager.LivePlayers, null!, renegadeId, _getMock.Object));
        var rc = gameManager.PlayCard(gameManager.CurPlayer.CardsInHand[0].Id);
        Assert.Equal(CardRc.Ok, rc);
        Assert.Equal(players.Count - 1, gameManager.LivePlayers.Count);
        Assert.Equal(cardCount - 1, gameManager.CurPlayer.CardsInHand.Count);
        Assert.Equal(id, gameManager.CurPlayer.Id);
    }
    
    [Fact]
    public void PlayCard_DeputyDeathTest()
    {
        FillDeck();
        var gameManager = new GameManager(_cardRepoMock.Object, _getMock.Object);
        var players = new List<Guid>([Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
                Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()]);
        gameManager.GameInit(players);
        var id = gameManager.CurPlayer.Id;
        gameManager.CurPlayer.ChangeWeapon((WeaponCard)CardFactory.CreateCard(CardName.Winchester, CardSuit.Clubs, CardRank.Ace));
        var deputyId = gameManager.LivePlayers.First(p => p.Role == PlayerRole.DeputySheriff).Id;
        _getMock.Setup(get => get.GetPlayerId(gameManager.LivePlayers.Skip(1).ToList(), id)).Returns(deputyId);

        gameManager.LivePlayers.First(p => p.Id == deputyId).ApplyDamage(3,
            new GameState(gameManager.LivePlayers, null!, deputyId, _getMock.Object));
        var rc = gameManager.PlayCard(gameManager.CurPlayer.CardsInHand[0].Id);
        Assert.Equal(CardRc.Ok, rc);
        Assert.Equal(players.Count - 1, gameManager.LivePlayers.Count);
        Assert.Empty(gameManager.CurPlayer.CardsInHand);
        Assert.Equal(id, gameManager.CurPlayer.Id);
    }
    
    [Fact]
    public void PlayCard_IndiansTest()
    {
        FillDeck();
        var gameManager = new GameManager(_cardRepoMock.Object, _getMock.Object);
        var players = new List<Guid>([Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
                Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()]);
        gameManager.GameInit(players);
        var id = gameManager.CurPlayer.Id;
        var indians = CardFactory.CreateCard(CardName.Indians, CardSuit.Clubs, CardRank.Ace);
        gameManager.CurPlayer.AddCardInHand(indians);
        var deputy = gameManager.LivePlayers.First(p => p.Role == PlayerRole.DeputySheriff);
        _getMock.Setup(get => get.GetPlayerId(gameManager.LivePlayers, id)).Returns(deputy.Id);

        deputy.ApplyDamage(3, new GameState(gameManager.LivePlayers, null!, deputy.Id, _getMock.Object));
        var deputyCardCount = deputy.CardsInHand.Count;
        for (var i = 0; i < deputyCardCount; ++i)
            deputy.RemoveCard(deputy.CardsInHand[0].Id);
        deputy.AddCardOnBoard(CardFactory.CreateCard(CardName.Scope, CardSuit.Clubs, CardRank.Ace));
        var cardCount = gameManager.CurPlayer.CardsInHand.Count;
        var rc = gameManager.PlayCard(indians.Id);
        Assert.Equal(CardRc.Ok, rc);
        Assert.Equal(players.Count - 1, gameManager.LivePlayers.Count);
        Assert.Equal(cardCount - 1, gameManager.CurPlayer.CardsInHand.Count);
        Assert.Equal(id, gameManager.CurPlayer.Id);
    }
    
    [Fact]
    public void PlayCard_DuelTest()
    {
        FillDeck();
        var gameManager = new GameManager(_cardRepoMock.Object, _getMock.Object);
        var players = new List<Guid>([Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
                Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()]);
        gameManager.GameInit(players);
        var id = gameManager.CurPlayer.Id;
        var duel = CardFactory.CreateCard(CardName.Duel, CardSuit.Clubs, CardRank.Ace);
        gameManager.CurPlayer.AddCardInHand(CardFactory.CreateCard(CardName.Bang, CardSuit.Clubs, CardRank.Ace));
        gameManager.CurPlayer.AddCardInHand(CardFactory.CreateCard(CardName.Bang, CardSuit.Clubs, CardRank.Ace));
        gameManager.CurPlayer.AddCardInHand(duel);
        gameManager.CurPlayer.AddCardInHand(CardFactory.CreateCard(CardName.Bang, CardSuit.Clubs, CardRank.Ace));
        gameManager.CurPlayer.AddCardInHand(CardFactory.CreateCard(CardName.Bang, CardSuit.Clubs, CardRank.Ace));
        var deputy = gameManager.LivePlayers.First(p => p.Role == PlayerRole.DeputySheriff);
        _getMock.Setup(get => get.GetPlayerId(gameManager.LivePlayers.Skip(1).ToList(), id)).Returns(deputy.Id);
        
        var deputyCardCount = deputy.CardsInHand.Count;
        for (var i = 0; i < deputyCardCount; ++i)
            deputy.RemoveCard(deputy.CardsInHand[0].Id);
        deputy.AddCardInHand(CardFactory.CreateCard(CardName.Bang, CardSuit.Clubs, CardRank.Ace));
        var cardCount = gameManager.CurPlayer.CardsInHand.Count;
        var rc = gameManager.PlayCard(duel.Id);
        Assert.Equal(CardRc.Ok, rc);
        Assert.Equal(players.Count, gameManager.LivePlayers.Count);
        Assert.Equal(cardCount - 2, gameManager.CurPlayer.CardsInHand.Count);
        Assert.Equal(id, gameManager.CurPlayer.Id);
    }

    [Fact]
    public void PlayCard_LastPlayerKillsPlayerTest()
    {
        FillDeck();
        var gameManager = new GameManager(_cardRepoMock.Object, _getMock.Object);
        var players = new List<Guid>([Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()]);
        gameManager.GameInit(players);
        var sheriffId = gameManager.CurPlayer.Id;
        gameManager.LivePlayers[0].ApplyDamage(4, null!);
        gameManager.LivePlayers[0].RemoveCard(gameManager.LivePlayers[0].CardsInHand[0].Id);
        gameManager.LivePlayers[0].RemoveCard(gameManager.LivePlayers[0].CardsInHand[0].Id);
        gameManager.LivePlayers[0].RemoveCard(gameManager.LivePlayers[0].CardsInHand[0].Id);
        gameManager.LivePlayers[0].RemoveCard(gameManager.LivePlayers[0].CardsInHand[0].Id);
        for (var i = 0; i < players.Count - 1; ++i)
        {
            gameManager.LivePlayers[i].RemoveCard(gameManager.LivePlayers[i].CardsInHand[0].Id);
            gameManager.LivePlayers[i].RemoveCard(gameManager.LivePlayers[i].CardsInHand[0].Id);
            gameManager.EndTurn();
        }

        var id = gameManager.CurPlayer.Id;
        _getMock.Setup(get => get.GetPlayerId(gameManager.LivePlayers.SkipLast(1).ToList(), id)).Returns(sheriffId);
        var cardCount = gameManager.CurPlayer.CardsInHand.Count;
        var rc = gameManager.PlayCard(gameManager.CurPlayer.CardsInHand[0].Id);
        Assert.Equal(CardRc.OutlawWin, rc);
        Assert.Equal(players.Count - 1, gameManager.LivePlayers.Count);
        Assert.Equal(cardCount - 1, gameManager.CurPlayer.CardsInHand.Count);
        Assert.Equal(id, gameManager.CurPlayer.Id);
    }

    [Fact]
    public void DiscardCard_NotInHandTest()
    {
        FillDeck();
        var gameManager = new GameManager(_cardRepoMock.Object, _getMock.Object);
        var players = new List<Guid>([Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
                Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()]);
        gameManager.GameInit(players);
        Assert.Throws<NotExistingGuidException>(() => gameManager.DiscardCard(Guid.NewGuid()));
    }

    [Fact]
    public void DiscardCard_Test()
    {
        FillDeck();
        var gameManager = new GameManager(_cardRepoMock.Object, _getMock.Object);
        var players = new List<Guid>([Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
                Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()]);
        gameManager.GameInit(players);
        var cardCount = gameManager.CurPlayer.CardsInHand.Count;
        gameManager.DiscardCard(gameManager.CurPlayer.CardsInHand[0].Id);
        Assert.Equal(cardCount - 1, gameManager.CurPlayer.CardsInHand.Count);
    }

    [Fact]
    public void EndTurn_TooMuchCardsTest()
    {
        FillDeck();
        var gameManager = new GameManager(_cardRepoMock.Object, _getMock.Object);
        var players = new List<Guid>([Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
                Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()]);
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
        var players = new List<Guid>([Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
                Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()]);
        gameManager.GameInit(players);
        var playerId = gameManager.LivePlayers[1].Id;
        var playerCardCount = gameManager.LivePlayers[1].CardsInHand.Count;
        gameManager.CurPlayer.RemoveCard(gameManager.CurPlayer.CardsInHand[0].Id);
        gameManager.CurPlayer.RemoveCard(gameManager.CurPlayer.CardsInHand[0].Id);
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
        var players = new List<Guid>([Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()]);
        gameManager.GameInit(players);
        gameManager.CurPlayer.ApplyDamage(3, new GameState(gameManager.LivePlayers,
            null!, gameManager.CurPlayer.Id, _getMock.Object));
        for (var i = 0; i < 5; ++i)
            gameManager.CurPlayer.RemoveCard(gameManager.CurPlayer.CardsInHand[0].Id);
        gameManager.CurPlayer.AddCardOnBoard(CardFactory.CreateCard(CardName.Dynamite, CardSuit.Clubs, CardRank.Ace));
        for (var i = 0; i < 3; ++i)
        {
            gameManager.EndTurn();
            gameManager.CurPlayer.RemoveCard(gameManager.CurPlayer.CardsInHand[0].Id);
            gameManager.CurPlayer.RemoveCard(gameManager.CurPlayer.CardsInHand[0].Id);
        }
        var rc = gameManager.EndTurn();
        Assert.Equal(CardRc.OutlawWin, rc);
    }
    
    [Fact]
    public void EndTurn_DynamiteDeathTest()
    {
        FillDeck(suit: CardSuit.Spades);
        var gameManager = new GameManager(_cardRepoMock.Object, _getMock.Object);
        var players = new List<Guid>([Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()]);
        gameManager.GameInit(players);
        gameManager.CurPlayer.RemoveCard(gameManager.CurPlayer.CardsInHand[0].Id);
        gameManager.CurPlayer.RemoveCard(gameManager.CurPlayer.CardsInHand[0].Id);
        gameManager.EndTurn();
        var playerId = gameManager.CurPlayer.Id;
        gameManager.CurPlayer.ApplyDamage(3, new GameState(gameManager.LivePlayers,
            null!, gameManager.CurPlayer.Id, _getMock.Object));
        for (var i = 0; i < 5; ++i)
            gameManager.CurPlayer.RemoveCard(gameManager.CurPlayer.CardsInHand[0].Id);
        gameManager.CurPlayer.AddCardOnBoard(CardFactory.CreateCard(CardName.Dynamite, CardSuit.Clubs, CardRank.Ace));
        for (var i = 0; i < 3; ++i)
        {
            gameManager.EndTurn();
            gameManager.CurPlayer.RemoveCard(gameManager.CurPlayer.CardsInHand[0].Id);
            gameManager.CurPlayer.RemoveCard(gameManager.CurPlayer.CardsInHand[0].Id);
        }
        var rc = gameManager.EndTurn();
        Assert.Equal(CardRc.Ok, rc);
        Assert.Equal(players.Count - 1, gameManager.LivePlayers.Count);
        Assert.NotEqual(playerId, gameManager.CurPlayer.Id);
    }
    
    [Fact]
    public void EndTurn_DynamiteWithoutDeathTest()
    {
        FillDeck(suit: CardSuit.Spades);
        var gameManager = new GameManager(_cardRepoMock.Object, _getMock.Object);
        var players = new List<Guid>([Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()]);
        gameManager.GameInit(players);
        for (var i = 0; i < 5; ++i)
            gameManager.CurPlayer.RemoveCard(gameManager.CurPlayer.CardsInHand[0].Id);
        var playerId = gameManager.CurPlayer.Id;
        gameManager.CurPlayer.AddCardOnBoard(CardFactory.CreateCard(CardName.Dynamite, CardSuit.Clubs, CardRank.Ace));
        for (var i = 0; i < 3; ++i)
        {
            gameManager.EndTurn();
            gameManager.CurPlayer.RemoveCard(gameManager.CurPlayer.CardsInHand[0].Id);
            gameManager.CurPlayer.RemoveCard(gameManager.CurPlayer.CardsInHand[0].Id);
        }
        var rc = gameManager.EndTurn();
        Assert.Equal(CardRc.Ok, rc);
        Assert.Equal(players.Count, gameManager.LivePlayers.Count);
        Assert.Equal(playerId, gameManager.CurPlayer.Id);
    }
    
    [Fact]
    public void EndTurn_DynamiteReviveTest()
    {
        FillDeck(26, CardSuit.Spades);
        var gameManager = new GameManager(_cardRepoMock.Object, _getMock.Object);
        var players = new List<Guid>([Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()]);
        gameManager.GameInit(players);
        gameManager.CurPlayer.ApplyDamage(3, new GameState(gameManager.LivePlayers,
            null!, gameManager.CurPlayer.Id, _getMock.Object));
        for (var i = 0; i < 5; ++i)
            gameManager.CurPlayer.RemoveCard(gameManager.CurPlayer.CardsInHand[0].Id);
        var playerId = gameManager.CurPlayer.Id;
        gameManager.CurPlayer.AddCardOnBoard(CardFactory.CreateCard(CardName.Dynamite, CardSuit.Clubs, CardRank.Seven));
        gameManager.CurPlayer.AddCardOnBoard(CardFactory.CreateCard(CardName.BeerBarrel, CardSuit.Clubs, CardRank.Ace));
        for (var i = 0; i < 3; ++i)
        {
            gameManager.EndTurn();
            gameManager.CurPlayer.RemoveCard(gameManager.CurPlayer.CardsInHand[0].Id);
            gameManager.CurPlayer.RemoveCard(gameManager.CurPlayer.CardsInHand[0].Id);
        }
        gameManager.ForUnitTestWithDynamiteAndBeerBarrel();
        var rc = gameManager.EndTurn();
        Assert.Equal(CardRc.Ok, rc);
        Assert.Equal(players.Count, gameManager.LivePlayers.Count);
        Assert.Equal(playerId, gameManager.CurPlayer.Id);
        Assert.Equal(1, gameManager.CurPlayer.Health);
    }
    
    [Fact]
    public void EndTurn_BeerBarrelTest()
    {
        FillDeck(suit: CardSuit.Clubs);
        var gameManager = new GameManager(_cardRepoMock.Object, _getMock.Object);
        var players = new List<Guid>([Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()]);
        gameManager.GameInit(players);
        gameManager.CurPlayer.ApplyDamage(3, new GameState(gameManager.LivePlayers,
            null!, gameManager.CurPlayer.Id, _getMock.Object));
        for (var i = 0; i < 5; ++i)
            gameManager.CurPlayer.RemoveCard(gameManager.CurPlayer.CardsInHand[0].Id);
        gameManager.CurPlayer.AddCardOnBoard(CardFactory.CreateCard(CardName.BeerBarrel, CardSuit.Clubs, CardRank.Ace));
        for (var i = 0; i < 3; ++i)
        {
            gameManager.EndTurn();
            gameManager.CurPlayer.RemoveCard(gameManager.CurPlayer.CardsInHand[0].Id);
            gameManager.CurPlayer.RemoveCard(gameManager.CurPlayer.CardsInHand[0].Id);
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
        var players = new List<Guid>([Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()]);
        gameManager.GameInit(players);
        for (var i = 0; i < 2; ++i)
            gameManager.CurPlayer.RemoveCard(gameManager.CurPlayer.CardsInHand[0].Id);
        var playerId = gameManager.CurPlayer.Id;
        gameManager.CurPlayer.AddCardOnBoard(CardFactory.CreateCard(CardName.Jail, CardSuit.Clubs, CardRank.Ace));
        for (var i = 0; i < 3; ++i)
        {
            gameManager.EndTurn();
            gameManager.CurPlayer.RemoveCard(gameManager.CurPlayer.CardsInHand[0].Id);
            gameManager.CurPlayer.RemoveCard(gameManager.CurPlayer.CardsInHand[0].Id);
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
        var players = new List<Guid>([Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()]);
        gameManager.GameInit(players);
        foreach (var player in gameManager.Players.Where(p => p.Role is not PlayerRole.Sheriff and
                     not PlayerRole.DeputySheriff))
            player.ApplyDamage(4,
                new GameState(gameManager.LivePlayers, null!, gameManager.CurPlayer.Id, _getMock.Object));
        var rc = gameManager.CheckEndGame();
        Assert.Equal(CardRc.SheriffWin, rc);
        Assert.Equal(2, gameManager.LivePlayers.Count);
    }
    
    [Fact]
    public void CheckEndGame_RenegadeWinTest()
    {
        FillDeck();
        var gameManager = new GameManager(_cardRepoMock.Object, _getMock.Object);
        var players = new List<Guid>([Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()]);
        gameManager.GameInit(players);
        foreach (var player in gameManager.Players.Where(p => p.Role is not PlayerRole.Renegade))
            player.ApplyDamage(5,
                new GameState(gameManager.LivePlayers, null!, gameManager.CurPlayer.Id, _getMock.Object));
        var rc = gameManager.CheckEndGame();
        Assert.Equal(CardRc.RenegadeWin, rc);
        Assert.Single(gameManager.LivePlayers);
    }

    [Fact]
    public void TopDiscardedCard_Test()
    {
        FillDeck();
        var gameManager = new GameManager(_cardRepoMock.Object, _getMock.Object);
        var players = new List<Guid>([Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()]);
        gameManager.GameInit(players);
        var id = gameManager.CurPlayer.CardsInHand[0].Id;
        gameManager.DiscardCard(id);
        Assert.Equal(id, gameManager.TopDiscardedCard!.Id);
    }
}