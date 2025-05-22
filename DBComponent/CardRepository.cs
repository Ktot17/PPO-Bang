using BLComponent;
using BLComponent.InputPorts;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace DBComponent;

public class CardRepository(IConfiguration config) : ICardRepository
{
    private readonly Connection _connection = new(config);

    public IList<Card> GetAll
    {
        get
        {
            var conn = new NpgsqlConnection(_connection.GetDefaultConnectionString);
            var cards = new List<Card>();
            conn.Open();
            using var cmd = new NpgsqlCommand("SELECT * FROM decks.ClassicDeck", conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                cards.Add(CardFactory.CreateCard((CardName)reader["Name"],
                    (CardSuit)reader["Suit"], (CardRank)reader["Rank"]));
            conn.Close();
            return cards;
        }
    }
}
