namespace BLComponent.Cards;

public abstract class EquipmentCard : Card
{
    protected EquipmentCard(CardSuit suit, CardRank rank) : base(suit, rank)
    {
        Type = CardType.Equipment;
    }
}

public abstract class NotJail(CardSuit suit, CardRank rank) : EquipmentCard(suit, rank)
{
    public override CardRc Play(GameContext context)
    {
        if (context.Players[context.CurrentPlayer].CardsOnBoard.Any(c => c.Name == Name))
            return CardRc.CantPlay;
        context.Players[context.CurrentPlayer].AddCardOnBoard(this);
        return CardRc.Ok;
    }
}

public sealed class Barrel : NotJail
{
    public Barrel(CardSuit suit, CardRank rank) : base(suit, rank)
    {
        Name = CardName.Barrel;
    }
    
    public bool ApplyEffect(GameContext context, int playerIndex)
    {
        var card = context.CardDeck.Draw();
        context.CardDeck.Discard(card);
        var player = context.Players[playerIndex];
        var cardIndex = player.CardsOnBoard.ToList().FindIndex(c => c.Name == Name);
        if (card.Suit is not CardSuit.Hearts) return false;
        context.CardDeck.Discard(player.RemoveCard(cardIndex + player.CardsInHand.Count));
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
    public Dynamite(CardSuit suit, CardRank rank) : base(suit, rank)
    {
        Name = CardName.Dynamite;
    }

    public void ApplyEffect(GameContext context)
    {
        var card = context.CardDeck.Draw();
        context.CardDeck.Discard(card);
        var player = context.Players[context.CurrentPlayer];
        var cardIndex = player.CardsOnBoard.ToList().FindIndex(c => c.Name == Name);
        var dynamite = player.RemoveCard(cardIndex + player.CardsInHand.Count);
        if (card.Suit is not CardSuit.Spades || card.Rank is < CardRank.Two or > CardRank.Nine)
            context.Players[(context.CurrentPlayer + 1) % context.Players.Count].AddCardOnBoard(dynamite);
        else
        {
            player.ApplyDamage(3, context);
            context.CardDeck.Discard(dynamite);
        }
    }
}

public sealed class BeerBarrel : NotJail
{
    public BeerBarrel(CardSuit suit, CardRank rank) : base(suit, rank)
    {
        Name = CardName.BeerBarrel;
    }
    
    public void ApplyEffect(GameContext context)
    {
        var card = context.CardDeck.Draw();
        context.CardDeck.Discard(card);
        var player = context.Players[context.CurrentPlayer];
        var cardIndex = player.CardsOnBoard.ToList().FindIndex(c => c.Name == Name);
        var beerBarrel = player.RemoveCard(cardIndex + player.CardsInHand.Count);
        if (card.Suit is not CardSuit.Clubs || card.Rank is < CardRank.Two or > CardRank.Nine)
            context.Players[(context.CurrentPlayer + 1) % context.Players.Count].AddCardOnBoard(beerBarrel);
        else
        {
            player.Heal(2);
            context.CardDeck.Discard(beerBarrel);
        }
    }
}

public sealed class Jail : EquipmentCard
{
    public Jail(CardSuit suit, CardRank rank) : base(suit, rank)
    {
        Name = CardName.Jail;
    }

    public override CardRc Play(GameContext context)
    {
        var playerIndex = GetTarget(context);
        var target = context.Players[playerIndex];
        if (target.CardsOnBoard.Any(c => c.Name == Name) ||
            target.Role is PlayerRole.Sheriff)
            return CardRc.CantPlay;
        target.AddCardOnBoard(this);
        return CardRc.Ok;
    }
    
    public bool ApplyEffect(GameContext context)
    {
        var card = context.CardDeck.Draw();
        context.CardDeck.Discard(card);
        var player = context.Players[context.CurrentPlayer];
        var cardIndex = player.CardsOnBoard.ToList().FindIndex(c => c.Name == Name);
        var jail = player.RemoveCard(cardIndex + player.CardsInHand.Count);
        context.CardDeck.Discard(jail);
        return card.Suit is not CardSuit.Hearts;
    }
}