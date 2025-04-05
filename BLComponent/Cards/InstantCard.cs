namespace BLComponent.Cards;

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

    public override CardRc Play(GameContext context)
    {
        var player = context.Players[context.CurrentPlayer];
        if (player.IsBangPlayed && (player.Weapon is null || player.Weapon.Name != CardName.Volcanic))
            return CardRc.CantPlay;
        var playerIndex = GetTarget(context);
        if (player.Range < context.GetRange(context.CurrentPlayer, playerIndex))
            return CardRc.TooFar;
        player.BangPlayed();
        Shoot(context, playerIndex);
        return CardRc.Ok;
    }
}

public sealed class Beer : InstantCard
{
    public Beer(CardSuit suit, CardRank rank) : base(suit, rank)
    {
        Name = CardName.Beer;
    }

    public override CardRc Play(GameContext context)
    {
        var player = context.Players[context.CurrentPlayer];
        if (context.Players.Count(p => !p.IsDead) <= 2 ||
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

    public override CardRc Play(GameContext context)
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

    public override CardRc Play(GameContext context)
    {
        var playerIndex = GetTarget(context);
        var target = context.Players[playerIndex];
        if (1 < context.GetRange(context.CurrentPlayer, playerIndex))
            return CardRc.TooFar;
        if (target.CardCount == 0)
            return CardRc.CantPlay;
        var cards = new List<Card?>();
        cards.AddRange(target.CardsInHand);
        cards.AddRange(target.CardsOnBoard);
        cards.Add(target.Weapon);
        var cardIndex = context.Get.GetCardIndex(cards, target.CardsInHand.Count);
        var card = target.RemoveCard(cardIndex);
        context.Players[context.CurrentPlayer].AddCardInHand(card);
        return CardRc.Ok;
    }
}

public sealed class GeneralStore : InstantCard
{
    public GeneralStore(CardSuit suit, CardRank rank) : base(suit, rank)
    {
        Name = CardName.GeneralStore;
    }

    public override CardRc Play(GameContext context)
    {
        var cards = new List<Card>();
        for (var i = 0; i < context.Players.Count; i++)
            cards.Add(context.CardDeck.Draw());
        var playerIndex = context.CurrentPlayer;
        do
        {
            var card = context.Get.GetCardIndex(cards, 0, context.Players[playerIndex].Id);
            context.Players[playerIndex].AddCardInHand(cards[card]);
            cards.RemoveAt(card);
            playerIndex = (playerIndex + 1) % context.Players.Count;
        } while (playerIndex != context.CurrentPlayer);
        return CardRc.Ok;
    }
}

public sealed class Indians : InstantCard
{
    public Indians(CardSuit suit, CardRank rank) : base(suit, rank)
    {
        Name = CardName.Indians;
    }

    public override CardRc Play(GameContext context)
    {
        var playerIndex = (context.CurrentPlayer + 1) % context.Players.Count;
        while (playerIndex != context.CurrentPlayer)
        {
            var cardIndex = context.Players[playerIndex].CardsInHand.ToList().FindIndex(c => c.Name is CardName.Bang);
            if (cardIndex == -1)
            {
                context.Players[playerIndex].ApplyDamage(1,
                    new GameContext(context.Players, context.CardDeck, playerIndex, context.Get));
            }
            else
            {
                var card = context.Players[playerIndex].RemoveCard(cardIndex);
                context.CardDeck.Discard(card);
            }
            playerIndex = (playerIndex + 1) % context.Players.Count;
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

    public override CardRc Play(GameContext context)
    {
        var playerIndex = GetTarget(context);
        var cur = playerIndex;
        int cardIndex;
        while ((cardIndex = context.Players[cur].CardsInHand.ToList().FindIndex(c => c.Name == CardName.Bang)) != -1)
        {
            var card = context.Players[cur].RemoveCard(cardIndex);
            context.CardDeck.Discard(card);
            cur = cur == playerIndex ? context.CurrentPlayer : playerIndex;
        }

        context.Players[cur].ApplyDamage(1,
            new GameContext(context.Players, context.CardDeck, cur, context.Get));
        return CardRc.Ok;
    }
}

public sealed class Gatling : InstantCard
{
    public Gatling(CardSuit suit, CardRank rank) : base(suit, rank)
    {
        Name = CardName.Gatling;
    }

    public override CardRc Play(GameContext context)
    {
        var playerIndex = (context.CurrentPlayer + 1) % context.Players.Count;
        while (playerIndex != context.CurrentPlayer)
        {
            Shoot(context, playerIndex);
            playerIndex = (playerIndex + 1) % context.Players.Count;
        }

        return CardRc.Ok;
    }
}

public sealed class CatBalou : InstantCard
{
    public CatBalou(CardSuit suit, CardRank rank) : base(suit, rank)
    {
        Name = CardName.CatBalou;
    }

    public override CardRc Play(GameContext context)
    {
        var playerIndex = GetTarget(context);
        var target = context.Players[playerIndex];
        if (target.CardCount == 0)
            return CardRc.CantPlay;
        var cards = new List<Card?>();
        cards.AddRange(target.CardsInHand);
        cards.AddRange(target.CardsOnBoard);
        cards.Add(target.Weapon);
        var cardIndex = context.Get.GetCardIndex(cards, target.CardsInHand.Count);
        var card = target.RemoveCard(cardIndex);
        context.CardDeck.Discard(card);
        return CardRc.Ok;
    }
}

public sealed class Saloon : InstantCard
{
    public Saloon(CardSuit suit, CardRank rank) : base(suit, rank)
    {
        Name = CardName.Saloon;
    }

    public override CardRc Play(GameContext context)
    {
        context.Players[context.CurrentPlayer].Heal(2);
        var playerIndex = (context.CurrentPlayer + 1) % context.Players.Count;
        while (playerIndex != context.CurrentPlayer)
        {
            context.Players[playerIndex].Heal(1);
            playerIndex = (playerIndex + 1) % context.Players.Count;
        }
        return CardRc.Ok;
    }
}

public sealed class Stagecoach : InstantCard
{
    public Stagecoach(CardSuit suit, CardRank rank) : base(suit, rank)
    {
        Name = CardName.Stagecoach;
    }

    public override CardRc Play(GameContext context)
    {
        for (var i = 0; i < 2; i++)
        {
            var card = context.CardDeck.Draw();
            context.Players[context.CurrentPlayer].AddCardInHand(card);
        }
        return CardRc.Ok;
    }
}

public sealed class WellsFargo : InstantCard
{
    public WellsFargo(CardSuit suit, CardRank rank) : base(suit, rank)
    {
        Name = CardName.WellsFargo;
    }

    public override CardRc Play(GameContext context)
    {
        for (var i = 0; i < 3; i++)
        {
            var card = context.CardDeck.Draw();
            context.Players[context.CurrentPlayer].AddCardInHand(card);
        }
        return CardRc.Ok;
    }
}