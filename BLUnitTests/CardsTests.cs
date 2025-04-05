using BLComponent;
using BLComponent.Cards;
using BLComponent.InputPorts;
using Moq;

namespace BLUnitTests;

public class CardsTests
{
    private readonly Mock<ICardRepository> _cardRepoMock = new();
    private readonly Mock<IGet> _getMock = new();

    [Fact]
    public void Play_ManyBangsTest()
    {
        var bang = (Bang)CardFactory.CreateCard(CardName.Bang, CardSuit.Diamonds, CardRank.Ace);
        var players = new List<Player>([
            new Player(0, PlayerRole.Sheriff, 5),
            new Player(1, PlayerRole.Outlaw, 4),
            new Player(2, PlayerRole.Outlaw, 4),
        ]);
        _getMock.Setup(get => get.GetPlayerIndex(players, 0)).Returns(1);
        var rc = bang.Play(new GameContext(players, null!, 0, _getMock.Object));
        Assert.Equal(CardRc.Ok, rc);
        rc = bang.Play(new GameContext(players, null!, 0, _getMock.Object));
        Assert.Equal(CardRc.CantPlay, rc);
        rc = bang.Play(new GameContext(players, null!, 0, _getMock.Object));
        Assert.Equal(CardRc.CantPlay, rc);
    }
    
    [Fact]
    public void Play_ManyBangsWithVolcanicTest()
    {
        var bang = (Bang)CardFactory.CreateCard(CardName.Bang, CardSuit.Diamonds, CardRank.Ace);
        var players = new List<Player>([
            new Player(0, PlayerRole.Sheriff, 5),
            new Player(1, PlayerRole.Outlaw, 4),
            new Player(2, PlayerRole.Outlaw, 4),
        ]);
        _getMock.Setup(get => get.GetPlayerIndex(players, 0)).Returns(1);
        players[0].ChangeWeapon((WeaponCard)CardFactory.CreateCard(CardName.Volcanic, CardSuit.Clubs, CardRank.Ace));
        var rc = bang.Play(new GameContext(players, null!, 0, _getMock.Object));
        Assert.Equal(CardRc.Ok, rc);
        rc = bang.Play(new GameContext(players, null!, 0, _getMock.Object));
        Assert.Equal(CardRc.Ok, rc);
        rc = bang.Play(new GameContext(players, null!, 0, _getMock.Object));
        Assert.Equal(CardRc.Ok, rc);
        rc = bang.Play(new GameContext(players, null!, 0, _getMock.Object));
        Assert.Equal(CardRc.Ok, rc);
    }
    
    [Fact]
    public void Play_BangTooFarTest()
    {
        var bang = (Bang)CardFactory.CreateCard(CardName.Bang, CardSuit.Diamonds, CardRank.Ace);
        var players = new List<Player>([
            new Player(0, PlayerRole.Sheriff, 5),
            new Player(1, PlayerRole.Outlaw, 4),
            new Player(2, PlayerRole.Outlaw, 4),
            new Player(3, PlayerRole.Outlaw, 4),
        ]);
        _getMock.Setup(get => get.GetPlayerIndex(players, 0)).Returns(2);
        var rc = bang.Play(new GameContext(players, null!, 0, _getMock.Object));
        Assert.Equal(CardRc.TooFar, rc);
    }

    [Fact]
    public void Play_MissedTest()
    {
        var missed = (Missed)CardFactory.CreateCard(CardName.Missed, CardSuit.Diamonds, CardRank.Ace);
        var rc = missed.Play(null!);
        Assert.Equal(CardRc.CantPlay, rc);
    }

    [Fact]
    public void Play_BarrelOkTest()
    {
        var bang = (Bang)CardFactory.CreateCard(CardName.Bang, CardSuit.Diamonds, CardRank.Ace);
        var barrel = (Barrel)CardFactory.CreateCard(CardName.Barrel, CardSuit.Diamonds, CardRank.Ace);
        var players = new List<Player>([
            new Player(0, PlayerRole.Sheriff, 5),
            new Player(1, PlayerRole.Outlaw, 4),
            new Player(2, PlayerRole.Outlaw, 4),
            new Player(3, PlayerRole.Outlaw, 4),
        ]);
        _cardRepoMock.Setup(repo => repo.GetAll()).Returns([
            CardFactory.CreateCard(CardName.Bang, CardSuit.Hearts, CardRank.Ace)
        ]);
        _getMock.Setup(get => get.GetPlayerIndex(players, 0)).Returns(1);
        var deck = new Deck(_cardRepoMock.Object);
        players[1].AddCardOnBoard(barrel);
        var rc = bang.Play(new GameContext(players, deck, 0, _getMock.Object));
        Assert.Equal(CardRc.Ok, rc);
        Assert.Equal(4, players[1].Health);
        Assert.Empty(players[1].CardsOnBoard);
    }
    
