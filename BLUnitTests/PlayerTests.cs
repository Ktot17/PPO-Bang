using BLComponent;
using BLComponent.InputPorts;
using Moq;

namespace BLUnitTests;

public class PlayerTests
{
    [Fact]
    public void AddCard_Test()
    {
        var player = new Player(Guid.NewGuid(), PlayerRole.Sheriff, 5);
        var card = CardFactory.CreateCard(CardName.Bang, CardSuit.Clubs, CardRank.Ace);
        player.AddCardInHand(card);
        Assert.Single(player.CardsInHand);
        card = CardFactory.CreateCard(CardName.Barrel, CardSuit.Clubs, CardRank.Ace);
        player.AddCardOnBoard(card);
        Assert.Single(player.CardsOnBoard);
        card = CardFactory.CreateCard(CardName.Volcanic, CardSuit.Clubs, CardRank.Ace);
        var weapon = player.ChangeWeapon((WeaponCard)card);
        Assert.Null(weapon);
        Assert.NotNull(player.Weapon);
        weapon = player.ChangeWeapon((WeaponCard)CardFactory.CreateCard(CardName.Winchester, CardSuit.Clubs, CardRank.Ace));
        Assert.Equivalent(card, weapon);
        Assert.NotNull(player.Weapon);
    }

    [Fact]
    public void RemoveCard_BadIdTest()
    {
        var player = new Player(Guid.NewGuid(), PlayerRole.Sheriff, 5);
        Assert.Throws<NotExistingGuidException>(() => player.RemoveCard(Guid.NewGuid()));
        Assert.Throws<NotExistingGuidException>(() => player.RemoveCard(Guid.NewGuid()));
    }

    [Fact]
    public void RemoveCard_FromHandTest()
    {
        var player = new Player(Guid.NewGuid(), PlayerRole.Sheriff, 5);
        for (var i = 0; i < 5; i++)
            player.AddCardInHand(CardFactory.CreateCard(CardName.Bang, CardSuit.Clubs, CardRank.Ace));
        for (var i = 0; i < 5; i++)
            player.AddCardOnBoard(CardFactory.CreateCard(CardName.Barrel, CardSuit.Clubs, CardRank.Ace));
        player.ChangeWeapon((WeaponCard)CardFactory.CreateCard(CardName.Volcanic, CardSuit.Clubs, CardRank.Ace));
        var id = player.CardsInHand[1].Id;
        var card = player.RemoveCard(id);
        Assert.Equivalent(card, CardFactory.CreateCard(CardName.Bang, CardSuit.Clubs, CardRank.Ace));
        Assert.Equal(4, player.CardsInHand.Count);
        Assert.Equal(5, player.CardsOnBoard.Count);
        Assert.NotNull(player.Weapon);
    }
    
    [Fact]
    public void RemoveCard_FromBoardTest()
    {
        var player = new Player(Guid.NewGuid(), PlayerRole.Sheriff, 5);
        for (var i = 0; i < 5; i++)
            player.AddCardInHand(CardFactory.CreateCard(CardName.Bang, CardSuit.Clubs, CardRank.Ace));
        for (var i = 0; i < 5; i++)
            player.AddCardOnBoard(CardFactory.CreateCard(CardName.Barrel, CardSuit.Clubs, CardRank.Ace));
        player.ChangeWeapon((WeaponCard)CardFactory.CreateCard(CardName.Volcanic, CardSuit.Clubs, CardRank.Ace));
        var id = player.CardsOnBoard[1].Id;
        var card = player.RemoveCard(id);
        Assert.Equivalent(card, CardFactory.CreateCard(CardName.Barrel, CardSuit.Clubs, CardRank.Ace));
        Assert.Equal(5, player.CardsInHand.Count);
        Assert.Equal(4, player.CardsOnBoard.Count);
        Assert.NotNull(player.Weapon);
    }
    
    [Fact]
    public void RemoveCard_WeaponTest()
    {
        var player = new Player(Guid.NewGuid(), PlayerRole.Sheriff, 5);
        for (var i = 0; i < 5; i++)
            player.AddCardInHand(CardFactory.CreateCard(CardName.Bang, CardSuit.Clubs, CardRank.Ace));
        for (var i = 0; i < 5; i++)
            player.AddCardOnBoard(CardFactory.CreateCard(CardName.Barrel, CardSuit.Clubs, CardRank.Ace));
        player.ChangeWeapon((WeaponCard)CardFactory.CreateCard(CardName.Volcanic, CardSuit.Clubs, CardRank.Ace));
        var id = player.Weapon!.Id;
        var card = player.RemoveCard(id);
        Assert.Equivalent(card, CardFactory.CreateCard(CardName.Volcanic, CardSuit.Clubs, CardRank.Ace));
        Assert.Equal(5, player.CardsInHand.Count);
        Assert.Equal(5, player.CardsOnBoard.Count);
        Assert.Null(player.Weapon);
    }

