using DiscordLogUpload.Models;
using DiscordLogUpload.Services;
using System.Text.Json;

// Services
DiscordService discordService = new DiscordService();
FileService fileService = new FileService();

// Configuration
Console.WriteLine("Discord Log-Upload Tool started...");
string configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");

if (fileService.FileExists(configFilePath) == false)
{
    return;
}

// Loading Config
Console.WriteLine("Loading Config File...");

string jsonString = File.ReadAllText(configFilePath);
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

    //Insert name-ip to postgresql
    var connections = fileService.ParseLogFileNew(copyFilePath);
    if (connections.Count > 0)
    {
        DatabaseService databaseService = new DatabaseService();
        databaseService.SetAllConnectionOld();
        databaseService.InsertNameIp(connections);
    }

    var messages = fileService.ParseMessagesFromLog(copyFilePath);
    if (messages.Count > 0 && messages[0] != null)
    {
        DatabaseService databaseService = new DatabaseService();
        databaseService.InsertMessages(messages);
    }

    if (fileService.FileHasContent(copyFilePath) == false)
    {
        Console.WriteLine("File will not be uploaded since it is empty. Exiting...");
        return;
    }
    var success = await discordService.SendFileToDiscord(config.DiscordWebhookUrl, copyFilePath!);

    if (success)
    {
        fileService.CleanUpOldFiles(logFilePath);
    }
}


