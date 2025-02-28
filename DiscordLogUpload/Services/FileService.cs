namespace DiscordLogUpload.Services;

public class FileService
{
    public bool FileExists(string path)
    {
        if (File.Exists(path)) {
            Console.WriteLine("Log file found: " + path);
            return true;
        }
        else
        {
            Console.WriteLine("Log file not found: " + path);
            return false;
        }
    }

    public bool ArchiveLogFile(string logFilePath, string destinationFilePath)
    {
        try
        {
            File.Move(logFilePath, destinationFilePath);
            Console.WriteLine("Log file archived successfully.");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Failed to archive log file: " + ex.Message);
            return false;
        }
        finally
        {
            File.Create(logFilePath);
            Console.WriteLine("New log file created.");
        }
    }
}