    [Fact]
    public void ApplyDamage_UsualTest()
    {
        var player = new Player(Guid.NewGuid(), PlayerRole.Sheriff, 5);
        var rc = player.ApplyDamage(2, null!);
        Assert.True(rc);
        Assert.False(player.IsDead);
        Assert.False(player.IsDeadOnThisTurn);
        Assert.Equal(3, player.Health);
    }
    
    [Fact]
    public void ApplyDamage_DeathTest()
    {
        var player = new Player(Guid.NewGuid(), PlayerRole.Sheriff, 5);
        var rc = player.ApplyDamage(5, null!);
        Assert.False(rc);
        Assert.True(player.IsDead);
        Assert.True(player.IsDeadOnThisTurn);
        Assert.Equal(0, player.Health);
    }
    
    [Fact]
    public void ApplyDamage_OneBeerDeathTest()
    {
        var players = new List<Player>([
            new Player(Guid.NewGuid(), PlayerRole.Sheriff, 5),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
        ]);
        var cardRepoMock = new Mock<ICardRepository>();
        cardRepoMock.Setup(repo => repo.GetAll()).Returns([]);
        players[0].AddCardInHand(CardFactory.CreateCard(CardName.Beer, CardSuit.Clubs, CardRank.Ace));
        var rc = players[0].ApplyDamage(5, new GameState(players, new Deck(cardRepoMock.Object), players[0].Id, null!));
        Assert.True(rc);
        Assert.False(players[0].IsDead);
        Assert.False(players[0].IsDeadOnThisTurn);
        Assert.Equal(1, players[0].Health);
    }
    
    [Fact]
    public void ApplyDamage_ManyBeerDeathTest()
    {
        var players = new List<Player>([
            new Player(Guid.NewGuid(), PlayerRole.Sheriff, 5),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
        ]);
        var cardRepoMock = new Mock<ICardRepository>();
        cardRepoMock.Setup(repo => repo.GetAll()).Returns([]);
        players[0].AddCardInHand(CardFactory.CreateCard(CardName.Beer, CardSuit.Clubs, CardRank.Ace));
        players[0].AddCardInHand(CardFactory.CreateCard(CardName.Beer, CardSuit.Clubs, CardRank.Ace));
        players[0].AddCardInHand(CardFactory.CreateCard(CardName.Beer, CardSuit.Clubs, CardRank.Ace));
        var rc = players[0].ApplyDamage(7, new GameState(players, new Deck(cardRepoMock.Object), players[0].Id, null!));
        Assert.True(rc);
        Assert.False(players[0].IsDead);
        Assert.False(players[0].IsDeadOnThisTurn);
        Assert.Equal(1, players[0].Health);
    }
    
    [Fact]
    public void ApplyDamage_TwoPlayersBeerDeathTest()
    {
        var players = new List<Player>([
            new Player(Guid.NewGuid(), PlayerRole.Sheriff, 5),
            new Player(Guid.NewGuid(), PlayerRole.Outlaw, 4),
        ]);
        var cardRepoMock = new Mock<ICardRepository>();
        cardRepoMock.Setup(repo => repo.GetAll()).Returns([]);
        players[0].AddCardInHand(CardFactory.CreateCard(CardName.Beer, CardSuit.Clubs, CardRank.Ace));
        var rc = players[0].ApplyDamage(5, new GameState(players, new Deck(cardRepoMock.Object), players[0].Id, null!));
        Assert.False(rc);
        Assert.True(players[0].IsDead);
        Assert.True(players[0].IsDeadOnThisTurn);
        Assert.Equal(0, players[0].Health);
    }

    [Fact]
    public void Heal_MaxHealthTest()
    {
        var player = new Player(Guid.NewGuid(), PlayerRole.Sheriff, 5);
        var rc = player.Heal(100);
        Assert.True(rc);
        Assert.False(player.IsDead);
        Assert.Equal(5, player.Health);
    }
    
    [Fact]
    public void Heal_UsualTest()
    {
        var player = new Player(Guid.NewGuid(), PlayerRole.Sheriff, 5);
        player.ApplyDamage(2, null!);
        var rc = player.Heal(1);
        Assert.False(rc);
        Assert.False(player.IsDead);
        Assert.Equal(4, player.Health);
    }

    [Fact]
    public void Heal_ReviveTest()
    {
        var player = new Player(Guid.NewGuid(), PlayerRole.Sheriff, 5);
        player.ApplyDamage(6, null!);
        var rc = player.Heal(2);
        Assert.False(rc);
        Assert.False(player.IsDead);
        Assert.Equal(1, player.Health);
    }
}