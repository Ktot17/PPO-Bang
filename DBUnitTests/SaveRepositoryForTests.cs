using DBComponent;
using Microsoft.Extensions.Configuration;

namespace DBUnitTests;

public class SaveRepositoryForTests : SaveRepository
{
    public SaveRepositoryForTests(IConfiguration config) : base(config)
    {
        Connection.Open();
        using var command = Connection.CreateCommand();
        command.CommandText = "DELETE FROM states;";
        command.ExecuteNonQuery();
        Connection.Close();
    }
}