    [Fact]
    public void Play_BarrelNotOkTest()
    {
        var bang = (Bang)CardFactory.CreateCard(CardName.Bang, CardSuit.Diamonds, CardRank.Ace);
        var barrel = (Barrel)CardFactory.CreateCard(CardName.Barrel, CardSuit.Diamonds, CardRank.Ace);
        var players = new List<Player>([
            new Player(0, PlayerRole.Sheriff, 5),
            new Player(1, PlayerRole.Outlaw, 4),
            new Player(2, PlayerRole.Outlaw, 4),
            new Player(3, PlayerRole.Outlaw, 4),
        ]);
        _cardRepoMock.Setup(repo => repo.GetAll()).Returns([
            CardFactory.CreateCard(CardName.Bang, CardSuit.Spades, CardRank.Ace)
        ]);
        _getMock.Setup(get => get.GetPlayerIndex(players, 0)).Returns(1);
        var deck = new Deck(_cardRepoMock.Object);
        players[1].AddCardOnBoard(barrel);
        var rc = bang.Play(new GameContext(players, deck, 0, _getMock.Object));
        Assert.Equal(CardRc.Ok, rc);
        Assert.Equal(3, players[1].Health);
        Assert.NotEmpty(players[1].CardsOnBoard);
    }
    
    [Fact]
    public void Play_BangMissedTest()
    {
        var bang = (Bang)CardFactory.CreateCard(CardName.Bang, CardSuit.Diamonds, CardRank.Ace);
        var missed = (Missed)CardFactory.CreateCard(CardName.Missed, CardSuit.Diamonds, CardRank.Ace);
        var players = new List<Player>([
            new Player(0, PlayerRole.Sheriff, 5),
            new Player(1, PlayerRole.Outlaw, 4),
            new Player(2, PlayerRole.Outlaw, 4),
            new Player(3, PlayerRole.Outlaw, 4),
        ]);
        _cardRepoMock.Setup(repo => repo.GetAll()).Returns([
            CardFactory.CreateCard(CardName.Bang, CardSuit.Spades, CardRank.Ace)
        ]);
        _getMock.Setup(get => get.GetPlayerIndex(players, 0)).Returns(1);
        var deck = new Deck(_cardRepoMock.Object);
        players[1].AddCardInHand(missed);
        var rc = bang.Play(new GameContext(players, deck, 0, _getMock.Object));
        Assert.Equal(CardRc.Ok, rc);
        Assert.Equal(4, players[1].Health);
        Assert.Empty(players[1].CardsInHand);
    }
    
    [Fact]
    public void Play_BeerMaxHealthTest()
    {
        var beer = (Beer)CardFactory.CreateCard(CardName.Beer, CardSuit.Diamonds, CardRank.Ace);
        var players = new List<Player>([
            new Player(0, PlayerRole.Sheriff, 5),
            new Player(1, PlayerRole.Outlaw, 4),
            new Player(2, PlayerRole.Outlaw, 4),
            new Player(3, PlayerRole.Outlaw, 4),
        ]);
        var rc = beer.Play(new GameContext(players, null!, 0, _getMock.Object));
        Assert.Equal(CardRc.CantPlay, rc);
        Assert.Equal(5, players[0].Health);
    }
    
    [Fact]
    public void Play_BeerTwoPlayersTest()
    {
        var beer = (Beer)CardFactory.CreateCard(CardName.Beer, CardSuit.Diamonds, CardRank.Ace);
        var players = new List<Player>([
            new Player(0, PlayerRole.Sheriff, 5),
            new Player(1, PlayerRole.Outlaw, 4),
        ]);
        players[0].ApplyDamage(1, new GameContext(players, null!, 0, _getMock.Object));
        var rc = beer.Play(new GameContext(players, null!, 0, _getMock.Object));
        Assert.Equal(CardRc.CantPlay, rc);
        Assert.Equal(4, players[0].Health);
    }
    
    [Fact]
    public void Play_BeerOkTest()
    {
        var beer = (Beer)CardFactory.CreateCard(CardName.Beer, CardSuit.Diamonds, CardRank.Ace);
        var players = new List<Player>([
            new Player(0, PlayerRole.Sheriff, 5),
            new Player(1, PlayerRole.Outlaw, 4),
            new Player(2, PlayerRole.Outlaw, 4),
            new Player(3, PlayerRole.Outlaw, 4),
        ]);
        players[0].ApplyDamage(1, new GameContext(players, null!, 0, _getMock.Object));
        var rc = beer.Play(new GameContext(players, null!, 0, _getMock.Object));
        Assert.Equal(CardRc.Ok, rc);
        Assert.Equal(5, players[0].Health);
    }

