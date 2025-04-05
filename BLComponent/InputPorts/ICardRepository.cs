using BLComponent.Cards;

namespace BLComponent.InputPorts;

public interface ICardRepository
{
    List<Card> GetAll();
}