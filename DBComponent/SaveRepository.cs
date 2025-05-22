using BLComponent;
using BLComponent.InputPorts;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json;

namespace DBComponent;

public sealed class SaveRepository : ISaveRepository, IDisposable
{
    private readonly SqliteConnection _connection;

    public SaveRepository()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var dbPath = Path.Combine(appData, "Bang!", "saves.db");
        
        Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);
        _connection = new SqliteConnection($"Data Source={dbPath}");
        _connection.Open();
        
        using var command = _connection.CreateCommand();
        command.CommandText = "CREATE TABLE IF NOT EXISTS states (" + 
                              "id INTEGER PRIMARY KEY AUTOINCREMENT, " + 
                              "created INTEGER NOT NULL, " + 
                              "state TEXT NOT NULL)";
        command.ExecuteNonQuery();
        
        _connection.Close();
    }

    public Dictionary<int, long> GetAll
    {
        get
        {
            _connection.Open();
            using var command = _connection.CreateCommand();
            command.CommandText = "SELECT id, created FROM states ORDER BY created DESC";
            using var reader = command.ExecuteReader();
            var results = new Dictionary<int, long>();
            while (reader.Read())
                results.Add(reader.GetInt32(0), reader.GetInt64(1));
            _connection.Close();
            return results;
        }
    }

    public GameStateDto FindState(int stateId)
    {
        _connection.Open();
        using var command = _connection.CreateCommand();
        command.CommandText = $"SELECT state FROM states WHERE id = {stateId}";
        using var reader = command.ExecuteReader();
        reader.Read();
        var json = reader.GetString(0);
        _connection.Close();
        return JsonConvert.DeserializeObject<GameStateDto>(json)!;
    }

    public void SaveState(GameStateDto state)
    {
        _connection.Open();
        using var command = _connection.CreateCommand();
        var json = JsonConvert.SerializeObject(state);
        command.CommandText = "INSERT INTO states (created, state) " +
            $"VALUES ({DateTimeOffset.UtcNow.ToUnixTimeSeconds()}, '{json}')";
        command.ExecuteNonQuery();
        _connection.Close();
    }

    public void Dispose() => _connection.Dispose();
}
