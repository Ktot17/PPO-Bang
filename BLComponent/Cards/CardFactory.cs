namespace BLComponent;

public static class CardFactory
{
    private static readonly Dictionary<CardName, Func<CardSuit, CardRank, Card>> FactoryDictionary = new()
    {
        [CardName.Bang] = (s, r) => new Bang(s, r),
        [CardName.Beer] = (s, r) => new Beer(s, r),
        [CardName.Missed] = (s, r) => new Missed(s, r),
        [CardName.Panic] = (s, r) => new Panic(s, r),
        [CardName.GeneralStore] = (s, r) => new GeneralStore(s, r),
        [CardName.Indians] = (s, r) => new Indians(s, r),
        [CardName.Duel] = (s, r) => new Duel(s, r),
        [CardName.Gatling] = (s, r) => new Gatling(s, r),
        [CardName.CatBalou] = (s, r) => new CatBalou(s, r),
        [CardName.Saloon] = (s, r) => new Saloon(s, r),
        [CardName.Stagecoach] = (s, r) => new Stagecoach(s, r),
        [CardName.WellsFargo] = (s, r) => new WellsFargo(s, r),
        [CardName.Barrel] = (s, r) => new Barrel(s, r),
        [CardName.Scope] = (s, r) => new Scope(s, r),
        [CardName.Mustang] = (s, r) => new Mustang(s, r),
        [CardName.Dynamite] = (s, r) => new Dynamite(s, r),
        [CardName.BeerBarrel] = (s, r) => new BeerBarrel(s, r),
        [CardName.Jail] = (s, r) => new Jail(s, r),
        [CardName.Volcanic] = (s, r) => new Volcanic(s, r),
        [CardName.Schofield] = (s, r) => new Schofield(s, r),
        [CardName.Remington] = (s, r) => new Remington(s, r),
        [CardName.Carabine] = (s, r) => new Carabine(s, r),
        [CardName.Winchester] = (s, r) => new Winchester(s, r)
    };
    
    public static Card CreateCard(CardName name, CardSuit suit, CardRank rank) => FactoryDictionary[name].Invoke(suit, rank);
}
