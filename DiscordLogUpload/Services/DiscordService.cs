using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordLogUpload.Services;

public class DiscordService
{
    public async Task<bool> SendFileToDiscord(string webhookUrl, string filePath)
    {
        using HttpClient client = new();
        using MultipartFormDataContent form = new();

        form.Add(new StreamContent(File.OpenRead(filePath)), "file", Path.GetFileName(filePath));

        HttpResponseMessage response = await client.PostAsync(webhookUrl, form);

        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine("File sent to Discord successfully.");
        }
        else
        {
            Console.WriteLine($"Failed to send file. Status Code: {response.StatusCode}");
        }

        return response.IsSuccessStatusCode;
    }
}
