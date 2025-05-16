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

    internal override async Task<CardRc> Play(GameState state)
    {
        var player = state.CurrentPlayer;
        if (player.IsBangPlayed && (player.Weapon is null || player.Weapon.Name != CardName.Volcanic))
            return CardRc.CantPlay;
        var playerId = await state.GameView.GetPlayerIdAsync(state.LivePlayers.Where(p => p.Id != state.CurrentPlayerId).ToList(),
            state.CurrentPlayerId);
        if (state.Players.Find(p => p.Id == playerId) is null)
            throw new NotExistingGuidException();
        if (player.Range < state.GetRange(state.CurrentPlayerId, playerId))
            return CardRc.TooFar;
        player.BangPlayed();
        state.GameView.ShowCardResult(state.CurrentPlayerId, Name, false, playerId);
        await Shoot(state, playerId);
        return CardRc.Ok;
    }
}

public sealed class Beer : InstantCard
{
    private const int NoBeerPlayerCount = 2;
    
    public Beer(CardSuit suit, CardRank rank) : base(suit, rank)
    {
        Name = CardName.Beer;
    }

    internal override Task<CardRc> Play(GameState state)
    {
        var player = state.CurrentPlayer;
        if (state.LivePlayers.Count + (state.CurrentPlayer.IsDead ? 1 : 0) <= NoBeerPlayerCount ||
            player.Heal(1))
            return Task.FromResult(CardRc.CantPlay);
        state.GameView.ShowCardResult(state.CurrentPlayerId, Name);
        return Task.FromResult(CardRc.Ok);
    }
}

public sealed class Missed : InstantCard
{
    public Missed(CardSuit suit, CardRank rank) : base(suit, rank)
    {
        Name = CardName.Missed;
    }

    internal override Task<CardRc> Play(GameState state)
    {
        return Task.FromResult(CardRc.CantPlay);
    }
}

public sealed class Panic : InstantCard
{
    public Panic(CardSuit suit, CardRank rank) : base(suit, rank)
    {
        Name = CardName.Panic;
    }

    internal override async Task<CardRc> Play(GameState state)
    {
        var playerId = await state.GameView.GetPlayerIdAsync(state.LivePlayers.Where(p => p.Id != state.CurrentPlayerId).ToList(),
            state.CurrentPlayerId);
        var target = state.Players.Find(p => p.Id == playerId);
        if (target is null)
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
        var cardId = await state.GameView.GetCardIdAsync(cards, target.CardsInHand.Count);
        var card = target.RemoveCard(cardId);
        state.CurrentPlayer.AddCardInHand(card, state.GameView);
        state.GameView.ShowCardResult(state.CurrentPlayerId, Name, target.Id);
        return CardRc.Ok;
    }
}

public sealed class GeneralStore : InstantCard
{
    public GeneralStore(CardSuit suit, CardRank rank) : base(suit, rank)
    {
        Name = CardName.GeneralStore;
    }

    internal override async Task<CardRc> Play(GameState state)
    {
        var cards = new List<Card>();
        for (var i = 0; i < state.LivePlayers.Count; i++)
            cards.Add(state.CardDeck.Draw(state.GameView));
        var chosenCards = new List<Card>();
        var j = 0;
        while (j < state.LivePlayers.Count)
        {
            var cardId = await state.GameView.GetCardIdAsync([.. cards.Except(chosenCards)],
                0, state.CurrentPlayerId);
            var card = cards.Find(c => c.Id == cardId);
            if (card is null)
            {
                state.GameView.ShowCardResult(state.CurrentPlayerId, Name, false);
                continue;
            }

            chosenCards.Add(card);
            state.NextPlayer();
            ++j;
        }

        for (var i = 0; i < state.LivePlayers.Count; i++)
        {
            state.CurrentPlayer.AddCardInHand(chosenCards[i], state.GameView);
            state.NextPlayer();
        }
        state.GameView.ShowCardResult(state.CurrentPlayerId, Name, true);
        return CardRc.Ok;
    }
}

public sealed class Indians : InstantCard
{
    public Indians(CardSuit suit, CardRank rank) : base(suit, rank)
    {
        Name = CardName.Indians;
    }

