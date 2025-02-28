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
            // Open the file with shared read/write access, so other processes can still use it
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Write, FileShare.ReadWrite))
            {
                fs.SetLength(0);
                Console.WriteLine("File content cleared successfully.");
                return true;
            }
        }
        catch (IOException ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return false;
        }
    }

    public bool FileHasContent(string filePath) 
    {
        if (new FileInfo(filePath).Length == 0)
        {
            return false;
        }

        return true;
    }
}
