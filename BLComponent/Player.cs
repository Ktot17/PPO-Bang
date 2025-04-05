using BLComponent.Cards;

namespace BLComponent;

public class Player(int id, PlayerRole role, int maxHealth)
{
    public int Id { get; } = id;
    public PlayerRole Role { get; } = role;
    public int Health { get; private set; } = maxHealth;
    public int MaxHealth {get; } = maxHealth;
    public WeaponCard? Weapon { get; private set; }
    private readonly List<Card> _cardsInHand = [];
    public IReadOnlyList<Card> CardsInHand => _cardsInHand;
    private readonly List<Card> _cardsOnBoard = [];
    public IReadOnlyList<Card> CardsOnBoard => _cardsOnBoard;
    public bool IsBangPlayed { get; private set; }
    public bool IsDead => Health <= 0;
    public int Range => Weapon?.Range ?? 1;
    public bool IsDeadOnThisTurn { get; private set; }

    internal void AddCardInHand(Card card) => _cardsInHand.Add(card);

    internal void AddCardOnBoard(Card card) => _cardsOnBoard.Add(card);

    internal WeaponCard? ChangeWeapon(WeaponCard weapon)
    {
        var removedWeapon = Weapon;
        Weapon = weapon;
        return removedWeapon;
    }

    internal Card RemoveCard(int index)
    {
        if (index < 0 || index >= CardCount)
            throw new DiscardNotExistingCardException();
        Card removedCard;
        if (index < _cardsInHand.Count)
        {
            removedCard = _cardsInHand[index];
            _cardsInHand.RemoveAt(index);
        }
        else if (index < _cardsOnBoard.Count + _cardsInHand.Count)
        {
            index -= _cardsInHand.Count;
            removedCard = _cardsOnBoard[index];
            _cardsOnBoard.RemoveAt(index);
        }
        else
        {
            removedCard = Weapon!;
            Weapon = null;
        }
        return removedCard;
    }
    
    internal void DeadEarlier() => IsDeadOnThisTurn = false;

    internal void BangPlayed() => IsBangPlayed = true;

    internal void EndTurn() => IsBangPlayed = false;
    
    public int CardCount => _cardsInHand.Count + _cardsOnBoard.Count + (Weapon == null ? 0 : 1);

    internal bool ApplyDamage(int damage, GameContext context)
    {
        Health -= damage;
        if (Health > 0) return true;
        while (Health != 1)
        {
            var cardIndex = _cardsInHand.FindIndex(card => card.Name == CardName.Beer);
            if (cardIndex == -1)
            {
                IsDeadOnThisTurn = true;
                return false;
            }

            var card = RemoveCard(cardIndex);
            if (card.Play(context) is CardRc.Ok)
                context.CardDeck.Discard(card);
            else
            {
                AddCardInHand(card);
                IsDeadOnThisTurn = true;
                return false;
            }
        }
        return true;
    }

    internal bool Heal(int healAmount)
    {
        Health += healAmount;
        var rc = Health > MaxHealth;
        if (rc) Health = MaxHealth;
        return rc;
    }
}