    [Fact]
    public void Play_PanicTooFarTest()
    {
        var panic = (Panic)CardFactory.CreateCard(CardName.Panic, CardSuit.Spades, CardRank.Ace);
        var players = new List<Player>([
            new Player(0, PlayerRole.Sheriff, 5),
            new Player(1, PlayerRole.Outlaw, 4),
            new Player(2, PlayerRole.Outlaw, 4),
            new Player(3, PlayerRole.Outlaw, 4),
        ]);
        _getMock.Setup(get => get.GetPlayerIndex(players, 0)).Returns(2);
        var rc = panic.Play(new GameContext(players, null!, 0, _getMock.Object));
        Assert.Equal(CardRc.TooFar, rc);
    }
    
    [Fact]
    public void Play_PanicCantPlayTest()
    {
        var panic = (Panic)CardFactory.CreateCard(CardName.Panic, CardSuit.Spades, CardRank.Ace);
        var players = new List<Player>([
            new Player(0, PlayerRole.Sheriff, 5),
            new Player(1, PlayerRole.Outlaw, 4),
            new Player(2, PlayerRole.Outlaw, 4),
            new Player(3, PlayerRole.Outlaw, 4),
        ]);
        _getMock.Setup(get => get.GetPlayerIndex(players, 0)).Returns(1);
        var rc = panic.Play(new GameContext(players, null!, 0, _getMock.Object));
        Assert.Equal(CardRc.CantPlay, rc);
    }
    
    [Fact]
    public void Play_PanicOkTest()
    {
        var panic = (Panic)CardFactory.CreateCard(CardName.Panic, CardSuit.Spades, CardRank.Ace);
        var players = new List<Player>([
            new Player(0, PlayerRole.Sheriff, 5),
            new Player(1, PlayerRole.Outlaw, 4),
            new Player(2, PlayerRole.Outlaw, 4),
            new Player(3, PlayerRole.Outlaw, 4),
        ]);
        _getMock.Setup(get => get.GetPlayerIndex(players, 0)).Returns(1);
        players[1].AddCardInHand(CardFactory.CreateCard(CardName.Bang, CardSuit.Clubs, CardRank.Ace));
        _getMock.Setup(get => get.GetCardIndex(players[1].CardsInHand, 0, null)).Returns(0);
        var rc = panic.Play(new GameContext(players, null!, 0, _getMock.Object));
        Assert.Equal(CardRc.Ok, rc);
        Assert.Empty(players[1].CardsInHand);
        Assert.Single(players[0].CardsInHand);
    }

    [Fact]
    public void Play_GeneralStoreOkTest()
    {
        var generalStore = (GeneralStore)CardFactory.CreateCard(CardName.GeneralStore, CardSuit.Clubs, CardRank.Ace);
        var players = new List<Player>([
            new Player(0, PlayerRole.Sheriff, 5),
            new Player(1, PlayerRole.Outlaw, 4),
            new Player(2, PlayerRole.Outlaw, 4),
            new Player(3, PlayerRole.Outlaw, 4),
        ]);
        _cardRepoMock.Setup(repo => repo.GetAll()).Returns([
            CardFactory.CreateCard(CardName.Bang, CardSuit.Spades, CardRank.Ace),
            CardFactory.CreateCard(CardName.Bang, CardSuit.Spades, CardRank.Ace),
            CardFactory.CreateCard(CardName.Bang, CardSuit.Spades, CardRank.Ace),
            CardFactory.CreateCard(CardName.Bang, CardSuit.Spades, CardRank.Ace),
        ]);
        _getMock.Setup(get => get.GetCardIndex(null!, 0, null)).Returns(0);
        var deck = new Deck(_cardRepoMock.Object);
        var rc = generalStore.Play(new GameContext(players, deck, 0, _getMock.Object));
        Assert.Equal(CardRc.Ok, rc);
        foreach (var player in players)
            Assert.Single(player.CardsInHand);
    }

