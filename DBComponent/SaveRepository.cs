using BLComponent;
using BLComponent.InputPorts;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace DBComponent;

public class SaveRepository : ISaveRepository, IDisposable
{
    protected readonly SqliteConnection Connection;

    public SaveRepository(IConfiguration config)
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var dbPath = Path.Combine(appData, config["SavesFileName"]!);
        
        Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);
        Connection = new SqliteConnection($"Data Source={dbPath}");
        Connection.Open();
        
        using var command = Connection.CreateCommand();
        command.CommandText = "CREATE TABLE IF NOT EXISTS states (" + 
                              "id INTEGER PRIMARY KEY AUTOINCREMENT, " + 
                              "created INTEGER NOT NULL, " + 
                              "state TEXT NOT NULL)";
        command.ExecuteNonQuery();
        
        Connection.Close();
    }

    public Dictionary<int, long> GetAll
    {
        get
        {
            Connection.Open();
            using var command = Connection.CreateCommand();
            command.CommandText = "SELECT id, created FROM states ORDER BY created DESC";
            using var reader = command.ExecuteReader();
            var results = new Dictionary<int, long>();
            while (reader.Read())
                results.Add(reader.GetInt32(0), reader.GetInt64(1));
            Connection.Close();
            return results;
        }
    }

    public GameStateDto FindState(int stateId)
    {
        Connection.Open();
        using var command = Connection.CreateCommand();
        command.CommandText = $"SELECT state FROM states WHERE id = {stateId}";
        using var reader = command.ExecuteReader();
        reader.Read();
        var json = reader.GetString(0);
        Connection.Close();
        return JsonConvert.DeserializeObject<GameStateDto>(json)!;
    }

    public void SaveState(GameStateDto state)
    {
        Connection.Open();
        using var command = Connection.CreateCommand();
        var json = JsonConvert.SerializeObject(state);
        command.CommandText = "INSERT INTO states (created, state) " +
            $"VALUES ({DateTimeOffset.UtcNow.ToUnixTimeSeconds()}, '{json}')";
        command.ExecuteNonQuery();
        Connection.Close();
    }

    public void Dispose() => Connection.Dispose();
}
