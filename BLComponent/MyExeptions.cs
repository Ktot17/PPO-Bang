namespace BLComponent;

public class WrongNumberOfPlayersException(int players)
    : Exception($"Wrong number of players. Expected between 4 and 7, but got {players}.");
    
public class NotUniqueIdsException() : Exception("Not unique ids for this game.");

public class NotExistingGuidException() : Exception("There are no game object with this Guid.");