    [Fact]
    public void Play_IndiansDeathTest()
    {
        var indians = (Indians)CardFactory.CreateCard(CardName.Indians, CardSuit.Spades, CardRank.Ace);
        var players = new List<Player>([
            new Player(0, PlayerRole.Sheriff, 5),
            new Player(1, PlayerRole.Outlaw, 4),
            new Player(2, PlayerRole.Outlaw, 4),
            new Player(3, PlayerRole.Outlaw, 4),
        ]);
        players[1].ApplyDamage(3, null!);
        var rc = indians.Play(new GameContext(players, null!, 0, _getMock.Object));
        Assert.Equal(CardRc.Ok, rc);
        Assert.Equal(players[0].MaxHealth, players[0].Health);
        Assert.Equal(0, players[1].Health);
        Assert.Equal(players[2].MaxHealth - 1, players[2].Health);
        Assert.Equal(players[3].MaxHealth - 1, players[3].Health);
    }
    
    [Fact]
    public void Play_IndiansOkTest()
    {
        var indians = (Indians)CardFactory.CreateCard(CardName.Indians, CardSuit.Spades, CardRank.Ace);
        var players = new List<Player>([
            new Player(0, PlayerRole.Sheriff, 5),
            new Player(1, PlayerRole.Outlaw, 4),
            new Player(2, PlayerRole.Outlaw, 4),
            new Player(3, PlayerRole.Outlaw, 4),
        ]);
        _cardRepoMock.Setup(repo => repo.GetAll()).Returns([]);
        var deck = new Deck(_cardRepoMock.Object);
        players[1].AddCardInHand(CardFactory.CreateCard(CardName.Bang, CardSuit.Clubs, CardRank.Ace));
        var rc = indians.Play(new GameContext(players, deck, 0, _getMock.Object));
        Assert.Equal(CardRc.Ok, rc);
        Assert.Equal(players[0].MaxHealth, players[0].Health);
        Assert.Equal(players[1].MaxHealth, players[1].Health);
        Assert.Empty(players[1].CardsInHand);
        Assert.Equal(players[2].MaxHealth - 1, players[2].Health);
        Assert.Equal(players[3].MaxHealth - 1, players[3].Health);
    }

    [Fact]
    public void Play_DuelDeathTest()
    {
        var duel = (Duel)CardFactory.CreateCard(CardName.Duel, CardSuit.Clubs, CardRank.Ace);
        var players = new List<Player>([
            new Player(0, PlayerRole.Sheriff, 5),
            new Player(1, PlayerRole.Outlaw, 4),
            new Player(2, PlayerRole.Outlaw, 4),
            new Player(3, PlayerRole.Outlaw, 4),
        ]);
        _cardRepoMock.Setup(repo => repo.GetAll()).Returns([]);
        _getMock.Setup(get => get.GetPlayerIndex(players, 0)).Returns(1);
        var deck = new Deck(_cardRepoMock.Object);
        for (var i = 0; i < 2; ++i)
        {
            players[i].AddCardInHand(CardFactory.CreateCard(CardName.Bang, CardSuit.Clubs, CardRank.Ace));
            players[i].AddCardInHand(CardFactory.CreateCard(CardName.Bang, CardSuit.Clubs, CardRank.Ace));
        }

        players[1].ApplyDamage(3, null!);
        var rc = duel.Play(new GameContext(players, deck, 0, _getMock.Object));
        Assert.Equal(CardRc.Ok, rc);
        Assert.Empty(players[0].CardsInHand);
        Assert.Empty(players[1].CardsInHand);
    }
    
    [Fact]
    public void Play_DuelOkTest()
    {
        var duel = (Duel)CardFactory.CreateCard(CardName.Duel, CardSuit.Clubs, CardRank.Ace);
        var players = new List<Player>([
            new Player(0, PlayerRole.Sheriff, 5),
            new Player(1, PlayerRole.Outlaw, 4),
            new Player(2, PlayerRole.Outlaw, 4),
            new Player(3, PlayerRole.Outlaw, 4),
        ]);
        _cardRepoMock.Setup(repo => repo.GetAll()).Returns([]);
        _getMock.Setup(get => get.GetPlayerIndex(players, 0)).Returns(1);
        var deck = new Deck(_cardRepoMock.Object);
        for (var i = 0; i < 2; ++i)
        {
            players[i].AddCardInHand(CardFactory.CreateCard(CardName.Bang, CardSuit.Clubs, CardRank.Ace));
            players[i].AddCardInHand(CardFactory.CreateCard(CardName.Bang, CardSuit.Clubs, CardRank.Ace));
        }

        var rc = duel.Play(new GameContext(players, deck, 0, _getMock.Object));
        Assert.Equal(CardRc.Ok, rc);
        Assert.Equal(players[0].MaxHealth, players[0].Health);
        Assert.Empty(players[0].CardsInHand);
        Assert.Equal(players[1].MaxHealth - 1, players[1].Health);
        Assert.Empty(players[1].CardsInHand);
    }

