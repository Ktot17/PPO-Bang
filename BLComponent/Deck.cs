using BLComponent.InputPorts;
using Newtonsoft.Json;

namespace BLComponent;

public record DeckDto
{
    public DeckDto() {}

    internal DeckDto(Deck deck)
    {
        DrawPile = deck.DrawPile.Select(c => new CardDto(c)).ToList();
        DiscardPile = deck.DiscardPile.Select(c => new CardDto(c)).ToList();
    }
    
    [JsonProperty]
    public IReadOnlyList<CardDto> DrawPile { get; private set; } = [];
    [JsonProperty]
    public IReadOnlyList<CardDto> DiscardPile { get; private set; } = [];
}

public class Deck
{
    protected readonly Stack<Card> DrawPileP = new();
    private readonly List<Card> _discardPile = [];
    private readonly Random _random = new();
    
    internal Card? TopDiscardedCard => _discardPile.Count == 0 ? null : _discardPile[_discardPile.Count - 1];
    
    internal Deck() {}

    internal Deck(ICardRepository cardRepository)
    {
        var cards = cardRepository.GetAll;
        Shuffle(cards);
        foreach (var card in cards)
            DrawPileP.Push(card);
    }

    internal Deck(DeckDto dto)
    {
        foreach (var cardDto in dto.DrawPile.Reverse())
            DrawPileP.Push(CardFactory.CreateCard(cardDto.Name, cardDto.Suit, cardDto.Rank));
        foreach (var cardDto in dto.DiscardPile)
            _discardPile.Add(CardFactory.CreateCard(cardDto.Name, cardDto.Suit, cardDto.Rank));
    }

    internal Card Draw(IGameView gameView)
    {
        if (DrawPileP.Count != 0)
            return DrawPileP.Pop();
        Shuffle(_discardPile);
        foreach (var card in _discardPile)
        {
            gameView.CardReturnedToDeck(card.Id);
            DrawPileP.Push(card);
        }
        _discardPile.Clear();
        return DrawPileP.Pop();
    }

    internal void Discard(Card card, IGameView gameView)
    {
        gameView.CardDiscarded(card.Id);
        _discardPile.Add(card);
    }

    private void Shuffle(IList<Card> cards)
    {
        var n = cards.Count;
        while (n > 1)
        {
            --n;
            var k = _random.Next(n + 1);
            (cards[k], cards[n]) = (cards[n], cards[k]);
        }
    }

    internal IReadOnlyList<Card> DrawPile => [..DrawPileP];
    internal IReadOnlyList<Card> DiscardPile => _discardPile;
}
