using Microsoft.Extensions.Configuration;
using OpenAI.Chat;

namespace ChatWithAI;

public class Program
{
    private static readonly string AiModel = "gpt-5-nano";

    private async static Task Main(string[] args)
    {
        var configuration = new ConfigurationBuilder().AddUserSecrets<Program>().Build();

        string openAiApiKey = configuration["OpenAI:ApiKey"];
        ChatClient client = new(AiModel, openAiApiKey);

        Console.WriteLine($"Starting the chat with AI, model: {AiModel}");

        ChatCompletion chat = await client.CompleteChatAsync("Hello");
        Console.WriteLine(chat.Content[0].Text);

        while (true)
        {
            Console.Write("Ask anything: ");
            string? request = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(request))
            {
                break;
            }

            ChatCompletion response = await client.CompleteChatAsync(request);
            Console.WriteLine($"AI replied: {response.Content[0].Text}");
        }
    }
}
