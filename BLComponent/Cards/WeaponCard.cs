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
    private const int VolcanicRange = 1;
    
    public Volcanic(CardSuit suit, CardRank rank) : base(suit, rank)
    {
        Name = CardName.Volcanic;
        Range = VolcanicRange;
    }
}

public sealed class Schofield : WeaponCard
{
    private const int SchofieldRange = 2;
    
    public Schofield(CardSuit suit, CardRank rank) : base(suit, rank)
    {
        Name = CardName.Schofield;
        Range = SchofieldRange;
    }
}

public sealed class Remington : WeaponCard
{
    private const int RemingtonRange = 3;
    
    public Remington(CardSuit suit, CardRank rank) : base(suit, rank)
    {
        Name = CardName.Remington;
        Range = RemingtonRange;
    }
}

public sealed class Carabine : WeaponCard
{
    private const int CarabineRange = 4;
    
    public Carabine(CardSuit suit, CardRank rank) : base(suit, rank)
    {
        Name = CardName.Carabine;
        Range = CarabineRange;
    }
}

public sealed class Winchester : WeaponCard
{
    private const int WinchesterRange = 5;
    
    public Winchester(CardSuit suit, CardRank rank) : base(suit, rank)
    {
        Name = CardName.Winchester;
        Range = WinchesterRange;
    }
}
