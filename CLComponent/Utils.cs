using System.Text;
using BLComponent;

namespace CLComponent;

public class Utils
{
    private readonly Dictionary<CardRank, string> _ranks = new()
    {
        [CardRank.Ace] = "A",
        [CardRank.King] = "K",
        [CardRank.Queen] = "Q",
        [CardRank.Jack] = "J",
        [CardRank.Ten] = "10",
        [CardRank.Nine] = "9",
        [CardRank.Eight] = "8",
        [CardRank.Seven] = "7",
        [CardRank.Six] = "6",
        [CardRank.Five] = "5",
        [CardRank.Four] = "4",
        [CardRank.Three] = "3",
        [CardRank.Two] = "2",
    };

    private readonly Dictionary<CardSuit, char> _suits = new()
    {
        [CardSuit.Spades] = '\u2660',
        [CardSuit.Diamonds] = '\u2666',
        [CardSuit.Clubs] = '\u2663',
        [CardSuit.Hearts] = '\u2665',
    };

    private readonly Dictionary<PlayerRole, string> _roles = new()
    {
        [PlayerRole.Sheriff] = "Шериф",
        [PlayerRole.Outlaw] = "Бандит",
        [PlayerRole.DeputySheriff] = "Помощник шерифа",
        [PlayerRole.Renegade] = "Ренегат",
    };
    
    public IReadOnlyDictionary<PlayerRole, string> RolesToString => _roles;
    
    private readonly Dictionary<CardName, string> _cardNames = new()
    {
        [CardName.Bang] = "Бэнг!",
        [CardName.Beer] = "Пиво",
        [CardName.Missed] = "Мимо",
        [CardName.Panic] = "Паника",
        [CardName.GeneralStore] = "Магазин",
        [CardName.Indians] = "Индейцы",
        [CardName.Duel] = "Дуэль",
        [CardName.Gatling] = "Гатлинг",
        [CardName.CatBalou] = "Красотка",
        [CardName.Saloon] = "Салун",
        [CardName.Stagecoach] = "Диллижанс",
        [CardName.WellsFargo] = "Уэллс Фарго",
        [CardName.Barrel] = "Бочка",
        [CardName.Scope] = "Прицел",
        [CardName.Mustang] = "Мустанг",
        [CardName.Dynamite] = "Динамит",
        [CardName.BeerBarrel] = "Бочка с пивом",
        [CardName.Jail] = "Тюрьма",
        [CardName.Volcanic] = "Волканик",
        [CardName.Schofield] = "Скофилд",
        [CardName.Remington] = "Ремингтон",
        [CardName.Carabine] = "Карабин",
        [CardName.Winchester] = "Винчестер"
    };
    
    public IReadOnlyDictionary<CardName, string> CardNames => _cardNames;
    
    public string CardToString(Card card) => $"{_cardNames[card.Name]} {_ranks[card.Rank]}{_suits[card.Suit]}";
    
    public string PlayerToString(Player player, bool isCur)
    {
        var role = _roles[player.Role];

        var stringBuilder = new StringBuilder($"{player.Name}");
        if (player.IsDead)
            stringBuilder.Append(" (Мёртв)");
        stringBuilder.Append('\n');
        if (isCur || player.Role == PlayerRole.Sheriff || player.IsDead)
            stringBuilder.Append("Роль: " + role + "\n");
        if (player.IsDead)
        {
            return stringBuilder.ToString();
        }
        stringBuilder.Append($"Здоровье: {player.Health}/{player.MaxHealth}\n" +
                             "Карты в руке:\n");
        for (var i = 0; i < player.CardsInHand.Count; i++)
        {
            var card = isCur ? CardToString(player.CardsInHand[i]) : "Неизвестная карта";
            stringBuilder.Append($"  {i + 1}. {card}\n");
        }

        stringBuilder.Append("Карты на столе:\n");
        for (var i = player.CardsInHand.Count; i < player.CardsOnBoard.Count + player.CardsInHand.Count; i++)
            stringBuilder.Append($"  {i + 1}. {CardToString(player.CardsOnBoard[i - player.CardsInHand.Count])}\n");
        stringBuilder.Append(player.Weapon is not null ? $"Оружие:\n  {player.CardsOnBoard.Count + player.CardsInHand.Count + 1}. " +
                                 CardToString(player.Weapon) + "\n" : "Оружие:\n  Нет оружия\n");
        return stringBuilder.ToString();
    }
}
