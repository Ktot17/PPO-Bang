namespace BLComponent;

public class Player(Guid id, PlayerRole role, int maxHealth)
{
    public Guid Id { get; } = id;
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

    internal void AddCardInHand(Card card, IGameView gameView)
    {
        gameView.CardAddedInHand(card.Id, Id);
        _cardsInHand.Add(card);
    }

    internal void AddCardOnBoard(Card card, IGameView gameView)
    {
        gameView.CardAddedOnBoard(card.Id, Id);
        _cardsOnBoard.Add(card);
    }

    internal WeaponCard? ChangeWeapon(WeaponCard weapon, IGameView gameView)
    {
        gameView.WeaponAdded(weapon.Id, Id);
        var removedWeapon = Weapon;
        Weapon = weapon;
        return removedWeapon;
    }

    internal Card RemoveCard(Guid cardId)
    {
        Card? removedCard;
        if ((removedCard = _cardsInHand.Find(c => c.Id == cardId)) is not null)
            _cardsInHand.Remove(removedCard);
        else if ((removedCard = _cardsOnBoard.Find(c => c.Id == cardId)) is not null)
            _cardsOnBoard.Remove(removedCard);
        else if (Weapon is not null && cardId == Weapon.Id)
        {
            removedCard = Weapon;
            Weapon = null;
        }
        else
            throw new NotExistingGuidException();
        return removedCard;
    }
    
    internal void DeadEarlier() => IsDeadOnThisTurn = false;

    internal void BangPlayed() => IsBangPlayed = true;

    internal void EndTurn() => IsBangPlayed = false;
    
    public int CardCount => _cardsInHand.Count + _cardsOnBoard.Count + (Weapon is null ? 0 : 1);

    internal async Task<bool> ApplyDamage(int damage, GameState state)
    {
        Health -= damage;
        if (Health > 0)
            return true;
        while (Health != 1)
        {
            var card = _cardsInHand.Find(c => c.Name == CardName.Beer);
            if (card is null)
            {
                IsDeadOnThisTurn = true;
                return false;
            }

            _cardsInHand.Remove(card);
            if (await card.Play(state) is CardRc.Ok)
                state.CardDeck.Discard(card, state.GameView);
            else
            {
                AddCardInHand(card, state.GameView);
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
        if (rc)
            Health = MaxHealth;
        return rc;
    }
}