    [Fact]
    public void Play_GatlingManyDeathsTest()
    {
        var gatling = (Gatling)CardFactory.CreateCard(CardName.Gatling, CardSuit.Clubs, CardRank.Ace);
        var players = new List<Player>([
            new Player(0, PlayerRole.Sheriff, 5),
            new Player(1, PlayerRole.Outlaw, 4),
            new Player(2, PlayerRole.Outlaw, 4),
            new Player(3, PlayerRole.Outlaw, 4),
        ]);
        players[1].ApplyDamage(3, null!);
        players[3].ApplyDamage(3, null!);
        var rc = gatling.Play(new GameContext(players, null!, 0, _getMock.Object));
        Assert.Equal(CardRc.Ok, rc);
        Assert.Equal(players[0].MaxHealth, players[0].Health);
        Assert.Equal(0, players[1].Health);
        Assert.Equal(players[2].MaxHealth - 1, players[2].Health);
        Assert.Equal(0, players[3].Health);
    }
    
    [Fact]
    public void Play_GatlingOkTest()
    {
        var gatling = (Gatling)CardFactory.CreateCard(CardName.Gatling, CardSuit.Clubs, CardRank.Ace);
        var players = new List<Player>([
            new Player(0, PlayerRole.Sheriff, 5),
            new Player(1, PlayerRole.Outlaw, 4),
            new Player(2, PlayerRole.Outlaw, 4),
            new Player(3, PlayerRole.Outlaw, 4),
        ]);
        var rc = gatling.Play(new GameContext(players, null!, 0, _getMock.Object));
        Assert.Equal(CardRc.Ok, rc);
        Assert.Equal(players[0].MaxHealth, players[0].Health);
        Assert.Equal(players[1].MaxHealth - 1, players[1].Health);
        Assert.Equal(players[2].MaxHealth - 1, players[2].Health);
        Assert.Equal(players[3].MaxHealth - 1, players[3].Health);
    }
    
    [Fact]
    public void Play_CatBalouCantPlayTest()
    {
        var catBalou = (CatBalou)CardFactory.CreateCard(CardName.CatBalou, CardSuit.Spades, CardRank.Ace);
        var players = new List<Player>([
            new Player(0, PlayerRole.Sheriff, 5),
            new Player(1, PlayerRole.Outlaw, 4),
            new Player(2, PlayerRole.Outlaw, 4),
            new Player(3, PlayerRole.Outlaw, 4),
        ]);
        _getMock.Setup(get => get.GetPlayerIndex(players, 0)).Returns(1);
        var rc = catBalou.Play(new GameContext(players, null!, 0, _getMock.Object));
        Assert.Equal(CardRc.CantPlay, rc);
    }
    
    [Fact]
    public void Play_CatBalouOkTest()
    {
        var catBalou = (CatBalou)CardFactory.CreateCard(CardName.CatBalou, CardSuit.Spades, CardRank.Ace);
        var players = new List<Player>([
            new Player(0, PlayerRole.Sheriff, 5),
            new Player(1, PlayerRole.Outlaw, 4),
            new Player(2, PlayerRole.Outlaw, 4),
            new Player(3, PlayerRole.Outlaw, 4),
        ]);
        _cardRepoMock.Setup(repo => repo.GetAll()).Returns([]);
        var deck = new Deck(_cardRepoMock.Object);
        _getMock.Setup(get => get.GetPlayerIndex(players, 0)).Returns(1);
        players[1].AddCardInHand(CardFactory.CreateCard(CardName.Bang, CardSuit.Clubs, CardRank.Ace));
        _getMock.Setup(get => get.GetCardIndex(players[1].CardsInHand, 0, null)).Returns(0);
        var rc = catBalou.Play(new GameContext(players, deck, 0, _getMock.Object));
        Assert.Equal(CardRc.Ok, rc);
        Assert.Empty(players[1].CardsInHand);
        Assert.Empty(players[0].CardsInHand);
    }

    [Fact]
    public void Play_SaloonOkTest()
    {
        var saloon = (Saloon)CardFactory.CreateCard(CardName.Saloon, CardSuit.Clubs, CardRank.Ace);
        var players = new List<Player>([
            new Player(0, PlayerRole.Sheriff, 5),
            new Player(1, PlayerRole.Outlaw, 4),
            new Player(2, PlayerRole.Outlaw, 4),
            new Player(3, PlayerRole.Outlaw, 4),
        ]);
        players[0].ApplyDamage(3, null!);
        players[1].ApplyDamage(2, null!);
        players[2].ApplyDamage(1, null!);
        players[3].ApplyDamage(3, null!);
        var rc = saloon.Play(new GameContext(players, null!, 0, _getMock.Object));
        Assert.Equal(CardRc.Ok, rc);
        Assert.Equal(4, players[0].Health);
        Assert.Equal(3, players[1].Health);
        Assert.Equal(4, players[2].Health);
        Assert.Equal(2, players[3].Health);
    }

