namespace BLComponent.InputPorts;

public interface ICardRepository
{
    public IList<Card> GetAll { get; }
}