    internal override async Task<CardRc> Play(GameState state)
    {
        state.GameView.ShowCardResult(state.CurrentPlayerId, Name);
        foreach (var player in state.LivePlayers.Where(p => p.Id != state.CurrentPlayerId))
        {
            var card = player.CardsInHand.FirstOrDefault(c => c.Name is CardName.Bang);
            if (await state.GameView.YesOrNoAsync(player.Id, CardName.Bang) && card is not null)
            {
                state.GameView.ShowCardResult(player.Id, Name, false);
                player.RemoveCard(card.Id);
                state.CardDeck.Discard(card, state.GameView);
            }
            else
            {
                state.GameView.ShowCardResult(player.Id, Name, true);
                await player.ApplyDamage(1,
                    new GameState(state.Players, state.CardDeck, player.Id, state.GameView));
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

    internal override async Task<CardRc> Play(GameState state)
    {
        var playerId = await state.GameView.GetPlayerIdAsync(state.LivePlayers.Where(p => p.Id != state.CurrentPlayerId).ToList(),
            state.CurrentPlayerId);
        var target = state.Players.Find(p => p.Id == playerId);
        if (target is null)
            throw new NotExistingGuidException();
        state.GameView.ShowCardResult(state.CurrentPlayerId, Name);
        var curPlayer = target;
        while (await state.GameView.YesOrNoAsync(curPlayer.Id, CardName.Bang) &&
               curPlayer.CardsInHand.FirstOrDefault(c => c.Name == CardName.Bang) is { } card)
        {
            state.GameView.ShowCardResult(curPlayer.Id, Name, false);
            curPlayer.RemoveCard(card.Id);
            state.CardDeck.Discard(card, state.GameView);
            curPlayer = curPlayer.Id == state.CurrentPlayerId ? target : state.CurrentPlayer;
        }

        state.GameView.ShowCardResult(curPlayer.Id, Name, true);
        await curPlayer.ApplyDamage(1,
            new GameState(state.Players, state.CardDeck, curPlayer.Id, state.GameView));
        return CardRc.Ok;
    }
}

public sealed class Gatling : InstantCard
{
    public Gatling(CardSuit suit, CardRank rank) : base(suit, rank)
    {
        Name = CardName.Gatling;
    }

    internal override async Task<CardRc> Play(GameState state)
    {
        state.GameView.ShowCardResult(state.CurrentPlayerId, Name);
        foreach (var player in state.LivePlayers.Where(p => p.Id != state.CurrentPlayerId))
            await Shoot(state, player.Id);

        return CardRc.Ok;
    }
}

public sealed class CatBalou : InstantCard
{
    public CatBalou(CardSuit suit, CardRank rank) : base(suit, rank)
    {
        Name = CardName.CatBalou;
    }

    internal override async Task<CardRc> Play(GameState state)
    {
        var playerId = await state.GameView.GetPlayerIdAsync(state.LivePlayers.Where(p => p.Id != state.CurrentPlayerId).ToList(),
            state.CurrentPlayerId);
        var target = state.Players.Find(p => p.Id == playerId);
        if (target is null)
            throw new NotExistingGuidException();
        if (target.CardCount == 0)
            return CardRc.CantPlay;
        var cards = new List<Card?>();
        cards.AddRange(target.CardsInHand);
        cards.AddRange(target.CardsOnBoard);
        cards.Add(target.Weapon);
        var cardId = await state.GameView.GetCardIdAsync(cards, target.CardsInHand.Count);
        var card = target.RemoveCard(cardId);
        state.CardDeck.Discard(card, state.GameView);
        state.GameView.ShowCardResult(state.CurrentPlayerId, Name, target.Id, card);
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

    internal override Task<CardRc> Play(GameState state)
    {
        state.GameView.ShowCardResult(state.CurrentPlayerId, Name);
        foreach (var player in state.LivePlayers)
            player.Heal(player == state.CurrentPlayer ? PlayerHealAmount + 1 : PlayerHealAmount);

        return Task.FromResult(CardRc.Ok);
    }
}

public sealed class Stagecoach : InstantCard
{
    private const int CardDrawCount = 2;
    
    public Stagecoach(CardSuit suit, CardRank rank) : base(suit, rank)
    {
        Name = CardName.Stagecoach;
    }

    internal override Task<CardRc> Play(GameState state)
    {
        state.GameView.ShowCardResult(state.CurrentPlayerId, Name);
        for (var i = 0; i < CardDrawCount; i++)
        {
            var card = state.CardDeck.Draw(state.GameView);
            state.CurrentPlayer.AddCardInHand(card, state.GameView);
        }
        return Task.FromResult(CardRc.Ok);
    }
}

public sealed class WellsFargo : InstantCard
{
    private const int CardDrawCount = 3;
    
    public WellsFargo(CardSuit suit, CardRank rank) : base(suit, rank)
    {
        Name = CardName.WellsFargo;
    }

    internal override Task<CardRc> Play(GameState state)
    {
        state.GameView.ShowCardResult(state.CurrentPlayerId, Name);
        for (var i = 0; i < CardDrawCount; i++)
        {
            var card = state.CardDeck.Draw(state.GameView);
            state.CurrentPlayer.AddCardInHand(card, state.GameView);
        }
        return Task.FromResult(CardRc.Ok);
    }
}
