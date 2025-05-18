using Newtonsoft.Json;

namespace BLComponent;

public record CardDto
{
    public CardDto() {}

    internal CardDto(Card card)
    {
        Id = card.Id;
        Suit = card.Suit;
        Rank = card.Rank;
        Name = card.Name;
        Type = card.Type;
    }
    
    [JsonProperty]
    public Guid Id { get; private set; }
    [JsonProperty]
    public CardSuit Suit { get; private set; }
    [JsonProperty]
    public CardRank Rank { get; private set; }
    [JsonProperty]
    public CardName Name { get; private set; }
    [JsonProperty]
    public CardType Type { get; private set; }
}

public abstract class Card(CardSuit suit, CardRank rank)
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public CardSuit Suit { get; private set; } = suit;
    public CardRank Rank { get; private set; } = rank;
    public CardName Name { get; protected set; }
    public CardType Type { get; protected set; }

    internal abstract Task<CardRc> Play(GameState state);

    internal static async Task Shoot(GameState state, Guid playerId)
    {
        var target = state.Players.First(p => p.Id == playerId);
        var barrel = target.CardsOnBoard.FirstOrDefault(c => c.Name == CardName.Barrel);
        if (barrel is not null && ((Barrel)barrel).ApplyEffect(state, playerId))
            return;

        var missed = target.CardsInHand.FirstOrDefault(card => card.Name == CardName.Missed);
        if (await state.GameView.YesOrNoAsync(target.Id, CardName.Missed) && missed is not null)
        {
            state.CardDeck.Discard(target.RemoveCard(missed.Id), state.GameView);
            state.GameView.ShowCardResult(playerId, missed.Name);
        }
        else
        {
            state.GameView.ShowCardResult(state.CurrentPlayerId, CardName.Bang, true, playerId);
            await target.ApplyDamage(1, new GameState(state.Players, state.CardDeck, target.Id, state.GameView));
        }
    }
}
