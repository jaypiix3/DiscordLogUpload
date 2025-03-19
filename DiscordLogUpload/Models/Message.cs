
namespace DiscordLogUpload.Models;

public class Message
{
    public Guid? Id { get; set; }
    public DateTime? Timestamp { get; set; }
    public required string Player { get; set; }
    public required string MessageText { get; set; }
}
