using DiscordLogUpload.Models;
using Npgsql;

namespace DiscordLogUpload.Services;

public class DatabaseService
{
    public readonly string connectionString = "Server=5.199.135.196;Database=servermanager;User Id=postgres;Password=Gummiente247;";

    public void InsertNameIp(List<Connection> connections)
    {
        using (NpgsqlConnection npgsqlConnection = new NpgsqlConnection(connectionString))
        {
            npgsqlConnection.Open();

            foreach (var connection in connections)
            {
                if (connection.Name == null || connection.Ip == null)
                {
                    continue;
                }

                using (NpgsqlCommand cmd = new NpgsqlCommand())
                {
                    cmd.Connection = npgsqlConnection;
                    cmd.CommandText = "INSERT INTO connections (id, name, ip, old, version) VALUES (@id, @name, @ip, @old, @version)";
                    cmd.Parameters.AddWithValue("name", connection.Name);
                    cmd.Parameters.AddWithValue("ip", connection.Ip);
                    cmd.Parameters.AddWithValue("id", Guid.NewGuid());
                    cmd.Parameters.AddWithValue("old", false);
                    cmd.Parameters.AddWithValue("version", connection.Version);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }

    public void InsertMessages(List<Message> messages)
    {
        using (NpgsqlConnection npgsqlConnection = new NpgsqlConnection(connectionString))
        {
            npgsqlConnection.Open();

            foreach (var message in messages)
            {
                using (NpgsqlCommand cmd = new NpgsqlCommand())
                {
                    cmd.Connection = npgsqlConnection;
                    cmd.CommandText = "INSERT INTO messages (id, timestamp, player, message_text) VALUES (@id, @timestamp, @player, @message_text)";
                    cmd.Parameters.AddWithValue("id", Guid.NewGuid());
                    cmd.Parameters.AddWithValue("timestamp", message.Timestamp!);
                    cmd.Parameters.AddWithValue("player", message.Player);
                    cmd.Parameters.AddWithValue("message_text", message.MessageText);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }




    public bool NameIpCombinationExists(NpgsqlConnection sqlCon, Connection connection)
    {
        using (NpgsqlCommand cmd = new NpgsqlCommand())
        {
            cmd.Connection = sqlCon;
            cmd.CommandText = "SELECT COUNT(*) FROM connections WHERE name = @name AND ip = @ip";
            cmd.Parameters.AddWithValue("name", connection.Name);
            cmd.Parameters.AddWithValue("ip", connection.Ip);
            object? result = cmd.ExecuteScalar();
            int count = result != null ? (int)(long)result : 0;
            return count > 0;
        }
    }

    public void SetAllConnectionOld()
    {
        using (NpgsqlConnection npgsqlConnection = new NpgsqlConnection(connectionString))
        {
            npgsqlConnection.Open();
            using (NpgsqlCommand cmd = new NpgsqlCommand())
            {
                cmd.Connection = npgsqlConnection;
                cmd.CommandText = "UPDATE connections SET old = @old";
                cmd.Parameters.AddWithValue("old", true);
                cmd.ExecuteNonQuery();
            }
        }
    }
}
