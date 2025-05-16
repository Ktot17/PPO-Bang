namespace BLComponent;

public abstract class EquipmentCard : Card
{
    protected EquipmentCard(CardSuit suit, CardRank rank) : base(suit, rank)
    {
        Type = CardType.Equipment;
    }
}

public abstract class NotJail(CardSuit suit, CardRank rank) : EquipmentCard(suit, rank)
{
    internal override Task<CardRc> Play(GameState state)
    {
        if (state.CurrentPlayer.CardsOnBoard.Any(c => c.Name == Name))
            return Task.FromResult(CardRc.CantPlay);
        state.CurrentPlayer.AddCardOnBoard(this, state.GameView);
        state.GameView.ShowCardResult(state.CurrentPlayerId, Name);
        return Task.FromResult(CardRc.Ok);
    }
}

public sealed class Barrel : NotJail
{
    public Barrel(CardSuit suit, CardRank rank) : base(suit, rank)
    {
        Name = CardName.Barrel;
    }
    
    internal bool ApplyEffect(GameState state, Guid playerId)
    {
        var card = state.CardDeck.Draw(state.GameView);
        state.CardDeck.Discard(card, state.GameView);
        var player = state.Players.First(p => p.Id == playerId);
        var barrel = player.CardsOnBoard.First(c => c.Name == Name);
        state.GameView.ShowCardResult(playerId, Name, card.Suit is CardSuit.Hearts, card);
        if (card.Suit is not CardSuit.Hearts)
            return false;
        state.CardDeck.Discard(player.RemoveCard(barrel.Id), state.GameView);
        return true;
    }
}

public sealed class Scope : NotJail
{
    public Scope(CardSuit suit, CardRank rank) : base(suit, rank)
    {
        Name = CardName.Scope;
    }
}

public sealed class Mustang : NotJail
{
    public Mustang(CardSuit suit, CardRank rank) : base(suit, rank)
    {
        Name = CardName.Mustang;
    }
}

public sealed class Dynamite : NotJail
{
    private const int DynamiteDamage = 3;
    
    public Dynamite(CardSuit suit, CardRank rank) : base(suit, rank)
    {
        Name = CardName.Dynamite;
    }

    internal async Task ApplyEffect(GameState state)
    {
        var card = state.CardDeck.Draw(state.GameView);
        state.CardDeck.Discard(card, state.GameView);
        var player = state.CurrentPlayer;
        var dynamite = player.CardsOnBoard.First(c => c.Name == Name);
        player.RemoveCard(dynamite.Id);
        if (card.Suit is not CardSuit.Spades || card.Rank is < CardRank.Two or > CardRank.Nine)
        {
            var next = state.GetNextPlayer();
            state.GameView.ShowCardResult(next.Id, Name, false, card);
            next.AddCardOnBoard(dynamite, state.GameView);
        }
        else
        {
            state.GameView.ShowCardResult(player.Id, Name, true, card);
            await player.ApplyDamage(DynamiteDamage, state);
            state.CardDeck.Discard(dynamite, state.GameView);
        }
    }
}

public sealed class BeerBarrel : NotJail
{
    private const int BeerBarrelHeal = 2;
    
    public BeerBarrel(CardSuit suit, CardRank rank) : base(suit, rank)
    {
        Name = CardName.BeerBarrel;
    }
    
    internal void ApplyEffect(GameState state)
    {
        var card = state.CardDeck.Draw(state.GameView);
        state.CardDeck.Discard(card, state.GameView);
        var player = state.CurrentPlayer;
        var beerBarrel = player.CardsOnBoard.First(c => c.Name == Name);
        player.RemoveCard(beerBarrel.Id);
        if (card.Suit is not CardSuit.Clubs || card.Rank is < CardRank.Two or > CardRank.Nine)
        {
            var next = state.GetNextPlayer();
            state.GameView.ShowCardResult(next.Id, Name, false, card);
            next.AddCardOnBoard(beerBarrel, state.GameView);
        }
        else
        {
            state.GameView.ShowCardResult(player.Id, Name, true, card);
            player.Heal(BeerBarrelHeal);
            state.CardDeck.Discard(beerBarrel, state.GameView);
        }
    }
}

public sealed class Jail : EquipmentCard
{
    public Jail(CardSuit suit, CardRank rank) : base(suit, rank)
    {
        Name = CardName.Jail;
    }

    internal override async Task<CardRc> Play(GameState state)
    {
        var playerId = await state.GameView.GetPlayerIdAsync(state.LivePlayers.Where(p => p.Id != state.CurrentPlayerId).ToList(),
            state.CurrentPlayerId);
        var target = state.Players.Find(p => p.Id == playerId);
        if (target is null)
            throw new NotExistingGuidException();
        if (target.CardsOnBoard.Any(c => c.Name == Name) ||
            target.Role is PlayerRole.Sheriff)
            return CardRc.CantPlay;
        state.GameView.ShowCardResult(state.CurrentPlayerId, Name, playerId);
        target.AddCardOnBoard(this, state.GameView);
        return CardRc.Ok;
    }
    
    internal bool ApplyEffect(GameState state)
    {
        var card = state.CardDeck.Draw(state.GameView);
        state.CardDeck.Discard(card, state.GameView);
        var player = state.CurrentPlayer;
        var jail = player.CardsOnBoard.First(c => c.Name == Name);
        player.RemoveCard(jail.Id);
        state.CardDeck.Discard(jail, state.GameView);
        state.GameView.ShowCardResult(player.Id, Name, card.Suit is not CardSuit.Hearts, card);
        return card.Suit is not CardSuit.Hearts;
    }
}
