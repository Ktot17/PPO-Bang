namespace BLComponent;

public abstract class InstantCard : Card
{
    protected InstantCard(CardSuit suit, CardRank rank) : base(suit, rank)
    {
        Type = CardType.Instant;
    }
}

public sealed class Bang : InstantCard
{
    public Bang(CardSuit suit, CardRank rank) : base(suit, rank)
    {
        Name = CardName.Bang;
    }

    internal override CardRc Play(GameState state)
    {
        var player = state.CurrentPlayer;
        if (player.IsBangPlayed && (player.Weapon is null || player.Weapon.Name != CardName.Volcanic))
            return CardRc.CantPlay;
        var playerId = state.Get.GetPlayerId(state.LivePlayers.Where(p => p.Id != state.CurrentPlayerId).ToList(),
            state.CurrentPlayerId);
        if (state.Players.Find(p => p.Id == playerId) == null)
            throw new NotExistingGuidException();
        if (player.Range < state.GetRange(state.CurrentPlayerId, playerId))
            return CardRc.TooFar;
        player.BangPlayed();
        Shoot(state, playerId);
        return CardRc.Ok;
    }
}

public sealed class Beer : InstantCard
{
    public Beer(CardSuit suit, CardRank rank) : base(suit, rank)
    {
        Name = CardName.Beer;
    }

    internal override CardRc Play(GameState state)
    {
        var player = state.CurrentPlayer;
        if (state.LivePlayers.Count <= 2 ||
            player.Heal(1))
            return CardRc.CantPlay;
        return CardRc.Ok;
    }
}

public sealed class Missed : InstantCard
{
    public Missed(CardSuit suit, CardRank rank) : base(suit, rank)
    {
        Name = CardName.Missed;
    }

    internal override CardRc Play(GameState state)
    {
        return CardRc.CantPlay;
    }
}

public sealed class Panic : InstantCard
{
    public Panic(CardSuit suit, CardRank rank) : base(suit, rank)
    {
        Name = CardName.Panic;
    }

    internal override CardRc Play(GameState state)
    {
        var playerId = state.Get.GetPlayerId(state.LivePlayers.Where(p => p.Id != state.CurrentPlayerId).ToList(),
            state.CurrentPlayerId);
        Player? target;
        try
        {
            target = state.Players.First(p => p.Id == playerId);
        }
        catch (InvalidOperationException)
        {
            throw new NotExistingGuidException();
        }
        if (1 < state.GetRange(state.CurrentPlayerId, playerId))
            return CardRc.TooFar;
        if (target.CardCount == 0)
            return CardRc.CantPlay;
        var cards = new List<Card?>();
        cards.AddRange(target.CardsInHand);
        cards.AddRange(target.CardsOnBoard);
        cards.Add(target.Weapon);
        var cardId = state.Get.GetCardId(cards, target.CardsInHand.Count);
        var card = target.RemoveCard(cardId);
        state.CurrentPlayer.AddCardInHand(card);
        return CardRc.Ok;
    }
}

public sealed class GeneralStore : InstantCard
{
    public GeneralStore(CardSuit suit, CardRank rank) : base(suit, rank)
    {
        Name = CardName.GeneralStore;
    }

    internal override CardRc Play(GameState state)
    {
        var cards = new List<Card>();
        for (var i = 0; i < state.LivePlayers.Count; i++)
            cards.Add(state.CardDeck.Draw());
        var playerId = state.CurrentPlayerId;
        var chosenCards = new List<Card>();
        do
        {
            var cardId = state.Get.GetCardId(cards, 0, state.CurrentPlayerId);
            Card? card;
            try
            {
                card = cards.First(c => c.Id == cardId);
            }
            catch (InvalidOperationException)
            {
                state.CardDeck.ReturnCardsToDeck(cards);
                throw new NotExistingGuidException();
            }
            chosenCards.Add(card);
            state.NextPlayer();
        } while (playerId != state.CurrentPlayerId);

        var j = 0;
        do
        {
            state.CurrentPlayer.AddCardInHand(chosenCards[j++]);
            state.NextPlayer();
        } while (playerId != state.CurrentPlayerId);
        return CardRc.Ok;
    }
}

public sealed class Indians : InstantCard
{
    public Indians(CardSuit suit, CardRank rank) : base(suit, rank)
    {
        Name = CardName.Indians;
    }

