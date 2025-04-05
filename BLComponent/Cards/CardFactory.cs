namespace BLComponent.Cards;

public static class CardFactory
{
    public static Card CreateCard(CardName name, CardSuit suit, CardRank rank)
    {
        return name switch
        {
            CardName.Bang => new Bang(suit, rank),
            CardName.Beer => new Beer(suit, rank),
            CardName.Missed => new Missed(suit, rank),
            CardName.Panic => new Panic(suit, rank),
            CardName.GeneralStore => new GeneralStore(suit, rank),
            CardName.Indians => new Indians(suit, rank),
            CardName.Duel => new Duel(suit, rank),
            CardName.Gatling => new Gatling(suit, rank),
            CardName.CatBalou => new CatBalou(suit, rank),
            CardName.Saloon => new Saloon(suit, rank),
            CardName.Stagecoach => new Stagecoach(suit, rank),
            CardName.WellsFargo => new WellsFargo(suit, rank),
            CardName.Barrel => new Barrel(suit, rank),
            CardName.Scope => new Scope(suit, rank),
            CardName.Mustang => new Mustang(suit, rank),
            CardName.Dynamite => new Dynamite(suit, rank),
            CardName.BeerBarrel => new BeerBarrel(suit, rank),
            CardName.Jail => new Jail(suit, rank),
            CardName.Volcanic => new Volcanic(suit, rank),
            CardName.Schofield => new Schofield(suit, rank),
            CardName.Remington => new Remington(suit, rank),
            CardName.Carabine => new Carabine(suit, rank),
            CardName.Winchester => new Winchester(suit, rank),
            _ => throw new ArgumentOutOfRangeException(nameof(name), name, null)
        };
    }
}