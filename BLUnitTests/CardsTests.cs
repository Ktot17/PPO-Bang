using BLComponent;
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
            new Player(Guid.NewGuid(), PlayerRole.Sheriff, 5),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
        ]);
        _getMock.Setup(get => get.GetPlayerId(players.Skip(1).ToList(), players[0].Id)).Returns(players[1].Id);
        var rc = bang.Play(new GameState(players, null!, players[0].Id, _getMock.Object));
        Assert.Equal(CardRc.Ok, rc);
        rc = bang.Play(new GameState(players, null!, players[0].Id, _getMock.Object));
        Assert.Equal(CardRc.CantPlay, rc);
        rc = bang.Play(new GameState(players, null!, players[0].Id, _getMock.Object));
        Assert.Equal(CardRc.CantPlay, rc);
    }
    
    [Fact]
    public void Play_ManyBangsWithVolcanicTest()
    {
        var bang = (Bang)CardFactory.CreateCard(CardName.Bang, CardSuit.Diamonds, CardRank.Ace);
        var players = new List<Player>([
            new Player(Guid.NewGuid(), PlayerRole.Sheriff, 5),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
        ]);
        _getMock.Setup(get => get.GetPlayerId(players.Skip(1).ToList(), players[0].Id)).Returns(players[1].Id);
        players[0].ChangeWeapon((WeaponCard)CardFactory.CreateCard(CardName.Volcanic, CardSuit.Clubs, CardRank.Ace));
        var rc = bang.Play(new GameState(players, null!, players[0].Id, _getMock.Object));
        Assert.Equal(CardRc.Ok, rc);
        rc = bang.Play(new GameState(players, null!, players[0].Id, _getMock.Object));
        Assert.Equal(CardRc.Ok, rc);
        rc = bang.Play(new GameState(players, null!, players[0].Id, _getMock.Object));
        Assert.Equal(CardRc.Ok, rc);
        rc = bang.Play(new GameState(players, null!, players[0].Id, _getMock.Object));
        Assert.Equal(CardRc.Ok, rc);
    }
    
    [Fact]
    public void Play_BangTooFarTest()
    {
        var bang = (Bang)CardFactory.CreateCard(CardName.Bang, CardSuit.Diamonds, CardRank.Ace);
        var players = new List<Player>([
            new Player(Guid.NewGuid(), PlayerRole.Sheriff, 5),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
        ]);
        _getMock.Setup(get => get.GetPlayerId(players.Skip(1).ToList(), players[0].Id)).Returns(players[2].Id);
        var rc = bang.Play(new GameState(players, null!, players[0].Id, _getMock.Object));
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
            new Player(Guid.NewGuid(), PlayerRole.Sheriff, 5),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
        ]);
        _cardRepoMock.Setup(repo => repo.GetAll).Returns([
            CardFactory.CreateCard(CardName.Bang, CardSuit.Hearts, CardRank.Ace)
        ]);
        _getMock.Setup(get => get.GetPlayerId(players.Skip(1).ToList(), players[0].Id)).Returns(players[1].Id);
        var deck = new Deck(_cardRepoMock.Object);
        players[1].AddCardOnBoard(barrel);
        var rc = bang.Play(new GameState(players, deck, players[0].Id, _getMock.Object));
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
            new Player(Guid.NewGuid(), PlayerRole.Sheriff, 5),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
        ]);
        _cardRepoMock.Setup(repo => repo.GetAll).Returns([
            CardFactory.CreateCard(CardName.Bang, CardSuit.Spades, CardRank.Ace)
        ]);
        _getMock.Setup(get => get.GetPlayerId(players.Skip(1).ToList(), players[0].Id)).Returns(players[1].Id);
        var deck = new Deck(_cardRepoMock.Object);
        players[1].AddCardOnBoard(barrel);
        var rc = bang.Play(new GameState(players, deck, players[0].Id, _getMock.Object));
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
            new Player(Guid.NewGuid(), PlayerRole.Sheriff, 5),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
        ]);
        _cardRepoMock.Setup(repo => repo.GetAll).Returns([
            CardFactory.CreateCard(CardName.Bang, CardSuit.Spades, CardRank.Ace)
        ]);
        _getMock.Setup(get => get.GetPlayerId(players.Skip(1).ToList(), players[0].Id)).Returns(players[1].Id);
        var deck = new Deck(_cardRepoMock.Object);
        players[1].AddCardInHand(missed);
        var rc = bang.Play(new GameState(players, deck, players[0].Id, _getMock.Object));
        Assert.Equal(CardRc.Ok, rc);
        Assert.Equal(4, players[1].Health);
        Assert.Empty(players[1].CardsInHand);
    }
    
    [Fact]
    public void Play_BeerMaxHealthTest()
    {
        var beer = (Beer)CardFactory.CreateCard(CardName.Beer, CardSuit.Diamonds, CardRank.Ace);
        var players = new List<Player>([
            new Player(Guid.NewGuid(), PlayerRole.Sheriff, 5),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
        ]);
        var rc = beer.Play(new GameState(players, null!, players[0].Id, _getMock.Object));
        Assert.Equal(CardRc.CantPlay, rc);
        Assert.Equal(5, players[0].Health);
    }
    
    [Fact]
    public void Play_BeerTwoPlayersTest()
    {
        var beer = (Beer)CardFactory.CreateCard(CardName.Beer, CardSuit.Diamonds, CardRank.Ace);
        var players = new List<Player>([
            new Player(Guid.NewGuid(), PlayerRole.Sheriff, 5),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
        ]);
        players[0].ApplyDamage(1, new GameState(players, null!, players[0].Id, _getMock.Object));
        var rc = beer.Play(new GameState(players, null!, players[0].Id, _getMock.Object));
        Assert.Equal(CardRc.CantPlay, rc);
        Assert.Equal(4, players[0].Health);
    }
    
    [Fact]
    public void Play_BeerOkTest()
    {
        var beer = (Beer)CardFactory.CreateCard(CardName.Beer, CardSuit.Diamonds, CardRank.Ace);
        var players = new List<Player>([
            new Player(Guid.NewGuid(), PlayerRole.Sheriff, 5),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
        ]);
        players[0].ApplyDamage(1, new GameState(players, null!, players[0].Id, _getMock.Object));
        var rc = beer.Play(new GameState(players, null!, players[0].Id, _getMock.Object));
        Assert.Equal(CardRc.Ok, rc);
        Assert.Equal(5, players[0].Health);
    }

    [Fact]
    public void Play_PanicTooFarTest()
    {
        var panic = (Panic)CardFactory.CreateCard(CardName.Panic, CardSuit.Spades, CardRank.Ace);
        var players = new List<Player>([
            new Player(Guid.NewGuid(), PlayerRole.Sheriff, 5),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
        ]);
        _getMock.Setup(get => get.GetPlayerId(players.Skip(1).ToList(), players[0].Id)).Returns(players[2].Id);
        var rc = panic.Play(new GameState(players, null!, players[0].Id, _getMock.Object));
        Assert.Equal(CardRc.TooFar, rc);
    }
    
    [Fact]
    public void Play_PanicCantPlayTest()
    {
        var panic = (Panic)CardFactory.CreateCard(CardName.Panic, CardSuit.Spades, CardRank.Ace);
        var players = new List<Player>([
            new Player(Guid.NewGuid(), PlayerRole.Sheriff, 5),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
        ]);
        _getMock.Setup(get => get.GetPlayerId(players.Skip(1).ToList(), players[0].Id)).Returns(players[1].Id);
        var rc = panic.Play(new GameState(players, null!, players[0].Id, _getMock.Object));
        Assert.Equal(CardRc.CantPlay, rc);
    }
    
    [Fact]
    public void Play_PanicOkTest()
    {
        var panic = (Panic)CardFactory.CreateCard(CardName.Panic, CardSuit.Spades, CardRank.Ace);
        var players = new List<Player>([
            new Player(Guid.NewGuid(), PlayerRole.Sheriff, 5),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
        ]);
        _getMock.Setup(get => get.GetPlayerId(players.Skip(1).ToList(), players[0].Id)).Returns(players[1].Id);
        players[1].AddCardInHand(CardFactory.CreateCard(CardName.Bang, CardSuit.Clubs, CardRank.Ace));
        var cards = new List<Card?>();
        cards.AddRange(players[1].CardsInHand);
        cards.AddRange(players[1].CardsOnBoard);
        cards.Add(players[1].Weapon);
        _getMock.Setup(get => get.GetCardId(cards, players[1].CardsInHand.Count))
            .Returns(players[1].CardsInHand[0].Id);
        var rc = panic.Play(new GameState(players, null!, players[0].Id, _getMock.Object));
        Assert.Equal(CardRc.Ok, rc);
        Assert.Empty(players[1].CardsInHand);
        Assert.Single(players[0].CardsInHand);
    }

    [Fact]
    public void Play_GeneralStoreOkTest()
    {
        var generalStore = (GeneralStore)CardFactory.CreateCard(CardName.GeneralStore, CardSuit.Clubs, CardRank.Ace);
        var players = new List<Player>([
            new Player(Guid.NewGuid(), PlayerRole.Sheriff, 5),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
        ]);
        _cardRepoMock.Setup(repo => repo.GetAll).Returns([
            CardFactory.CreateCard(CardName.Bang, CardSuit.Spades, CardRank.Ace),
            CardFactory.CreateCard(CardName.Bang, CardSuit.Spades, CardRank.Ace),
            CardFactory.CreateCard(CardName.Bang, CardSuit.Spades, CardRank.Ace),
            CardFactory.CreateCard(CardName.Bang, CardSuit.Spades, CardRank.Ace),
        ]);
        var deck = new Deck(_cardRepoMock.Object);
        var cards = deck.DrawPile();
        _getMock.Setup(get => get.GetCardId(cards, 0, players[0].Id)).Returns(cards[0].Id);
        _getMock.Setup(get => get.GetCardId(cards, 0, players[1].Id)).Returns(cards[1].Id);
        _getMock.Setup(get => get.GetCardId(cards, 0, players[2].Id)).Returns(cards[2].Id);
        _getMock.Setup(get => get.GetCardId(cards, 0, players[3].Id)).Returns(cards[3].Id);
        var rc = generalStore.Play(new GameState(players, deck, players[0].Id, _getMock.Object));
        Assert.Equal(CardRc.Ok, rc);
        foreach (var player in players)
            Assert.Single(player.CardsInHand);
    }

    [Fact]
    public void Play_IndiansDeathTest()
    {
        var indians = (Indians)CardFactory.CreateCard(CardName.Indians, CardSuit.Spades, CardRank.Ace);
        var players = new List<Player>([
            new Player(Guid.NewGuid(), PlayerRole.Sheriff, 5),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
        ]);
        players[1].ApplyDamage(3, null!);
        var rc = indians.Play(new GameState(players, null!, players[0].Id, _getMock.Object));
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
            new Player(Guid.NewGuid(), PlayerRole.Sheriff, 5),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
        ]);
        _cardRepoMock.Setup(repo => repo.GetAll).Returns([]);
        var deck = new Deck(_cardRepoMock.Object);
        players[1].AddCardInHand(CardFactory.CreateCard(CardName.Bang, CardSuit.Clubs, CardRank.Ace));
        var rc = indians.Play(new GameState(players, deck, players[0].Id, _getMock.Object));
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
            new Player(Guid.NewGuid(), PlayerRole.Sheriff, 5),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
        ]);
        _cardRepoMock.Setup(repo => repo.GetAll).Returns([]);
        _getMock.Setup(get => get.GetPlayerId(players.Skip(1).ToList(), players[0].Id)).Returns(players[1].Id);
        var deck = new Deck(_cardRepoMock.Object);
        for (var i = 0; i < 2; ++i)
        {
            players[i].AddCardInHand(CardFactory.CreateCard(CardName.Bang, CardSuit.Clubs, CardRank.Ace));
            players[i].AddCardInHand(CardFactory.CreateCard(CardName.Bang, CardSuit.Clubs, CardRank.Ace));
        }

        players[1].ApplyDamage(3, null!);
        var rc = duel.Play(new GameState(players, deck, players[0].Id, _getMock.Object));
        Assert.Equal(CardRc.Ok, rc);
        Assert.Empty(players[0].CardsInHand);
        Assert.Empty(players[1].CardsInHand);
    }
    
    [Fact]
    public void Play_DuelOkTest()
    {
        var duel = (Duel)CardFactory.CreateCard(CardName.Duel, CardSuit.Clubs, CardRank.Ace);
        var players = new List<Player>([
            new Player(Guid.NewGuid(), PlayerRole.Sheriff, 5),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
        ]);
        _cardRepoMock.Setup(repo => repo.GetAll).Returns([]);
        _getMock.Setup(get => get.GetPlayerId(players.Skip(1).ToList(), players[0].Id)).Returns(players[1].Id);
        var deck = new Deck(_cardRepoMock.Object);
        for (var i = 0; i < 2; ++i)
        {
            players[i].AddCardInHand(CardFactory.CreateCard(CardName.Bang, CardSuit.Clubs, CardRank.Ace));
            players[i].AddCardInHand(CardFactory.CreateCard(CardName.Bang, CardSuit.Clubs, CardRank.Ace));
        }

        var rc = duel.Play(new GameState(players, deck, players[0].Id, _getMock.Object));
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
            new Player(Guid.NewGuid(), PlayerRole.Sheriff, 5),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
        ]);
        players[1].ApplyDamage(3, null!);
        players[3].ApplyDamage(3, null!);
        var rc = gatling.Play(new GameState(players, null!, players[0].Id, _getMock.Object));
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
            new Player(Guid.NewGuid(), PlayerRole.Sheriff, 5),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
        ]);
        var rc = gatling.Play(new GameState(players, null!, players[0].Id, _getMock.Object));
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
            new Player(Guid.NewGuid(), PlayerRole.Sheriff, 5),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
        ]);
        _getMock.Setup(get => get.GetPlayerId(players.Skip(1).ToList(), players[0].Id)).Returns(players[1].Id);
        var rc = catBalou.Play(new GameState(players, null!, players[0].Id, _getMock.Object));
        Assert.Equal(CardRc.CantPlay, rc);
    }
    
    [Fact]
    public void Play_CatBalouOkTest()
    {
        var catBalou = (CatBalou)CardFactory.CreateCard(CardName.CatBalou, CardSuit.Spades, CardRank.Ace);
        var players = new List<Player>([
            new Player(Guid.NewGuid(), PlayerRole.Sheriff, 5),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
        ]);
        _cardRepoMock.Setup(repo => repo.GetAll).Returns([]);
        var deck = new Deck(_cardRepoMock.Object);
        _getMock.Setup(get => get.GetPlayerId(players.Skip(1).ToList(), players[0].Id)).Returns(players[1].Id);
        players[1].AddCardInHand(CardFactory.CreateCard(CardName.Bang, CardSuit.Clubs, CardRank.Ace));
        var cards = new List<Card?>();
        cards.AddRange(players[1].CardsInHand);
        cards.AddRange(players[1].CardsOnBoard);
        cards.Add(players[1].Weapon);
        _getMock.Setup(get => get.GetCardId(cards, players[1].CardsInHand.Count))
            .Returns(players[1].CardsInHand[0].Id);
        var rc = catBalou.Play(new GameState(players, deck, players[0].Id, _getMock.Object));
        Assert.Equal(CardRc.Ok, rc);
        Assert.Empty(players[1].CardsInHand);
        Assert.Empty(players[0].CardsInHand);
    }

    [Fact]
    public void Play_SaloonOkTest()
    {
        var saloon = (Saloon)CardFactory.CreateCard(CardName.Saloon, CardSuit.Clubs, CardRank.Ace);
        var players = new List<Player>([
            new Player(Guid.NewGuid(), PlayerRole.Sheriff, 5),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
        ]);
        players[0].ApplyDamage(3, null!);
        players[1].ApplyDamage(2, null!);
        players[2].ApplyDamage(1, null!);
        players[3].ApplyDamage(3, null!);
        var rc = saloon.Play(new GameState(players, null!, players[0].Id, _getMock.Object));
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
            new Player(Guid.NewGuid(), PlayerRole.Sheriff, 5),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
        ]);
        _cardRepoMock.Setup(repo => repo.GetAll).Returns([
            CardFactory.CreateCard(CardName.Bang, CardSuit.Clubs, CardRank.Ace),
            CardFactory.CreateCard(CardName.Bang, CardSuit.Clubs, CardRank.Ace),
        ]);
        var deck = new Deck(_cardRepoMock.Object);
        var rc = stagecoach.Play(new GameState(players, deck, players[0].Id, _getMock.Object));
        Assert.Equal(CardRc.Ok, rc);
        Assert.Equal(2, players[0].CardsInHand.Count);
    }
    
    [Fact]
    public void Play_WellsFargoOkTest()
    {
        var wellsFargo = (WellsFargo)CardFactory.CreateCard(CardName.WellsFargo, CardSuit.Clubs, CardRank.Ace);
        var players = new List<Player>([
            new Player(Guid.NewGuid(), PlayerRole.Sheriff, 5),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
        ]);
        _cardRepoMock.Setup(repo => repo.GetAll).Returns([
            CardFactory.CreateCard(CardName.Bang, CardSuit.Clubs, CardRank.Ace),
            CardFactory.CreateCard(CardName.Bang, CardSuit.Clubs, CardRank.Ace),
            CardFactory.CreateCard(CardName.Bang, CardSuit.Clubs, CardRank.Ace),
        ]);
        var deck = new Deck(_cardRepoMock.Object);
        var rc = wellsFargo.Play(new GameState(players, deck, players[0].Id, _getMock.Object));
        Assert.Equal(CardRc.Ok, rc);
        Assert.Equal(3, players[0].CardsInHand.Count);
    }

    [Fact]
    public void Play_EquipmentCardsTest()
    {
        var scope = (Scope)CardFactory.CreateCard(CardName.Scope, CardSuit.Clubs, CardRank.Ace);
        var mustang = (Mustang)CardFactory.CreateCard(CardName.Mustang, CardSuit.Clubs, CardRank.Ace);
        var players = new List<Player>([
            new Player(Guid.NewGuid(), PlayerRole.Sheriff, 5),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
        ]);
        var rc = scope.Play(new GameState(players, null!, players[0].Id, _getMock.Object));
        Assert.Equal(CardRc.Ok, rc);
        rc = scope.Play(new GameState(players, null!, players[0].Id, _getMock.Object));
        Assert.Equal(CardRc.CantPlay, rc);
        rc = mustang.Play(new GameState(players, null!, players[0].Id, _getMock.Object));
        Assert.Equal(CardRc.Ok, rc);
        rc = mustang.Play(new GameState(players, null!, players[0].Id, _getMock.Object));
        Assert.Equal(CardRc.CantPlay, rc);
        var gameState = new GameState(players, null!, players[0].Id, _getMock.Object);
        var range = gameState.GetRange(players[0].Id, players[1].Id);
        Assert.Equal(0, range);
        range = gameState.GetRange(players[1].Id, players[0].Id);
        Assert.Equal(2, range);
    }

    [Fact]
    public void Play_DynamiteBlowTest()
    {
        var dynamite = (Dynamite)CardFactory.CreateCard(CardName.Dynamite, CardSuit.Clubs, CardRank.Ace);
        var players = new List<Player>([
            new Player(Guid.NewGuid(), PlayerRole.Sheriff, 5),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
        ]);
        _cardRepoMock.Setup(repo => repo.GetAll).Returns([
            CardFactory.CreateCard(CardName.Bang, CardSuit.Spades, CardRank.Six),
        ]);
        var deck = new Deck(_cardRepoMock.Object);
        var gameState = new GameState(players, deck, players[0].Id, _getMock.Object);
        var rc = dynamite.Play(gameState);
        Assert.Equal(CardRc.Ok, rc);
        dynamite.ApplyEffect(gameState);
        Assert.Equal(2, players[0].Health);
        Assert.Equal(dynamite.Id, deck.TopDiscardedCard!.Id);
    }
    
    [Fact]
    public void Play_DynamitePassTest()
    {
        var dynamite = (Dynamite)CardFactory.CreateCard(CardName.Dynamite, CardSuit.Clubs, CardRank.Ace);
        var players = new List<Player>([
            new Player(Guid.NewGuid(), PlayerRole.Sheriff, 5),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
        ]);
        _cardRepoMock.Setup(repo => repo.GetAll).Returns([
            CardFactory.CreateCard(CardName.Bang, CardSuit.Clubs, CardRank.Six),
        ]);
        var deck = new Deck(_cardRepoMock.Object);
        var gameState = new GameState(players, deck, players[0].Id, _getMock.Object);
        var rc = dynamite.Play(gameState);
        Assert.Equal(CardRc.Ok, rc);
        dynamite.ApplyEffect(gameState);
        Assert.Equal(players[0].MaxHealth, players[0].Health);
        Assert.Equal(dynamite.Id, players[1].CardsOnBoard[0].Id);
    }
    
    [Fact]
    public void Play_BeerBarrelHealTest()
    {
        var beerBarrel = (BeerBarrel)CardFactory.CreateCard(CardName.BeerBarrel, CardSuit.Clubs, CardRank.Ace);
        var players = new List<Player>([
            new Player(Guid.NewGuid(), PlayerRole.Sheriff, 5),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
        ]);
        _cardRepoMock.Setup(repo => repo.GetAll).Returns([
            CardFactory.CreateCard(CardName.Bang, CardSuit.Clubs, CardRank.Six),
        ]);
        var deck = new Deck(_cardRepoMock.Object);
        players[0].ApplyDamage(3, null!);
        var gameState = new GameState(players, deck, players[0].Id, _getMock.Object);
        var rc = beerBarrel.Play(gameState);
        Assert.Equal(CardRc.Ok, rc);
        beerBarrel.ApplyEffect(gameState);
        Assert.Equal(4, players[0].Health);
        Assert.Equal(beerBarrel.Id, deck.TopDiscardedCard!.Id);
    }
    
    [Fact]
    public void Play_BeerBarrelPassTest()
    {
        var beerBarrel = (BeerBarrel)CardFactory.CreateCard(CardName.BeerBarrel, CardSuit.Clubs, CardRank.Ace);
        var players = new List<Player>([
            new Player(Guid.NewGuid(), PlayerRole.Sheriff, 5),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
        ]);
        _cardRepoMock.Setup(repo => repo.GetAll).Returns([
            CardFactory.CreateCard(CardName.Bang, CardSuit.Spades, CardRank.Six),
        ]);
        var deck = new Deck(_cardRepoMock.Object);
        players[0].ApplyDamage(3, null!);
        var gameState = new GameState(players, deck, players[0].Id, _getMock.Object);
        var rc = beerBarrel.Play(gameState);
        Assert.Equal(CardRc.Ok, rc);
        beerBarrel.ApplyEffect(gameState);
        Assert.Equal(2, players[0].Health);
        Assert.Equal(beerBarrel.Id, players[1].CardsOnBoard[0].Id);
    }

    [Fact]
    public void Play_JailCantPlayOnSheriffTest()
    {
        var jail = (Jail)CardFactory.CreateCard(CardName.Jail, CardSuit.Clubs, CardRank.Ace);
        var players = new List<Player>([
            new Player(Guid.NewGuid(), PlayerRole.Sheriff, 5),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
        ]);
        _getMock.Setup(get => get.GetPlayerId(players.Take(1).Concat(players.Skip(2)).ToList(), players[1].Id)).Returns(players[0].Id);
        var rc = jail.Play(new GameState(players, null!, players[1].Id, _getMock.Object));
        Assert.Equal(CardRc.CantPlay, rc);
    }
    
    [Fact]
    public void Play_JailCantPlayTwiceTest()
    {
        var jail = (Jail)CardFactory.CreateCard(CardName.Jail, CardSuit.Clubs, CardRank.Ace);
        var players = new List<Player>([
            new Player(Guid.NewGuid(), PlayerRole.Sheriff, 5),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
        ]);
        _getMock.Setup(get => get.GetPlayerId(players.Skip(1).ToList(), players[0].Id)).Returns(players[1].Id);
        var rc = jail.Play(new GameState(players, null!, players[0].Id, _getMock.Object));
        Assert.Equal(CardRc.Ok, rc);
        rc = jail.Play(new GameState(players, null!, players[0].Id, _getMock.Object));
        Assert.Equal(CardRc.CantPlay, rc);
    }

    [Fact]
    public void Play_JailSkipTurnTest()
    {
        var jail = (Jail)CardFactory.CreateCard(CardName.Jail, CardSuit.Clubs, CardRank.Ace);
        var players = new List<Player>([
            new Player(Guid.NewGuid(), PlayerRole.Sheriff, 5),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
        ]);
        _cardRepoMock.Setup(repo => repo.GetAll).Returns([
            CardFactory.CreateCard(CardName.Bang, CardSuit.Spades, CardRank.Six),
        ]);
        _getMock.Setup(get => get.GetPlayerId(players.Skip(1).ToList(), players[0].Id)).Returns(players[1].Id);
        var deck = new Deck(_cardRepoMock.Object);
        var rc = jail.Play(new GameState(players, null!, players[0].Id, _getMock.Object));
        Assert.Equal(CardRc.Ok, rc);
        var res = jail.ApplyEffect(new GameState(players, deck, players[1].Id, _getMock.Object));
        Assert.True(res);
        Assert.Equal(jail.Id, deck.TopDiscardedCard!.Id);
    }

    [Fact]
    public void Play_WeaponTests()
    {
        var schofield = (Schofield)CardFactory.CreateCard(CardName.Schofield, CardSuit.Clubs, CardRank.Ace);
        var remington = (Remington)CardFactory.CreateCard(CardName.Remington, CardSuit.Clubs, CardRank.Ace);
        var carabine = (Carabine)CardFactory.CreateCard(CardName.Carabine, CardSuit.Clubs, CardRank.Ace);
        var winchester = (Winchester)CardFactory.CreateCard(CardName.Winchester, CardSuit.Clubs, CardRank.Ace);
        var players = new List<Player>([
            new Player(Guid.NewGuid(), PlayerRole.Sheriff, 5),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
        ]);
        var rc = schofield.Play(new GameState(players, null!, players[0].Id, _getMock.Object));
        Assert.Equal(CardRc.Ok, rc);
        rc = remington.Play(new GameState(players, null!, players[1].Id, _getMock.Object));
        Assert.Equal(CardRc.Ok, rc);
        rc = carabine.Play(new GameState(players, null!, players[2].Id, _getMock.Object));
        Assert.Equal(CardRc.Ok, rc);
        rc = winchester.Play(new GameState(players, null!, players[3].Id, _getMock.Object));
        Assert.Equal(CardRc.Ok, rc);
        Assert.Equal(2, players[0].Range);
        Assert.Equal(3, players[1].Range);
        Assert.Equal(4, players[2].Range);
        Assert.Equal(5, players[3].Range);
        _cardRepoMock.Setup(repo => repo.GetAll).Returns([]);
        var deck = new Deck(_cardRepoMock.Object);
        rc = schofield.Play(new GameState(players, deck, players[3].Id, _getMock.Object));
        Assert.Equal(CardRc.Ok, rc);
        Assert.Equal(2, players[3].Range);
        Assert.Equal(winchester.Id, deck.TopDiscardedCard!.Id);
    }

    [Fact]
    public void TargetCards_NotExistingGuidTest()
    {
        var players = new List<Player>([
            new Player(Guid.NewGuid(), PlayerRole.Sheriff, 5),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
        ]);
        var bang = (Bang)CardFactory.CreateCard(CardName.Bang, CardSuit.Clubs, CardRank.Ace);
        var jail = (Jail)CardFactory.CreateCard(CardName.Jail, CardSuit.Clubs, CardRank.Ace);
        var duel = (Duel)CardFactory.CreateCard(CardName.Duel, CardSuit.Clubs, CardRank.Ace);
        var generalStore = (GeneralStore)CardFactory.CreateCard(CardName.GeneralStore, CardSuit.Clubs, CardRank.Ace);
        var panic = (Panic)CardFactory.CreateCard(CardName.Panic, CardSuit.Clubs, CardRank.Ace);
        var catBalou = (CatBalou)CardFactory.CreateCard(CardName.CatBalou, CardSuit.Clubs, CardRank.Ace);
        
        _cardRepoMock.Setup(repo => repo.GetAll).Returns([
            CardFactory.CreateCard(CardName.Bang, CardSuit.Spades, CardRank.Ace),
            CardFactory.CreateCard(CardName.Bang, CardSuit.Spades, CardRank.Ace),
            CardFactory.CreateCard(CardName.Bang, CardSuit.Spades, CardRank.Ace),
            CardFactory.CreateCard(CardName.Bang, CardSuit.Spades, CardRank.Ace),
        ]);
        var deck = new Deck(_cardRepoMock.Object);
        _getMock.Setup(get => get.GetPlayerId(players.Skip(1).ToList(), players[0].Id)).Returns(Guid.Empty);
        _getMock.Setup(get => get.GetCardId(null!, players[1].CardsInHand.Count))
            .Returns(Guid.Empty);

        Assert.Throws<NotExistingGuidException>(() =>
            bang.Play(new GameState(players, null!, players[0].Id, _getMock.Object)));
        Assert.Throws<NotExistingGuidException>(() =>
            jail.Play(new GameState(players, null!, players[0].Id, _getMock.Object)));
        Assert.Throws<NotExistingGuidException>(() =>
            duel.Play(new GameState(players, null!, players[0].Id, _getMock.Object)));
        Assert.Throws<NotExistingGuidException>(() =>
            generalStore.Play(new GameState(players, deck, players[0].Id, _getMock.Object)));
        Assert.Throws<NotExistingGuidException>(() =>
            panic.Play(new GameState(players, null!, players[0].Id, _getMock.Object)));
        Assert.Throws<NotExistingGuidException>(() =>
            catBalou.Play(new GameState(players, null!, players[0].Id, _getMock.Object)));
    }
}
