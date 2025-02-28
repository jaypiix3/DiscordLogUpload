

namespace DiscordLogUpload.Models;

public class Config
{
    public string LogFileName { get; set; }
    public string LogPath { get; set; }
    public string BackupFolder { get; set; }
    public string DiscordWebhookUrl { get; set; }
}