    internal override CardRc Play(GameState state)
    {
        foreach (var player in state.LivePlayers.Where(p => p.Id != state.CurrentPlayerId))
        {
            var card = player.CardsInHand.FirstOrDefault(c => c.Name is CardName.Bang);
            if (card == null)
            {
                player.ApplyDamage(1,
                    new GameState(state.Players, state.CardDeck, player.Id, state.Get));
            }
            else
            {
                player.RemoveCard(card.Id);
                state.CardDeck.Discard(card);
            }
        }
        return CardRc.Ok;
    }
}

public sealed class Duel : InstantCard
{
    public Duel(CardSuit suit, CardRank rank) : base(suit, rank)
    {
        Name = CardName.Duel;
    }

    internal override CardRc Play(GameState state)
    {
        var playerId = state.Get.GetPlayerId(state.LivePlayers.Where(p => p.Id != state.CurrentPlayerId).ToList(),
            state.CurrentPlayerId);
        Player? target;
        try
        {
            target = state.Players.First(p => p.Id == playerId);
        }
        catch (InvalidOperationException)
        {
            throw new NotExistingGuidException();
        }
        var curPlayer = target;
        while (curPlayer.CardsInHand.FirstOrDefault(c => c.Name == CardName.Bang) is { } card)
        {
            curPlayer.RemoveCard(card.Id);
            state.CardDeck.Discard(card);
            curPlayer = curPlayer.Id == state.CurrentPlayerId ? target : state.CurrentPlayer;
        }

        curPlayer.ApplyDamage(1,
            new GameState(state.Players, state.CardDeck, curPlayer.Id, state.Get));
        return CardRc.Ok;
    }
}

public sealed class Gatling : InstantCard
{
    public Gatling(CardSuit suit, CardRank rank) : base(suit, rank)
    {
        Name = CardName.Gatling;
    }

    internal override CardRc Play(GameState state)
    {
        foreach (var player in state.LivePlayers.Where(p => p.Id != state.CurrentPlayerId))
            Shoot(state, player.Id);

        return CardRc.Ok;
    }
}

public sealed class CatBalou : InstantCard
{
    public CatBalou(CardSuit suit, CardRank rank) : base(suit, rank)
    {
        Name = CardName.CatBalou;
    }

    internal override CardRc Play(GameState state)
    {
        var playerId = state.Get.GetPlayerId(state.LivePlayers.Where(p => p.Id != state.CurrentPlayerId).ToList(),
            state.CurrentPlayerId);
        Player? target;
        try
        {
            target = state.Players.First(p => p.Id == playerId);
        }
        catch (InvalidOperationException)
        {
            throw new NotExistingGuidException();
        }
        if (target.CardCount == 0)
            return CardRc.CantPlay;
        var cards = new List<Card?>();
        cards.AddRange(target.CardsInHand);
        cards.AddRange(target.CardsOnBoard);
        cards.Add(target.Weapon);
        var cardId = state.Get.GetCardId(cards, target.CardsInHand.Count);
        var card = target.RemoveCard(cardId);
        state.CardDeck.Discard(card);
        return CardRc.Ok;
    }
}

public sealed class Saloon : InstantCard
{
    private const int PlayerHealAmount = 1;
    
    public Saloon(CardSuit suit, CardRank rank) : base(suit, rank)
    {
        Name = CardName.Saloon;
    }

    internal override CardRc Play(GameState state)
    {
        foreach (var player in state.LivePlayers)
            player.Heal(player == state.CurrentPlayer ? PlayerHealAmount + 1 : PlayerHealAmount);

        return CardRc.Ok;
    }
}

public sealed class Stagecoach : InstantCard
{
    private const int CardDrawCount = 2;
    
    public Stagecoach(CardSuit suit, CardRank rank) : base(suit, rank)
    {
        Name = CardName.Stagecoach;
    }

    internal override CardRc Play(GameState state)
    {
        for (var i = 0; i < CardDrawCount; i++)
        {
            var card = state.CardDeck.Draw();
            state.CurrentPlayer.AddCardInHand(card);
        }
        return CardRc.Ok;
    }
}

public sealed class WellsFargo : InstantCard
{
    private const int CardDrawCount = 3;
    
    public WellsFargo(CardSuit suit, CardRank rank) : base(suit, rank)
    {
        Name = CardName.WellsFargo;
    }

    internal override CardRc Play(GameState state)
    {
        for (var i = 0; i < CardDrawCount; i++)
        {
            var card = state.CardDeck.Draw();
            state.CurrentPlayer.AddCardInHand(card);
        }
        return CardRc.Ok;
    }
}
