using Microsoft.Extensions.Configuration;

namespace DBComponent;

public class Connection(IConfiguration config)
{
    public string? GetDefaultConnectionString => config["ConnectionStrings:DefaultConnection"];
}