    [Fact]
    public void Play_StagecoachOkTest()
    {
        var stagecoach = (Stagecoach)CardFactory.CreateCard(CardName.Stagecoach, CardSuit.Clubs, CardRank.Ace);
        var players = new List<Player>([
            new Player(0, PlayerRole.Sheriff, 5),
            new Player(1, PlayerRole.Outlaw, 4),
            new Player(2, PlayerRole.Outlaw, 4),
            new Player(3, PlayerRole.Outlaw, 4),
        ]);
        _cardRepoMock.Setup(repo => repo.GetAll()).Returns([
            CardFactory.CreateCard(CardName.Bang, CardSuit.Clubs, CardRank.Ace),
            CardFactory.CreateCard(CardName.Bang, CardSuit.Clubs, CardRank.Ace),
        ]);
        var deck = new Deck(_cardRepoMock.Object);
        var rc = stagecoach.Play(new GameContext(players, deck, 0, _getMock.Object));
        Assert.Equal(CardRc.Ok, rc);
        Assert.Equal(2, players[0].CardsInHand.Count);
    }
    
    [Fact]
    public void Play_WellsFargoOkTest()
    {
        var wellsFargo = (WellsFargo)CardFactory.CreateCard(CardName.WellsFargo, CardSuit.Clubs, CardRank.Ace);
        var players = new List<Player>([
            new Player(0, PlayerRole.Sheriff, 5),
            new Player(1, PlayerRole.Outlaw, 4),
            new Player(2, PlayerRole.Outlaw, 4),
            new Player(3, PlayerRole.Outlaw, 4),
        ]);
        _cardRepoMock.Setup(repo => repo.GetAll()).Returns([
            CardFactory.CreateCard(CardName.Bang, CardSuit.Clubs, CardRank.Ace),
            CardFactory.CreateCard(CardName.Bang, CardSuit.Clubs, CardRank.Ace),
            CardFactory.CreateCard(CardName.Bang, CardSuit.Clubs, CardRank.Ace),
        ]);
        var deck = new Deck(_cardRepoMock.Object);
        var rc = wellsFargo.Play(new GameContext(players, deck, 0, _getMock.Object));
        Assert.Equal(CardRc.Ok, rc);
        Assert.Equal(3, players[0].CardsInHand.Count);
    }

    [Fact]
    public void Play_EquipmentCardsTest()
    {
        var scope = (Scope)CardFactory.CreateCard(CardName.Scope, CardSuit.Clubs, CardRank.Ace);
        var mustang = (Mustang)CardFactory.CreateCard(CardName.Mustang, CardSuit.Clubs, CardRank.Ace);
        var players = new List<Player>([
            new Player(0, PlayerRole.Sheriff, 5),
            new Player(1, PlayerRole.Outlaw, 4),
            new Player(2, PlayerRole.Outlaw, 4),
            new Player(3, PlayerRole.Outlaw, 4),
        ]);
        var rc = scope.Play(new GameContext(players, null!, 0, _getMock.Object));
        Assert.Equal(CardRc.Ok, rc);
        rc = scope.Play(new GameContext(players, null!, 0, _getMock.Object));
        Assert.Equal(CardRc.CantPlay, rc);
        rc = mustang.Play(new GameContext(players, null!, 0, _getMock.Object));
        Assert.Equal(CardRc.Ok, rc);
        rc = mustang.Play(new GameContext(players, null!, 0, _getMock.Object));
        Assert.Equal(CardRc.CantPlay, rc);
        var gameContext = new GameContext(players, null!, 0, _getMock.Object);
        var range = gameContext.GetRange(0, 1);
        Assert.Equal(0, range);
        range = gameContext.GetRange(1, 0);
        Assert.Equal(2, range);
    }

