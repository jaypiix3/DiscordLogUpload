using DiscordLogUpload.Models;
using DiscordLogUpload.Services;
using System.Text.Json;

// Services
DiscordService discordService = new DiscordService();
FileService fileService = new FileService();

// Configuration
Console.WriteLine("Discord Log-Upload Tool started...");

if (fileService.FileExists("config.json") == false) return;

// Loading Config
Console.WriteLine("Loading Config File...");

string jsonString = File.ReadAllText("config.json");
Config config = JsonSerializer.Deserialize<Config>(jsonString);

if (config == null)
{
    Console.WriteLine("Failed to load config file. Exiting...");
    return;
}

Console.WriteLine("Config file loaded successfully.");

// Main Logic
var logFilePath = Path.Combine(config.LogPath, config.LogFileName);

if (fileService.FileExists(logFilePath))
{
    var copyFilePath = fileService.ArchiveLogFileByCopy(logFilePath, config.BackupFolder);

    if (copyFilePath == null)
    {
        Console.WriteLine("Failed to archive file. Exiting...");
        return;
    }

    var success = await discordService.SendFileToDiscord(config.DiscordWebhookUrl, copyFilePath!);

    if (success)
    {
        fileService.CleanUpOldFiles(logFilePath);
    }
}

Console.WriteLine("Press any key to exit...");
Console.ReadLine();
