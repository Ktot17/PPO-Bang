namespace BLComponent;

public abstract class Card(CardSuit suit, CardRank rank)
{
    public Guid Id { get; } = Guid.NewGuid();
    public CardSuit Suit { get; } = suit;
    public CardRank Rank { get; } = rank;
    public CardName Name { get; protected init; }
    public CardType Type { get; protected init; }

    internal abstract CardRc Play(GameState state);

    internal static void Shoot(GameState state, Guid playerId)
    {
        var target = state.Players.First(p => p.Id == playerId);
        var barrel = target.CardsOnBoard.FirstOrDefault(c => c.Name == CardName.Barrel);
        if (barrel != null && ((Barrel)barrel).ApplyEffect(state, playerId))
            return;

        var missed = target.CardsInHand.FirstOrDefault(card => card.Name == CardName.Missed);
        if (missed == null)
            target.ApplyDamage(1, new GameState(state.Players, state.CardDeck, target.Id, state.Get));
        else
            state.CardDeck.Discard(target.RemoveCard(missed.Id));
    }
}