    [Fact]
    public void Play_DynamiteBlowTest()
    {
        var dynamite = (Dynamite)CardFactory.CreateCard(CardName.Dynamite, CardSuit.Clubs, CardRank.Ace);
        var players = new List<Player>([
            new Player(0, PlayerRole.Sheriff, 5),
            new Player(1, PlayerRole.Outlaw, 4),
            new Player(2, PlayerRole.Outlaw, 4),
            new Player(3, PlayerRole.Outlaw, 4),
        ]);
        _cardRepoMock.Setup(repo => repo.GetAll()).Returns([
            CardFactory.CreateCard(CardName.Bang, CardSuit.Spades, CardRank.Six),
        ]);
        var deck = new Deck(_cardRepoMock.Object);
        var gameContext = new GameContext(players, deck, 0, _getMock.Object);
        var rc = dynamite.Play(gameContext);
        Assert.Equal(CardRc.Ok, rc);
        dynamite.ApplyEffect(gameContext);
        Assert.Equal(2, players[0].Health);
        Assert.Equivalent(dynamite, deck.TopDiscardedCard);
    }
    
    [Fact]
    public void Play_DynamitePassTest()
    {
        var dynamite = (Dynamite)CardFactory.CreateCard(CardName.Dynamite, CardSuit.Clubs, CardRank.Ace);
        var players = new List<Player>([
            new Player(0, PlayerRole.Sheriff, 5),
            new Player(1, PlayerRole.Outlaw, 4),
            new Player(2, PlayerRole.Outlaw, 4),
            new Player(3, PlayerRole.Outlaw, 4),
        ]);
        _cardRepoMock.Setup(repo => repo.GetAll()).Returns([
            CardFactory.CreateCard(CardName.Bang, CardSuit.Clubs, CardRank.Six),
        ]);
        var deck = new Deck(_cardRepoMock.Object);
        var gameContext = new GameContext(players, deck, 0, _getMock.Object);
        var rc = dynamite.Play(gameContext);
        Assert.Equal(CardRc.Ok, rc);
        dynamite.ApplyEffect(gameContext);
        Assert.Equal(players[0].MaxHealth, players[0].Health);
        Assert.Equivalent(dynamite, players[1].CardsOnBoard[0]);
    }
    
    [Fact]
    public void Play_BeerBarrelHealTest()
    {
        var beerBarrel = (BeerBarrel)CardFactory.CreateCard(CardName.BeerBarrel, CardSuit.Clubs, CardRank.Ace);
        var players = new List<Player>([
            new Player(0, PlayerRole.Sheriff, 5),
            new Player(1, PlayerRole.Outlaw, 4),
            new Player(2, PlayerRole.Outlaw, 4),
            new Player(3, PlayerRole.Outlaw, 4),
        ]);
        _cardRepoMock.Setup(repo => repo.GetAll()).Returns([
            CardFactory.CreateCard(CardName.Bang, CardSuit.Clubs, CardRank.Six),
        ]);
        var deck = new Deck(_cardRepoMock.Object);
        players[0].ApplyDamage(3, null!);
        var gameContext = new GameContext(players, deck, 0, _getMock.Object);
        var rc = beerBarrel.Play(gameContext);
        Assert.Equal(CardRc.Ok, rc);
        beerBarrel.ApplyEffect(gameContext);
        Assert.Equal(4, players[0].Health);
        Assert.Equivalent(beerBarrel, deck.TopDiscardedCard);
    }
    
    [Fact]
    public void Play_BeerBarrelPassTest()
    {
        var beerBarrel = (BeerBarrel)CardFactory.CreateCard(CardName.BeerBarrel, CardSuit.Clubs, CardRank.Ace);
        var players = new List<Player>([
            new Player(0, PlayerRole.Sheriff, 5),
            new Player(1, PlayerRole.Outlaw, 4),
            new Player(2, PlayerRole.Outlaw, 4),
            new Player(3, PlayerRole.Outlaw, 4),
        ]);
        _cardRepoMock.Setup(repo => repo.GetAll()).Returns([
            CardFactory.CreateCard(CardName.Bang, CardSuit.Spades, CardRank.Six),
        ]);
        var deck = new Deck(_cardRepoMock.Object);
        players[0].ApplyDamage(3, null!);
        var gameContext = new GameContext(players, deck, 0, _getMock.Object);
        var rc = beerBarrel.Play(gameContext);
        Assert.Equal(CardRc.Ok, rc);
        beerBarrel.ApplyEffect(gameContext);
        Assert.Equal(2, players[0].Health);
        Assert.Equivalent(beerBarrel, players[1].CardsOnBoard[0]);
    }

    [Fact]
    public void Play_JailCantPlayOnSheriffTest()
    {
        var jail = (Jail)CardFactory.CreateCard(CardName.Jail, CardSuit.Clubs, CardRank.Ace);
        var players = new List<Player>([
            new Player(0, PlayerRole.Sheriff, 5),
            new Player(1, PlayerRole.Outlaw, 4),
            new Player(2, PlayerRole.Outlaw, 4),
            new Player(3, PlayerRole.Outlaw, 4),
        ]);
        _getMock.Setup(get => get.GetPlayerIndex(players, 0)).Returns(0);
        var rc = jail.Play(new GameContext(players, null!, 1, _getMock.Object));
        Assert.Equal(CardRc.CantPlay, rc);
    }
    
