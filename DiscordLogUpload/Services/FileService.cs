namespace DiscordLogUpload.Services;

public class FileService
{
    public bool FileExists(string path)
    {
        if (File.Exists(path))
        {
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

    public string? ArchiveLogFileByCopy(string logFilePath, string destinationFolderPath)
    {
        string formattedDateFileString = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        string destinationFilePath = Path.Combine(destinationFolderPath, "autoexec_server_" + formattedDateFileString + ".log");
        string result = null;
        try
        {
            File.Copy(logFilePath, destinationFilePath);
            Console.WriteLine("File archived successfully.");
            result = destinationFilePath;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Failed to archive file: " + ex.Message);
        }
        
        return result;
    }

    public bool CleanUpOldFiles(string filePath)
    {
        try
        {
            File.Delete(filePath);
            Console.WriteLine("Old file deleted successfully.");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Failed to delete old file: " + ex.Message);
            return false;
        }
        finally
        {
            File.Create(filePath);
            Console.WriteLine("New file created.");
        }
    }
}
