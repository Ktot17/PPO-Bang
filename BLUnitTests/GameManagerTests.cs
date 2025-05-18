using BLComponent;
using BLComponent.InputPorts;
using Moq;

namespace BLUnitTests;

public class GameManagerTests
{
    private readonly Mock<ICardRepository> _cardRepoMock = new();
    private readonly Mock<ISaveRepository> _saveRepoMock = new();
    private readonly Mock<IGameView> _gameViewMock = new();

    private void FillDeck(int cardCount = 80, CardSuit suit = CardSuit.Diamonds)
    {
        _cardRepoMock.Setup(repo => repo.GetAll).Returns(() =>
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
        var gameManager = new GameManager(_cardRepoMock.Object, _saveRepoMock.Object, _gameViewMock.Object);
        var players = new List<string>(["1", "2", "3"]);
        
        Assert.Throws<WrongNumberOfPlayersException>(() => gameManager.GameInit(players));
        
        players = new List<string>(["1", "2", "3",
            "4", "5", "6",
            "7", "8", "9",
            "10", "11", "12"]);
        
        Assert.Throws<WrongNumberOfPlayersException>(() => gameManager.GameInit(players));
    }
    
    [Fact]
    public void GameInit_NotUniqueIdsTest()
    {
        FillDeck();
        var gameManager = new GameManager(_cardRepoMock.Object, _saveRepoMock.Object, _gameViewMock.Object);
        var players = new List<string>(["1", "1", "3", "2", "2", "4"]);
        
        Assert.Throws<NotUniqueNamesException>(() => gameManager.GameInit(players));
    }

    [Fact]
    public void GameInit_4RolesTests()
    {
        FillDeck();
        var gameManager = new GameManager(_cardRepoMock.Object, _saveRepoMock.Object, _gameViewMock.Object);
        var players = new List<string>(["1", "2", "3", "4"]);
        var expectedRoles = new List<PlayerRole> { PlayerRole.Outlaw,
            PlayerRole.Outlaw, PlayerRole.Renegade, PlayerRole.Sheriff };
        
        gameManager.GameInit(players);
        gameManager.GameStart();
        Assert.Equal(expectedRoles, gameManager.Players.Select(p => p.Role).Order());
        foreach (var player in gameManager.Players)
            Assert.Equal(player.Role == PlayerRole.Sheriff ? player.Health + 2 : player.Health, player.CardsInHand.Count);
    }

    [Fact]
    public void GameInit_5RolesTests()
    {
        FillDeck();
        var gameManager = new GameManager(_cardRepoMock.Object, _saveRepoMock.Object, _gameViewMock.Object);
        var players = new List<string>(["1", "2", "3", "4", "5"]);
        var expectedRoles = new List<PlayerRole> { PlayerRole.DeputySheriff, PlayerRole.Outlaw,
            PlayerRole.Outlaw, PlayerRole.Renegade, PlayerRole.Sheriff };
        
        gameManager.GameInit(players);
        gameManager.GameStart();
        Assert.Equal(expectedRoles, gameManager.Players.Select(p => p.Role).Order());
        foreach (var player in gameManager.Players)
            Assert.Equal(player.Role == PlayerRole.Sheriff ? player.Health + 2 : player.Health, player.CardsInHand.Count);
    }
    
    [Fact]
    public void GameInit_6RolesTests()
    {
        FillDeck();
        var gameManager = new GameManager(_cardRepoMock.Object, _saveRepoMock.Object, _gameViewMock.Object);
        var players = new List<string>(["1", "2",
            "3", "4", "5", "6"]);
        var expectedRoles = new List<PlayerRole> { PlayerRole.DeputySheriff, PlayerRole.Outlaw,
            PlayerRole.Outlaw, PlayerRole.Outlaw, PlayerRole.Renegade, PlayerRole.Sheriff };
        
        gameManager.GameInit(players);
        gameManager.GameStart();
        Assert.Equal(expectedRoles, gameManager.Players.Select(p => p.Role).Order());
        foreach (var player in gameManager.Players)
            Assert.Equal(player.Role == PlayerRole.Sheriff ? player.Health + 2 : player.Health, player.CardsInHand.Count);
    }
    
    [Fact]
    public void GameInit_7RolesTests()
    {
        FillDeck();
        var gameManager = new GameManager(_cardRepoMock.Object, _saveRepoMock.Object, _gameViewMock.Object);
        var players = new List<string>(["1", "2", "3",
            "4", "5", "6", "7"]);
        var expectedRoles = new List<PlayerRole> { PlayerRole.DeputySheriff, PlayerRole.DeputySheriff, PlayerRole.Outlaw,
            PlayerRole.Outlaw, PlayerRole.Outlaw, PlayerRole.Renegade, PlayerRole.Sheriff };
        
        gameManager.GameInit(players);
        gameManager.GameStart();
        Assert.Equal(expectedRoles, gameManager.Players.Select(p => p.Role).Order());
        foreach (var player in gameManager.Players)
            Assert.Equal(player.Role == PlayerRole.Sheriff ? player.Health + 2 : player.Health, player.CardsInHand.Count);
    }

    [Fact]
    public async Task PlayCard_NotInHandTest()
    {
        FillDeck();
        var gameManager = new GameManager(_cardRepoMock.Object, _saveRepoMock.Object, _gameViewMock.Object);
        var players = new List<string>(["1", "2", "3",
            "4", "5", "6", "7"]);
        gameManager.GameInit(players);
        await Assert.ThrowsAsync<NotExistingGuidException>(async () => await gameManager.PlayCard(Guid.NewGuid()));
    }

    [Fact]
    public async Task PlayCard_TooFarTest()
    {
        FillDeck();
        var gameManager = new GameManager(_cardRepoMock.Object, _saveRepoMock.Object, _gameViewMock.Object);
        var players = new List<string>(["1", "2", "3",
            "4", "5", "6", "7"]);
        gameManager.GameInit(players);
        gameManager.GameStart();
        var id = gameManager.CurPlayer.Id;
        var cardCount = gameManager.CurPlayer.CardsInHand.Count;
        _gameViewMock.Setup(get => get.GetPlayerIdAsync(gameManager.LivePlayers.Skip(1).ToList(),
            id)).Returns(Task.FromResult(gameManager.LivePlayers[2].Id));

        var rc = await gameManager.PlayCard(gameManager.CurPlayer.CardsInHand[0].Id);
        Assert.Equal(CardRc.TooFar, rc);
        Assert.Equal(cardCount, gameManager.CurPlayer.CardsInHand.Count);
        Assert.Equal(2, gameManager.GetRange(id, gameManager.LivePlayers[2].Id));
    }
    
    [Fact]
    public async Task PlayCard_CantPlayTest()
    {
        FillDeck();
        var gameManager = new GameManager(_cardRepoMock.Object, _saveRepoMock.Object, _gameViewMock.Object);
        var players = new List<string>(["1", "2", "3",
            "4", "5", "6", "7"]);
        gameManager.GameInit(players);
        gameManager.GameStart();
        var id = gameManager.CurPlayer.Id;
        var cardCount = gameManager.CurPlayer.CardsInHand.Count;
        _gameViewMock.Setup(get => get.GetPlayerIdAsync(gameManager.LivePlayers.Skip(1).ToList(),
            id)).Returns(Task.FromResult(gameManager.LivePlayers[1].Id));

        var rc = await gameManager.PlayCard(gameManager.CurPlayer.CardsInHand[0].Id);
        Assert.Equal(CardRc.Ok, rc);
        Assert.Equal(cardCount - 1, gameManager.CurPlayer.CardsInHand.Count);
        --cardCount;

        rc = await gameManager.PlayCard(gameManager.CurPlayer.CardsInHand[0].Id);
        Assert.Equal(CardRc.CantPlay, rc);
        Assert.Equal(cardCount, gameManager.CurPlayer.CardsInHand.Count);

        gameManager.CurPlayer.ChangeWeapon((WeaponCard)CardFactory.CreateCard(CardName.Volcanic, CardSuit.Clubs, CardRank.Ace),
            _gameViewMock.Object);
        
        rc = await gameManager.PlayCard(gameManager.CurPlayer.CardsInHand[0].Id);
        Assert.Equal(CardRc.Ok, rc);
        Assert.Equal(cardCount - 1, gameManager.CurPlayer.CardsInHand.Count);
    }
    
    [Fact]
    public async Task PlayCard_OutlawDeathTest()
    {
        FillDeck();
        var gameManager = new GameManager(_cardRepoMock.Object, _saveRepoMock.Object, _gameViewMock.Object);
        var players = new List<string>(["1", "2", "3",
            "4", "5", "6", "7"]);
        gameManager.GameInit(players);
        gameManager.GameStart();
        var cardCount = gameManager.CurPlayer.CardsInHand.Count;
        var id = gameManager.CurPlayer.Id;
        gameManager.CurPlayer.ChangeWeapon((WeaponCard)CardFactory.CreateCard(CardName.Winchester, CardSuit.Clubs, CardRank.Ace),
            _gameViewMock.Object);
        var outlawId = gameManager.LivePlayers.First(p => p.Role == PlayerRole.Outlaw).Id;
        _gameViewMock.Setup(get => get.GetPlayerIdAsync(gameManager.LivePlayers.Skip(1).ToList(), id)).
            Returns(Task.FromResult(outlawId));

        await gameManager.LivePlayers.First(p => p.Id == outlawId).ApplyDamage(3,
            new GameState(gameManager.LivePlayers, null!, outlawId, _gameViewMock.Object));
        var rc = await gameManager.PlayCard(gameManager.CurPlayer.CardsInHand[0].Id);
        Assert.Equal(CardRc.Ok, rc);
        Assert.Equal(players.Count - 1, gameManager.LivePlayers.Count);
        Assert.Equal(cardCount + 2, gameManager.CurPlayer.CardsInHand.Count);
        Assert.Equal(id, gameManager.CurPlayer.Id);
    }
    
    [Fact]
    public async Task PlayCard_RenegadeDeathTest()
    {
        FillDeck();
        var gameManager = new GameManager(_cardRepoMock.Object, _saveRepoMock.Object, _gameViewMock.Object);
        var players = new List<string>(["1", "2", "3",
            "4", "5", "6", "7"]);
        gameManager.GameInit(players);
        gameManager.GameStart();
        var cardCount = gameManager.CurPlayer.CardsInHand.Count;
        var id = gameManager.CurPlayer.Id;
        gameManager.CurPlayer.ChangeWeapon((WeaponCard)CardFactory.CreateCard(CardName.Winchester, CardSuit.Clubs, CardRank.Ace),
            _gameViewMock.Object);
        var renegadeId = gameManager.LivePlayers.First(p => p.Role == PlayerRole.Renegade).Id;
        _gameViewMock.Setup(get => get.GetPlayerIdAsync(gameManager.LivePlayers.Skip(1).ToList(), id)).
            Returns(Task.FromResult(renegadeId));

        await gameManager.LivePlayers.First(p => p.Id == renegadeId).ApplyDamage(3,
            new GameState(gameManager.LivePlayers, null!, renegadeId, _gameViewMock.Object));
        var rc = await gameManager.PlayCard(gameManager.CurPlayer.CardsInHand[0].Id);
        Assert.Equal(CardRc.Ok, rc);
        Assert.Equal(players.Count - 1, gameManager.LivePlayers.Count);
        Assert.Equal(cardCount - 1, gameManager.CurPlayer.CardsInHand.Count);
        Assert.Equal(id, gameManager.CurPlayer.Id);
    }
    
    [Fact]
    public async Task PlayCard_DeputyDeathTest()
    {
        FillDeck();
        var gameManager = new GameManager(_cardRepoMock.Object, _saveRepoMock.Object, _gameViewMock.Object);
        var players = new List<string>(["1", "2", "3",
            "4", "5", "6", "7"]);
        gameManager.GameInit(players);
        gameManager.GameStart();
        var id = gameManager.CurPlayer.Id;
        gameManager.CurPlayer.ChangeWeapon((WeaponCard)CardFactory.CreateCard(CardName.Winchester, CardSuit.Clubs, CardRank.Ace),
            _gameViewMock.Object);
        var deputyId = gameManager.LivePlayers.First(p => p.Role == PlayerRole.DeputySheriff).Id;
        _gameViewMock.Setup(get => get.GetPlayerIdAsync(gameManager.LivePlayers.Skip(1).ToList(), id)).
            Returns(Task.FromResult(deputyId));

        await gameManager.LivePlayers.First(p => p.Id == deputyId).ApplyDamage(3,
            new GameState(gameManager.LivePlayers, null!, deputyId, _gameViewMock.Object));
        var rc = await gameManager.PlayCard(gameManager.CurPlayer.CardsInHand[0].Id);
        Assert.Equal(CardRc.Ok, rc);
        Assert.Equal(players.Count - 1, gameManager.LivePlayers.Count);
        Assert.Empty(gameManager.CurPlayer.CardsInHand);
        Assert.Equal(id, gameManager.CurPlayer.Id);
    }
    
    [Fact]
    public async Task PlayCard_IndiansTest()
    {
        FillDeck();
        var gameManager = new GameManager(_cardRepoMock.Object, _saveRepoMock.Object, _gameViewMock.Object);
        var players = new List<string>(["1", "2", "3",
            "4", "5", "6", "7"]);
        gameManager.GameInit(players);
        var id = gameManager.CurPlayer.Id;
        var indians = CardFactory.CreateCard(CardName.Indians, CardSuit.Clubs, CardRank.Ace);
        gameManager.CurPlayer.AddCardInHand(indians, _gameViewMock.Object);
        var deputy = gameManager.LivePlayers.First(p => p.Role == PlayerRole.DeputySheriff);
        _gameViewMock.Setup(get => get.GetPlayerIdAsync(gameManager.LivePlayers, id)).
            Returns(Task.FromResult(deputy.Id));

        await deputy.ApplyDamage(3, new GameState(gameManager.LivePlayers, null!, deputy.Id, _gameViewMock.Object));
        var deputyCardCount = deputy.CardsInHand.Count;
        for (var i = 0; i < deputyCardCount; ++i)
            deputy.RemoveCard(deputy.CardsInHand[0].Id);
        deputy.AddCardOnBoard(CardFactory.CreateCard(CardName.Scope, CardSuit.Clubs, CardRank.Ace),
            _gameViewMock.Object);
        var cardCount = gameManager.CurPlayer.CardsInHand.Count;
        var rc = await gameManager.PlayCard(indians.Id);
        Assert.Equal(CardRc.Ok, rc);
        Assert.Equal(players.Count - 1, gameManager.LivePlayers.Count);
        Assert.Equal(cardCount - 1, gameManager.CurPlayer.CardsInHand.Count);
        Assert.Equal(id, gameManager.CurPlayer.Id);
    }
    
    [Fact]
    public async Task PlayCard_DuelTest()
    {
        FillDeck();
        var gameManager = new GameManager(_cardRepoMock.Object, _saveRepoMock.Object, _gameViewMock.Object);
        var players = new List<string>(["1", "2", "3",
            "4", "5", "6", "7"]);
        gameManager.GameInit(players);
        var id = gameManager.CurPlayer.Id;
        var duel = CardFactory.CreateCard(CardName.Duel, CardSuit.Clubs, CardRank.Ace);
        gameManager.CurPlayer.AddCardInHand(CardFactory.CreateCard(CardName.Bang, CardSuit.Clubs, CardRank.Ace),
            _gameViewMock.Object);
        gameManager.CurPlayer.AddCardInHand(CardFactory.CreateCard(CardName.Bang, CardSuit.Clubs, CardRank.Ace),
            _gameViewMock.Object);
        gameManager.CurPlayer.AddCardInHand(duel, _gameViewMock.Object);
        gameManager.CurPlayer.AddCardInHand(CardFactory.CreateCard(CardName.Bang, CardSuit.Clubs, CardRank.Ace),
            _gameViewMock.Object);
        gameManager.CurPlayer.AddCardInHand(CardFactory.CreateCard(CardName.Bang, CardSuit.Clubs, CardRank.Ace),
            _gameViewMock.Object);
        var deputy = gameManager.LivePlayers.First(p => p.Role == PlayerRole.DeputySheriff);
        _gameViewMock.Setup(get => get.GetPlayerIdAsync(gameManager.LivePlayers.Skip(1).ToList(), id)).
            Returns(Task.FromResult(deputy.Id));
        _gameViewMock.Setup(get => get.YesOrNoAsync(It.IsAny<Guid>(), It.IsAny<CardName>())).
            Returns(Task.FromResult(true));
        
        var deputyCardCount = deputy.CardsInHand.Count;
        for (var i = 0; i < deputyCardCount; ++i)
            deputy.RemoveCard(deputy.CardsInHand[0].Id);
        deputy.AddCardInHand(CardFactory.CreateCard(CardName.Bang, CardSuit.Clubs, CardRank.Ace), _gameViewMock.Object);
        var cardCount = gameManager.CurPlayer.CardsInHand.Count;
        var rc = await gameManager.PlayCard(duel.Id);
        Assert.Equal(CardRc.Ok, rc);
        Assert.Equal(players.Count, gameManager.LivePlayers.Count);
        Assert.Equal(cardCount - 2, gameManager.CurPlayer.CardsInHand.Count);
        Assert.Equal(id, gameManager.CurPlayer.Id);
    }

    [Fact]
    public async Task PlayCard_LastPlayerKillsPlayerTest()
    {
        FillDeck();
        var gameManager = new GameManager(_cardRepoMock.Object, _saveRepoMock.Object, _gameViewMock.Object);
        var players = new List<string>(["1", "2", "3",
            "4", "5", "6", "7"]);
        gameManager.GameInit(players);
        gameManager.GameStart();
        var sheriffId = gameManager.CurPlayer.Id;
        await gameManager.LivePlayers[0].ApplyDamage(4, null!);
        gameManager.LivePlayers[0].RemoveCard(gameManager.LivePlayers[0].CardsInHand[0].Id);
        gameManager.LivePlayers[0].RemoveCard(gameManager.LivePlayers[0].CardsInHand[0].Id);
        gameManager.LivePlayers[0].RemoveCard(gameManager.LivePlayers[0].CardsInHand[0].Id);
        gameManager.LivePlayers[0].RemoveCard(gameManager.LivePlayers[0].CardsInHand[0].Id);
        for (var i = 0; i < players.Count - 1; ++i)
        {
            gameManager.LivePlayers[i].RemoveCard(gameManager.LivePlayers[i].CardsInHand[0].Id);
            gameManager.LivePlayers[i].RemoveCard(gameManager.LivePlayers[i].CardsInHand[0].Id);
            await gameManager.EndTurn();
        }

        var id = gameManager.CurPlayer.Id;
        _gameViewMock.Setup(get => get.GetPlayerIdAsync(gameManager.LivePlayers.SkipLast(1).ToList(), id)).
            Returns(Task.FromResult(sheriffId));
        var cardCount = gameManager.CurPlayer.CardsInHand.Count;
        var rc = await gameManager.PlayCard(gameManager.CurPlayer.CardsInHand[0].Id);
        Assert.Equal(CardRc.OutlawWin, rc);
        Assert.Equal(players.Count - 1, gameManager.LivePlayers.Count);
        Assert.Equal(cardCount - 1, gameManager.CurPlayer.CardsInHand.Count);
        Assert.Equal(id, gameManager.CurPlayer.Id);
    }

    [Fact]
    public void DiscardCard_NotInHandTest()
    {
        FillDeck();
        var gameManager = new GameManager(_cardRepoMock.Object, _saveRepoMock.Object, _gameViewMock.Object);
        var players = new List<string>(["1", "2", "3",
            "4", "5", "6", "7"]);
        gameManager.GameInit(players);
        Assert.Throws<NotExistingGuidException>(() => gameManager.DiscardCard(Guid.NewGuid()));
    }

    [Fact]
    public void DiscardCard_Test()
    {
        FillDeck();
        var gameManager = new GameManager(_cardRepoMock.Object, _saveRepoMock.Object, _gameViewMock.Object);
        var players = new List<string>(["1", "2", "3",
            "4", "5", "6", "7"]);
        gameManager.GameInit(players);
        gameManager.GameStart();
        var cardCount = gameManager.CurPlayer.CardsInHand.Count;
        gameManager.DiscardCard(gameManager.CurPlayer.CardsInHand[0].Id);
        Assert.Equal(cardCount - 1, gameManager.CurPlayer.CardsInHand.Count);
    }

    [Fact]
    public async Task EndTurn_TooMuchCardsTest()
    {
        FillDeck();
        var gameManager = new GameManager(_cardRepoMock.Object, _saveRepoMock.Object, _gameViewMock.Object);
        var players = new List<string>(["1", "2", "3",
            "4", "5", "6", "7"]);
        gameManager.GameInit(players);
        gameManager.GameStart();
        var playerId = gameManager.CurPlayer.Id;
        var rc = await gameManager.EndTurn();
        Assert.Equal(CardRc.CantEndTurn, rc);
        Assert.Equal(playerId, gameManager.CurPlayer.Id);
    }
    
    [Fact]
    public async Task EndTurn_NoCardsOnBoardTest()
    {
        FillDeck();
        var gameManager = new GameManager(_cardRepoMock.Object, _saveRepoMock.Object, _gameViewMock.Object);
        var players = new List<string>(["1", "2", "3",
            "4", "5", "6", "7"]);
        gameManager.GameInit(players);
        gameManager.GameStart();
        var playerId = gameManager.LivePlayers[1].Id;
        var playerCardCount = gameManager.LivePlayers[1].CardsInHand.Count;
        gameManager.CurPlayer.RemoveCard(gameManager.CurPlayer.CardsInHand[0].Id);
        gameManager.CurPlayer.RemoveCard(gameManager.CurPlayer.CardsInHand[0].Id);
        var rc = await gameManager.EndTurn();
        Assert.Equal(CardRc.Ok, rc);
        Assert.Equal(playerId, gameManager.CurPlayer.Id);
        Assert.Equal(playerCardCount + 2, gameManager.CurPlayer.CardsInHand.Count);
    }
    
    [Fact]
    public async Task EndTurn_DynamiteSheriffDeathTest()
    {
        FillDeck(suit: CardSuit.Spades);
        var gameManager = new GameManager(_cardRepoMock.Object, _saveRepoMock.Object, _gameViewMock.Object);
        var players = new List<string>(["1", "2", "3", "4"]);
        gameManager.GameInit(players);
        gameManager.GameStart();
        await gameManager.CurPlayer.ApplyDamage(3, new GameState(gameManager.LivePlayers,
            null!, gameManager.CurPlayer.Id, _gameViewMock.Object));
        for (var i = 0; i < 5; ++i)
            gameManager.CurPlayer.RemoveCard(gameManager.CurPlayer.CardsInHand[0].Id);
        gameManager.CurPlayer.AddCardOnBoard(CardFactory.CreateCard(CardName.Dynamite, CardSuit.Clubs, CardRank.Ace),
            _gameViewMock.Object);
        for (var i = 0; i < 3; ++i)
        {
            await gameManager.EndTurn();
            gameManager.CurPlayer.RemoveCard(gameManager.CurPlayer.CardsInHand[0].Id);
            gameManager.CurPlayer.RemoveCard(gameManager.CurPlayer.CardsInHand[0].Id);
        }
        var rc = await gameManager.EndTurn();
        Assert.Equal(CardRc.OutlawWin, rc);
    }
    
    [Fact]
    public async Task EndTurn_DynamiteDeathTest()
    {
        FillDeck(suit: CardSuit.Spades);
        var gameManager = new GameManager(_cardRepoMock.Object, _saveRepoMock.Object, _gameViewMock.Object);
        var players = new List<string>(["1", "2", "3", "4"]);
        gameManager.GameInit(players);
        gameManager.GameStart();
        gameManager.CurPlayer.RemoveCard(gameManager.CurPlayer.CardsInHand[0].Id);
        gameManager.CurPlayer.RemoveCard(gameManager.CurPlayer.CardsInHand[0].Id);
        await gameManager.EndTurn();
        var playerId = gameManager.CurPlayer.Id;
        await gameManager.CurPlayer.ApplyDamage(3, new GameState(gameManager.LivePlayers,
            null!, gameManager.CurPlayer.Id, _gameViewMock.Object));
        for (var i = 0; i < 5; ++i)
            gameManager.CurPlayer.RemoveCard(gameManager.CurPlayer.CardsInHand[0].Id);
        gameManager.CurPlayer.AddCardOnBoard(CardFactory.CreateCard(CardName.Dynamite, CardSuit.Clubs, CardRank.Ace),
            _gameViewMock.Object);
        for (var i = 0; i < 3; ++i)
        {
            await gameManager.EndTurn();
            gameManager.CurPlayer.RemoveCard(gameManager.CurPlayer.CardsInHand[0].Id);
            gameManager.CurPlayer.RemoveCard(gameManager.CurPlayer.CardsInHand[0].Id);
        }
        var rc = await gameManager.EndTurn();
        Assert.Equal(CardRc.Ok, rc);
        Assert.Equal(players.Count - 1, gameManager.LivePlayers.Count);
        Assert.NotEqual(playerId, gameManager.CurPlayer.Id);
    }
    
    [Fact]
    public async Task EndTurn_DynamiteWithoutDeathTest()
    {
        FillDeck(suit: CardSuit.Spades);
        var gameManager = new GameManager(_cardRepoMock.Object, _saveRepoMock.Object, _gameViewMock.Object);
        var players = new List<string>(["1", "2", "3", "4"]);
        gameManager.GameInit(players);
        gameManager.GameStart();
        for (var i = 0; i < 5; ++i)
            gameManager.CurPlayer.RemoveCard(gameManager.CurPlayer.CardsInHand[0].Id);
        var playerId = gameManager.CurPlayer.Id;
        gameManager.CurPlayer.AddCardOnBoard(CardFactory.CreateCard(CardName.Dynamite, CardSuit.Clubs, CardRank.Ace),
            _gameViewMock.Object);
        for (var i = 0; i < 3; ++i)
        {
            await gameManager.EndTurn();
            gameManager.CurPlayer.RemoveCard(gameManager.CurPlayer.CardsInHand[0].Id);
            gameManager.CurPlayer.RemoveCard(gameManager.CurPlayer.CardsInHand[0].Id);
        }
        var rc = await gameManager.EndTurn();
        Assert.Equal(CardRc.Ok, rc);
        Assert.Equal(players.Count, gameManager.LivePlayers.Count);
        Assert.Equal(playerId, gameManager.CurPlayer.Id);
    }
    
    [Fact]
    public async Task EndTurn_DynamiteReviveTest()
    {
        FillDeck(26, CardSuit.Spades);
        var gameManager = new GameManager(_cardRepoMock.Object, _saveRepoMock.Object, _gameViewMock.Object);
        var players = new List<string>(["1", "2", "3", "4"]);
        gameManager.GameInit(players);
        gameManager.GameStart();
        await gameManager.CurPlayer.ApplyDamage(3, new GameState(gameManager.LivePlayers,
            null!, gameManager.CurPlayer.Id, _gameViewMock.Object));
        for (var i = 0; i < 5; ++i)
            gameManager.CurPlayer.RemoveCard(gameManager.CurPlayer.CardsInHand[0].Id);
        var playerId = gameManager.CurPlayer.Id;
        gameManager.CurPlayer.AddCardOnBoard(CardFactory.CreateCard(CardName.Dynamite, CardSuit.Clubs, CardRank.Seven),
            _gameViewMock.Object);
        gameManager.CurPlayer.AddCardOnBoard(CardFactory.CreateCard(CardName.BeerBarrel, CardSuit.Clubs, CardRank.Ace),
            _gameViewMock.Object);
        for (var i = 0; i < 3; ++i)
        {
            await gameManager.EndTurn();
            gameManager.CurPlayer.RemoveCard(gameManager.CurPlayer.CardsInHand[0].Id);
            gameManager.CurPlayer.RemoveCard(gameManager.CurPlayer.CardsInHand[0].Id);
        }
        gameManager.ForUnitTestWithDynamiteAndBeerBarrel();
        var rc = await gameManager.EndTurn();
        Assert.Equal(CardRc.Ok, rc);
        Assert.Equal(players.Count, gameManager.LivePlayers.Count);
        Assert.Equal(playerId, gameManager.CurPlayer.Id);
        Assert.Equal(1, gameManager.CurPlayer.Health);
    }
    
    [Fact]
    public async Task EndTurn_BeerBarrelTest()
    {
        FillDeck(suit: CardSuit.Clubs);
        var gameManager = new GameManager(_cardRepoMock.Object, _saveRepoMock.Object, _gameViewMock.Object);
        var players = new List<string>(["1", "2", "3",
            "4", "5", "6", "7"]);
        gameManager.GameInit(players);
        gameManager.GameStart();
        await gameManager.CurPlayer.ApplyDamage(3, new GameState(gameManager.LivePlayers,
            null!, gameManager.CurPlayer.Id, _gameViewMock.Object));
        for (var i = 0; i < 5; ++i)
            gameManager.CurPlayer.RemoveCard(gameManager.CurPlayer.CardsInHand[0].Id);
        gameManager.CurPlayer.AddCardOnBoard(CardFactory.CreateCard(CardName.BeerBarrel, CardSuit.Clubs, CardRank.Ace),
            _gameViewMock.Object);
        for (var i = 0; i < 3; ++i)
        {
            await gameManager.EndTurn();
            gameManager.CurPlayer.RemoveCard(gameManager.CurPlayer.CardsInHand[0].Id);
            gameManager.CurPlayer.RemoveCard(gameManager.CurPlayer.CardsInHand[0].Id);
        }
        var rc = await gameManager.EndTurn();
        Assert.Equal(CardRc.Ok, rc);
        Assert.Equal(4, gameManager.CurPlayer.Health);
    }
    
    [Fact]
    public async Task EndTurn_JailTest()
    {
        FillDeck();
        var gameManager = new GameManager(_cardRepoMock.Object, _saveRepoMock.Object, _gameViewMock.Object);
        var players = new List<string>(["1", "2", "3",
            "4", "5", "6", "7"]);
        gameManager.GameInit(players);
        gameManager.GameStart();
        for (var i = 0; i < 2; ++i)
            gameManager.CurPlayer.RemoveCard(gameManager.CurPlayer.CardsInHand[0].Id);
        var playerId = gameManager.CurPlayer.Id;
        gameManager.CurPlayer.AddCardOnBoard(CardFactory.CreateCard(CardName.Jail, CardSuit.Clubs, CardRank.Ace),
            _gameViewMock.Object);
        for (var i = 0; i < 3; ++i)
        {
            await gameManager.EndTurn();
            gameManager.CurPlayer.RemoveCard(gameManager.CurPlayer.CardsInHand[0].Id);
            gameManager.CurPlayer.RemoveCard(gameManager.CurPlayer.CardsInHand[0].Id);
        }
        var rc = await gameManager.EndTurn();
        Assert.Equal(CardRc.Ok, rc);
        Assert.NotEqual(playerId, gameManager.CurPlayer.Id);
    }

    [Fact]
    public async Task CheckEndGame_SheriffWinTest()
    {
        FillDeck();
        var gameManager = new GameManager(_cardRepoMock.Object, _saveRepoMock.Object, _gameViewMock.Object);
        var players = new List<string>(["1", "2", "3", "4", "5"]);
        gameManager.GameInit(players);
        foreach (var player in gameManager.Players.Where(p => p.Role is not PlayerRole.Sheriff and
                     not PlayerRole.DeputySheriff))
            await player.ApplyDamage(4,
                new GameState(gameManager.LivePlayers, null!, gameManager.CurPlayer.Id, _gameViewMock.Object));
        var rc = gameManager.CheckEndGame();
        Assert.Equal(CardRc.SheriffWin, rc);
        Assert.Equal(2, gameManager.LivePlayers.Count);
    }
    
    [Fact]
    public async Task CheckEndGame_RenegadeWinTest()
    {
        FillDeck();
        var gameManager = new GameManager(_cardRepoMock.Object, _saveRepoMock.Object, _gameViewMock.Object);
        var players = new List<string>(["1", "2", "3",
            "4", "5", "6", "7"]);
        gameManager.GameInit(players);
        foreach (var player in gameManager.Players.Where(p => p.Role is not PlayerRole.Renegade))
            await player.ApplyDamage(5,
                new GameState(gameManager.LivePlayers, null!, gameManager.CurPlayer.Id, _gameViewMock.Object));
        var rc = gameManager.CheckEndGame();
        Assert.Equal(CardRc.RenegadeWin, rc);
        Assert.Single(gameManager.LivePlayers);
    }

    [Fact]
    public void TopDiscardedCard_Test()
    {
        FillDeck();
        var gameManager = new GameManager(_cardRepoMock.Object, _saveRepoMock.Object, _gameViewMock.Object);
        var players = new List<string>(["1", "2", "3",
            "4", "5", "6", "7"]);
        gameManager.GameInit(players);
        gameManager.GameStart();
        var id = gameManager.CurPlayer.CardsInHand[0].Id;
        gameManager.DiscardCard(id);
        Assert.Equal(id, gameManager.TopDiscardedCard!.Id);
    }
}
