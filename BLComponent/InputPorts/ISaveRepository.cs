namespace BLComponent.InputPorts;

public interface ISaveRepository
{
    public Dictionary<int, long> GetAll { get; }
    public GameStateDto FindState(int stateId);
    public void SaveState(GameStateDto state);
}
