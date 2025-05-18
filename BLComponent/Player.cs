using Newtonsoft.Json;

namespace BLComponent;

public record PlayerDto
{
    public PlayerDto() {}

    internal PlayerDto(Player p)
    {
        Id = p.Id;
        Name = p.Name;
        Role = p.Role;
        Health = p.Health;
        MaxHealth = p.MaxHealth;
        Weapon = p.Weapon is null ? null : new WeaponCardDto(p.Weapon);
        CardsInHand = p.CardsInHand.Select(c => new CardDto(c)).ToList();
        CardsOnBoard = p.CardsOnBoard.Select(c => new CardDto(c)).ToList();
        IsBangPlayed = p.IsBangPlayed;
        IsDeadOnThisTurn = p.IsDeadOnThisTurn;
    }
    
    [JsonProperty]
    public Guid Id { get; private set; }
    [JsonProperty]
    public string Name { get; private set; } = string.Empty;
    [JsonProperty]
    public PlayerRole Role { get; private set; }
    [JsonProperty]
    public int Health { get; private set; }
    [JsonProperty]
    public int MaxHealth { get; private set; }
    [JsonProperty] 
    public WeaponCardDto? Weapon { get; private set; } = new();
    [JsonProperty] 
    public IReadOnlyList<CardDto> CardsInHand { get; private set; } = [];
    [JsonProperty] 
    public IReadOnlyList<CardDto> CardsOnBoard { get; private set; } = [];
    [JsonProperty]
    public bool IsBangPlayed { get; private set; }
    [JsonProperty]
    public bool IsDeadOnThisTurn { get; private set; }
}

public class Player
{
    public Guid Id { get; }
    public string Name { get; }
    public PlayerRole Role { get; }
    public int Health { get; private set; }
    public int MaxHealth {get; }
    public WeaponCard? Weapon { get; private set; }
    private readonly List<Card> _cardsInHand = [];
    public IReadOnlyList<Card> CardsInHand => _cardsInHand;
    private readonly List<Card> _cardsOnBoard = [];
    public IReadOnlyList<Card> CardsOnBoard => _cardsOnBoard;
    public bool IsBangPlayed { get; private set; }
    public bool IsDead => Health <= 0;
    public int Range => Weapon?.Range ?? 1;
    public bool IsDeadOnThisTurn { get; private set; }

    public Player(Guid id, string name, PlayerRole role, int maxHealth)
    {
        Id = id;
        Name = name;
        Role = role;
        Health = maxHealth;
        MaxHealth = maxHealth;
    }
    
    internal Player(PlayerDto dto)
    {
        Id = dto.Id;
        Name = dto.Name;
        Role = dto.Role;
        Health = dto.Health;
        MaxHealth = dto.MaxHealth;
        if (dto.Weapon is not null)
            Weapon = (WeaponCard)CardFactory.CreateCard(dto.Weapon.Name, dto.Weapon.Suit, dto.Weapon.Rank);
        foreach (var cardDto in dto.CardsInHand)
            _cardsInHand.Add(CardFactory.CreateCard(cardDto.Name, cardDto.Suit, cardDto.Rank));
        foreach (var cardDto in dto.CardsOnBoard)
            _cardsOnBoard.Add(CardFactory.CreateCard(cardDto.Name, cardDto.Suit, cardDto.Rank));
        IsBangPlayed = dto.IsBangPlayed;
        IsDeadOnThisTurn = dto.IsDeadOnThisTurn;
    }

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