    [Fact]
    public void Play_JailCantPlayTwiceTest()
    {
        var jail = (Jail)CardFactory.CreateCard(CardName.Jail, CardSuit.Clubs, CardRank.Ace);
        var players = new List<Player>([
            new Player(0, PlayerRole.Sheriff, 5),
            new Player(1, PlayerRole.Outlaw, 4),
            new Player(2, PlayerRole.Outlaw, 4),
            new Player(3, PlayerRole.Outlaw, 4),
        ]);
        _getMock.Setup(get => get.GetPlayerIndex(players, 0)).Returns(1);
        var rc = jail.Play(new GameContext(players, null!, 0, _getMock.Object));
        Assert.Equal(CardRc.Ok, rc);
        rc = jail.Play(new GameContext(players, null!, 0, _getMock.Object));
        Assert.Equal(CardRc.CantPlay, rc);
    }

    [Fact]
    public void Play_JailSkipTurnTest()
    {
        var jail = (Jail)CardFactory.CreateCard(CardName.Jail, CardSuit.Clubs, CardRank.Ace);
        var players = new List<Player>([
            new Player(0, PlayerRole.Sheriff, 5),
            new Player(1, PlayerRole.Outlaw, 4),
            new Player(2, PlayerRole.Outlaw, 4),
            new Player(3, PlayerRole.Outlaw, 4),
        ]);
        _cardRepoMock.Setup(repo => repo.GetAll()).Returns([
            CardFactory.CreateCard(CardName.Bang, CardSuit.Spades, CardRank.Six),
        ]);
        _getMock.Setup(get => get.GetPlayerIndex(players, 0)).Returns(1);
        var deck = new Deck(_cardRepoMock.Object);
        var rc = jail.Play(new GameContext(players, null!, 0, _getMock.Object));
        Assert.Equal(CardRc.Ok, rc);
        var res = jail.ApplyEffect(new GameContext(players, deck, 1, _getMock.Object));
        Assert.True(res);
        Assert.Equivalent(jail, deck.TopDiscardedCard);
    }

    [Fact]
    public void Play_WeaponTests()
    {
        var schofield = (Schofield)CardFactory.CreateCard(CardName.Schofield, CardSuit.Clubs, CardRank.Ace);
        var remington = (Remington)CardFactory.CreateCard(CardName.Remington, CardSuit.Clubs, CardRank.Ace);
        var carabine = (Carabine)CardFactory.CreateCard(CardName.Carabine, CardSuit.Clubs, CardRank.Ace);
        var winchester = (Winchester)CardFactory.CreateCard(CardName.Winchester, CardSuit.Clubs, CardRank.Ace);
        var players = new List<Player>([
            new Player(0, PlayerRole.Sheriff, 5),
            new Player(1, PlayerRole.Outlaw, 4),
            new Player(2, PlayerRole.Outlaw, 4),
            new Player(3, PlayerRole.Outlaw, 4),
        ]);
        var rc = schofield.Play(new GameContext(players, null!, 0, _getMock.Object));
        Assert.Equal(CardRc.Ok, rc);
        rc = remington.Play(new GameContext(players, null!, 1, _getMock.Object));
        Assert.Equal(CardRc.Ok, rc);
        rc = carabine.Play(new GameContext(players, null!, 2, _getMock.Object));
        Assert.Equal(CardRc.Ok, rc);
        rc = winchester.Play(new GameContext(players, null!, 3, _getMock.Object));
        Assert.Equal(CardRc.Ok, rc);
        Assert.Equal(2, players[0].Range);
        Assert.Equal(3, players[1].Range);
        Assert.Equal(4, players[2].Range);
        Assert.Equal(5, players[3].Range);
        _cardRepoMock.Setup(repo => repo.GetAll()).Returns([]);
        var deck = new Deck(_cardRepoMock.Object);
        rc = schofield.Play(new GameContext(players, deck, 3, _getMock.Object));
        Assert.Equal(CardRc.Ok, rc);
        Assert.Equal(2, players[3].Range);
        Assert.Equivalent(winchester, deck.TopDiscardedCard);
    }

    [Fact]
    public void CardFactory_CreateNotExistingCardTest()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => 
            CardFactory.CreateCard((CardName)50, CardSuit.Clubs, CardRank.Ace));
    }
}