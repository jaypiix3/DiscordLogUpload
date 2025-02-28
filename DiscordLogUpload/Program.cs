using DiscordLogUpload.Services;

//Services
DiscordService discordService = new DiscordService();
FileService fileService = new FileService();
// Configuration
string logFileName = "autoexec_server.log";
string logPath = "C:\\logs\\";
string backupFolder = "C:\\logs\\backup";
string discordWebhookUrl = "https://discord.com/api/webhooks/1345022493521608745/Gu2-p5ZTbH5kBdu1GOhB-FtST9EkPUSaKaMSW4BYgtG0GE3d20KYY31dAUMmnJhonUVh";  // Your Discord webhook

Console.WriteLine("Discord Log-Upload Tool started...");

var logFilePath = Path.Combine(logPath, logFileName);
var destinationFilePath = Path.Combine(backupFolder, logFileName);

if (fileService.FileExists(logFilePath))
{
    var success = await discordService.SendFileToDiscord(discordWebhookUrl, logFilePath);

    if (success)
    {
        fileService.ArchiveLogFile(logFilePath, destinationFilePath);
    }
}

Console.WriteLine("Press any key to exit...");
Console.ReadLine();
