﻿using Newtonsoft.Json;

namespace BLComponent;

public record WeaponCardDto : CardDto
{
    public WeaponCardDto() {}

    internal WeaponCardDto(WeaponCard card) : base(card)
    {
        Range = card.Range;
    }
    
    [JsonProperty]
    public int Range { get; private set; }
}

public abstract class WeaponCard : Card
{
    public int Range { get; protected set; }
    
    protected WeaponCard(CardSuit suit, CardRank rank) : base(suit, rank)
    {
        Type = CardType.Weapon;
    }

    internal override Task<CardRc> Play(GameState state)
    {
        var weapon = state.CurrentPlayer.ChangeWeapon(this, state.GameView);
        state.GameView.ShowCardResult(state.CurrentPlayerId, Name, weapon is null);
        if (weapon is not null)
            state.CardDeck.Discard(weapon, state.GameView);
        return Task.FromResult(CardRc.Ok);
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
