using BLComponent.InputPorts;

namespace BLComponent;

public sealed class Deck
{
    private readonly Stack<Card> _drawPile = new();
    private readonly List<Card> _discardPile = [];
    private readonly Random _random = new();
    
    internal Card? TopDiscardedCard => _discardPile.Count == 0 ? null : _discardPile[^1];

    internal Deck(ICardRepository cardRepository)
    {
        var cards = cardRepository.GetAll;
        Shuffle(cards);
        foreach (var card in cards)
            _drawPile.Push(card);
    }

    internal Card Draw()
    {
        if (_drawPile.Count != 0)
            return _drawPile.Pop();
        Shuffle(_discardPile);
        foreach (var card in _discardPile)
            _drawPile.Push(card);
        _discardPile.Clear();
        return _drawPile.Pop();
    }

    internal void Discard(Card card)
    {
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
    
    internal IReadOnlyList<Card> DrawPile()
    {
        return _drawPile.ToList();
    }

    internal void ForUnitTestWithDynamiteAndBeerBarrel()
    {
        _drawPile.Clear();
        _drawPile.Push(CardFactory.CreateCard(CardName.Bang, CardSuit.Clubs, CardRank.Seven));
        _drawPile.Push(CardFactory.CreateCard(CardName.Bang, CardSuit.Spades, CardRank.Seven));
    }

    internal void ReturnCardsToDeck(IEnumerable<Card> cards)
    {
        foreach (var card in cards.Reverse())
            _drawPile.Push(card);
    }
}
