namespace BLComponent;

public class WrongNumberOfPlayersException(int players)
    : Exception($"Wrong number of players. Expected between {GameManager.MinPlayersCount} and " +
                $"{GameManager.MaxPlayersCount}, but got {players}.");
    
public class NotUniqueNamesException() : Exception("Not unique ids for this game.");

public class NotExistingGuidException() : Exception("There are no game object with this Guid.");

public class NotExistingRoleException() : Exception("There are no such role.");
