namespace BLComponent.Cards;

public abstract class Card(CardSuit suit, CardRank rank)
{
    public CardSuit Suit { get; } = suit;
    public CardRank Rank { get; } = rank;
    public CardName Name { get; protected init; }
    public CardType Type { get; protected init; }

    public abstract CardRc Play(GameContext context);

    protected static int GetTarget(GameContext context)
    {
        var targets = context.Players;
        var playerIndex = context.Get.GetPlayerIndex(targets, context.CurrentPlayer);
        var playerId = targets[playerIndex].Id;
        playerIndex = context.Players.FindIndex(p => p.Id == playerId);
        return playerIndex;
    }

    protected static void Shoot(GameContext context, int playerIndex)
    {
        var target = context.Players[playerIndex];
        var cardIndex = target.CardsOnBoard.ToList().FindIndex(c => c.Name == CardName.Barrel);
        if (cardIndex != -1 && ((Barrel)target.CardsOnBoard[cardIndex]).ApplyEffect(context, playerIndex))
            return;

        var index = target.CardsInHand.ToList().FindIndex(card => card.Name == CardName.Missed);
        if (index == -1)
            target.ApplyDamage(1, new GameContext(context.Players, context.CardDeck, playerIndex, context.Get));
        else
            context.CardDeck.Discard(target.RemoveCard(index));
    }
}