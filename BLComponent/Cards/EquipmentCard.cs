namespace BLComponent;

public abstract class EquipmentCard : Card
{
    protected EquipmentCard(CardSuit suit, CardRank rank) : base(suit, rank)
    {
        Type = CardType.Equipment;
    }
}

public abstract class NotJail(CardSuit suit, CardRank rank) : EquipmentCard(suit, rank)
{
    internal override CardRc Play(GameState state)
    {
        if (state.CurrentPlayer.CardsOnBoard.Any(c => c.Name == Name))
            return CardRc.CantPlay;
        state.CurrentPlayer.AddCardOnBoard(this);
        return CardRc.Ok;
    }
}

public sealed class Barrel : NotJail
{
    public Barrel(CardSuit suit, CardRank rank) : base(suit, rank)
    {
        Name = CardName.Barrel;
    }
    
    internal bool ApplyEffect(GameState state, Guid playerId)
    {
        var card = state.CardDeck.Draw();
        state.CardDeck.Discard(card);
        var player = state.Players.First(p => p.Id == playerId);
        var barrel = player.CardsOnBoard.First(c => c.Name == Name);
        if (card.Suit is not CardSuit.Hearts)
            return false;
        state.CardDeck.Discard(player.RemoveCard(barrel.Id));
        return true;
    }
}

public sealed class Scope : NotJail
{
    public Scope(CardSuit suit, CardRank rank) : base(suit, rank)
    {
        Name = CardName.Scope;
    }
}

public sealed class Mustang : NotJail
{
    public Mustang(CardSuit suit, CardRank rank) : base(suit, rank)
    {
        Name = CardName.Mustang;
    }
}

public sealed class Dynamite : NotJail
{
    private const int DynamiteDamage = 3;
    
    public Dynamite(CardSuit suit, CardRank rank) : base(suit, rank)
    {
        Name = CardName.Dynamite;
    }

    internal void ApplyEffect(GameState state)
    {
        var card = state.CardDeck.Draw();
        state.CardDeck.Discard(card);
        var player = state.CurrentPlayer;
        var dynamite = player.CardsOnBoard.First(c => c.Name == Name);
        player.RemoveCard(dynamite.Id);
        if (card.Suit is not CardSuit.Spades || card.Rank is < CardRank.Two or > CardRank.Nine)
            state.GetNextPlayer().AddCardOnBoard(dynamite);
        else
        {
            player.ApplyDamage(DynamiteDamage, state);
            state.CardDeck.Discard(dynamite);
        }
    }
}

public sealed class BeerBarrel : NotJail
{
    private const int BeerBarrelHeal = 2;
    
    public BeerBarrel(CardSuit suit, CardRank rank) : base(suit, rank)
    {
        Name = CardName.BeerBarrel;
    }
    
    internal void ApplyEffect(GameState state)
    {
        var card = state.CardDeck.Draw();
        state.CardDeck.Discard(card);
        var player = state.CurrentPlayer;
        var beerBarrel = player.CardsOnBoard.First(c => c.Name == Name);
        player.RemoveCard(beerBarrel.Id);
        if (card.Suit is not CardSuit.Clubs || card.Rank is < CardRank.Two or > CardRank.Nine)
            state.GetNextPlayer().AddCardOnBoard(beerBarrel);
        else
        {
            player.Heal(BeerBarrelHeal);
            state.CardDeck.Discard(beerBarrel);
        }
    }
}

public sealed class Jail : EquipmentCard
{
    public Jail(CardSuit suit, CardRank rank) : base(suit, rank)
    {
        Name = CardName.Jail;
    }

    internal override CardRc Play(GameState state)
    {
        var playerId = state.Get.GetPlayerId(state.LivePlayers.Where(p => p.Id != state.CurrentPlayerId).ToList(),
            state.CurrentPlayerId);
        Player? target;
        try
        {
            target = state.Players.First(p => p.Id == playerId);
        }
        catch (InvalidOperationException)
        {
            throw new NotExistingGuidException();
        }
        if (target.CardsOnBoard.Any(c => c.Name == Name) ||
            target.Role is PlayerRole.Sheriff)
            return CardRc.CantPlay;
        target.AddCardOnBoard(this);
        return CardRc.Ok;
    }
    
    internal bool ApplyEffect(GameState state)
    {
        var card = state.CardDeck.Draw();
        state.CardDeck.Discard(card);
        var player = state.CurrentPlayer;
        var jail = player.CardsOnBoard.First(c => c.Name == Name);
        player.RemoveCard(jail.Id);
        state.CardDeck.Discard(jail);
        return card.Suit is not CardSuit.Hearts;
    }
}
