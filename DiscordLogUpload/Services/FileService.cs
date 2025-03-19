using System.Text.RegularExpressions;
using System.Text;
using DiscordLogUpload.Models;

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


    public List<Connection> ParseLogFile(string filePath)
    {
        List<Connection> logResults = new List<Connection>();
        string[] lines = File.ReadAllLines(filePath, Encoding.UTF8);

        // Regex patterns
        Regex serverRegex = new Regex(@"ClientId=(\d+) addr=<{([\d\.]+):\d+}>");
        Regex chatRegex = new Regex(@"\*\*\* '([^']+)' entered and joined the game");

        // Temporary storage to map ClientId -> IP
        Dictionary<int, string> clientIpMap = new Dictionary<int, string>();

        // Process log file line by line
        foreach (string line in lines)
        {
            // Check for IP and ClientId
            Match serverMatch = serverRegex.Match(line);
            if (serverMatch.Success)
            {
                int clientId = int.Parse(serverMatch.Groups[1].Value);
                string ip = serverMatch.Groups[2].Value;
                clientIpMap[clientId] = ip;  // Store ClientId -> IP mapping
                continue;
            }

            // Check for player join messages
            Match chatMatch = chatRegex.Match(line);
            if (chatMatch.Success)
            {
                string playerName = chatMatch.Groups[1].Value;

                // Find the closest matching ClientId (first available one)
                if (clientIpMap.Count > 0)
                {
                    int clientId = new List<int>(clientIpMap.Keys)[0];  // Get first available ClientId
                    string ip = clientIpMap[clientId];

                    // Add to result list
                    logResults.Add(new Connection { Name = playerName, Ip = ip });

                    // Remove used ClientId to avoid incorrect reuse
                    clientIpMap.Remove(clientId);
                }
            }
        }

        return logResults;
    }

    public List<Connection> ParseLogFileNew(string filePath)
    {
        List<Connection> logResults = new List<Connection>();
        string[] lines = File.ReadAllLines(filePath, Encoding.UTF8);

        // Regex patterns
        Regex serverRegex = new Regex(@"ClientId=(\d+) addr=<{([\d\.]+):\d+}>");
        Regex chatRegex = new Regex(@"\*\*\* '([^']+)' entered and joined the game");
        Regex versionRegex = new Regex(@"cid=(\d+) version=(\d+)");

        // Temporary storage for processing one player at a time
        Connection pendingConnection = null;
        int? pendingClientId = null;

        foreach (string line in lines)
        {
            // Check for IP and ClientId
            Match serverMatch = serverRegex.Match(line);
            if (serverMatch.Success)
            {
                pendingClientId = int.Parse(serverMatch.Groups[1].Value);
                string ip = serverMatch.Groups[2].Value;
                pendingConnection = new Connection { Ip = ip };
                continue;
            }

            // Check for player join message
            Match chatMatch = chatRegex.Match(line);
            if (chatMatch.Success && pendingConnection != null)
            {
                pendingConnection.Name = chatMatch.Groups[1].Value;
                continue;
            }

            // Check for version info
            Match versionMatch = versionRegex.Match(line);
            if (versionMatch.Success && pendingClientId.HasValue)
            {
                int clientId = int.Parse(versionMatch.Groups[1].Value);
                if (clientId == pendingClientId.Value && pendingConnection != null)  // Ensure correct ClientId
                {
                    pendingConnection.Version = versionMatch.Groups[2].Value;

                    // Add to results once all data is available
                    logResults.Add(pendingConnection);

                    // Reset temporary storage
                    pendingConnection = null;
                    pendingClientId = null;
                }
            }
        }

        return logResults;
    }


    public List<Connection> ParseLogFileNewOld(string filePath)
    {
        List<Connection> logResults = new List<Connection>();
        string[] lines = File.ReadAllLines(filePath, Encoding.UTF8);

        // Regex patterns
        Regex serverRegex = new Regex(@"ClientId=(\d+) addr=<{([\d\.]+):\d+}>");
        Regex chatRegex = new Regex(@"\*\*\* '([^']+)' entered and joined the game");
        Regex versionRegex = new Regex(@"cid=(\d+) version=(\d+)");

        // Temporary storage to map ClientId -> (IP, Version)
        Dictionary<int, Tuple<string, string>> clientInfoMap = new Dictionary<int, Tuple<string, string>>();

        // Process log file line by line
        foreach (string line in lines)
        {
            // Check for IP and ClientId
            Match serverMatch = serverRegex.Match(line);
            if (serverMatch.Success)
            {
                int clientId = int.Parse(serverMatch.Groups[1].Value);
                string ip = serverMatch.Groups[2].Value;

                // Store ClientId -> IP and placeholder for Version (null initially)
                if (!clientInfoMap.ContainsKey(clientId))
                {
                    clientInfoMap[clientId] = new Tuple<string, string>(ip, null);
                }
                continue;
            }

            // Check for version info (cid and version)
            Match versionMatch = versionRegex.Match(line);
            if (versionMatch.Success)
            {
                int clientId = int.Parse(versionMatch.Groups[1].Value);
                string version = versionMatch.Groups[2].Value;

                // If the ClientId already exists, update the version in the map
                if (clientInfoMap.ContainsKey(clientId))
                {
                    string ip = clientInfoMap[clientId].Item1;
                    clientInfoMap[clientId] = new Tuple<string, string>(ip, version);  // Update version
                }
                continue;  // Skip to the next line after handling version info
            }

            // Check for player join messages
            Match chatMatch = chatRegex.Match(line);
            if (chatMatch.Success)
            {
                string playerName = chatMatch.Groups[1].Value;

                // Find the first ClientId with a matching IP and Version
                foreach (var clientEntry in clientInfoMap)
                {
                    if (clientEntry.Value.Item2 != null)  // Only add if version is not null
                    {
                        string ip = clientEntry.Value.Item1;
                        string version = clientEntry.Value.Item2;

                        // Add the connection information
                        logResults.Add(new Connection { Name = playerName, Ip = ip, Version = version });

                        // Remove the ClientId entry to avoid reusing it
                        clientInfoMap.Remove(clientEntry.Key);
                        break;  // Exit loop once a match is found
                    }
                }
            }
        }
        return logResults;
    }

    public List<Message> ParseMessagesFromLogOld(string filePath)
    {
        List<Message> messages = new List<Message>();
        string[] lines = File.ReadAllLines(filePath, Encoding.UTF8);

        // Regex pattern to capture player and message
        Regex chatRegex = new Regex(@"I chat: \d+:-2:([^:]+): ([^\r\n]+)");

        // Process log file line by line
        foreach (string line in lines)
        {
            // Check for chat message lines using the regex
            Match chatMatch = chatRegex.Match(line);
            if (chatMatch.Success)
            {
                // Extract player and message from the matched groups
                string player = chatMatch.Groups[1].Value;
                string messageText = chatMatch.Groups[2].Value;

                // Create a new Message object and add it to the result list
                messages.Add(new Message { Player = player, MessageText = messageText });
            }
        }

        return messages;
    }

    public List<Message> ParseMessagesFromLog(string filePath)
    {
        List<Message> messages = new List<Message>();
        string[] lines = File.ReadAllLines(filePath, System.Text.Encoding.UTF8);

        // Regex pattern to capture timestamp, player, and message
        Regex chatRegex = new Regex(@"(\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}) I chat: \d+:-2:([^:]+): ([^\r\n]+)");

        // Process log file line by line
        foreach (string line in lines)
        {
            // Check for chat message lines using the regex
            Match chatMatch = chatRegex.Match(line);
            if (chatMatch.Success)
            {
                // Extract timestamp, player, and message from the matched groups
                string timestampStr = chatMatch.Groups[1].Value;
                string player = chatMatch.Groups[2].Value;
                string messageText = chatMatch.Groups[3].Value;

                // Convert timestamp string to DateTime
                DateTime timestamp = DateTime.ParseExact(timestampStr, "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);

                // Create a new Message object and add it to the result list
                messages.Add(new Message { Timestamp = timestamp, Player = player, MessageText = messageText });
            }
        }

        return messages;
    }
}
