namespace BLComponent;

public abstract class WeaponCard : Card
{
    public int Range { get; protected init; }
    
    protected WeaponCard(CardSuit suit, CardRank rank) : base(suit, rank)
    {
        Type = CardType.Weapon;
    }

    internal override CardRc Play(GameState state)
    {
        var weapon = state.CurrentPlayer.ChangeWeapon(this);
        if (weapon != null)
            state.CardDeck.Discard(weapon);
        return CardRc.Ok;
    }
}

public sealed class Volcanic : WeaponCard
{
    public Volcanic(CardSuit suit, CardRank rank) : base(suit, rank)
    {
        Name = CardName.Volcanic;
        Range = 1;
    }
}

public sealed class Schofield : WeaponCard
{
    public Schofield(CardSuit suit, CardRank rank) : base(suit, rank)
    {
        Name = CardName.Schofield;
        Range = 2;
    }
}

public sealed class Remington : WeaponCard
{
    public Remington(CardSuit suit, CardRank rank) : base(suit, rank)
    {
        Name = CardName.Remington;
        Range = 3;
    }
}

public sealed class Carabine : WeaponCard
{
    public Carabine(CardSuit suit, CardRank rank) : base(suit, rank)
    {
        Name = CardName.Carabine;
        Range = 4;
    }
}

public sealed class Winchester : WeaponCard
{
    public Winchester(CardSuit suit, CardRank rank) : base(suit, rank)
    {
        Name = CardName.Winchester;
        Range = 5;
    }
}