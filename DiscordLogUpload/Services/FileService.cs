namespace DiscordLogUpload.Services;

public class FileService
{
    public bool FileExists(string path)
    {
        if (File.Exists(path)) {
            Console.WriteLine("File found: " + path);
            return true;
        }
        else
        {
            Console.WriteLine("File not found: " + path);
            return false;
        }
    }

    public bool ArchiveLogFile(string logFilePath, string destinationFolderPath)
    {
        string formattedDateFileString = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        string destinationFilePath = Path.Combine(destinationFolderPath, "autoexec_server_" + formattedDateFileString + ".log");

        try
        {
            File.Move(logFilePath, destinationFilePath);
            Console.WriteLine("File archived successfully.");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Failed to archive file: " + ex.Message);
            return false;
        }
        finally
        {
            File.Create(logFilePath);
            Console.WriteLine("New file created.");
        }
    }
}
