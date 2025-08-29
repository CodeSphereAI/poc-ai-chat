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

        List<ChatMessage> messages = new();

        while (true)
        {
            Console.Write("Ask anything: ");
            string? request = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(request))
            {
                break;
            }
            messages.Add(new UserChatMessage(request));

            ChatCompletion response = await client.CompleteChatAsync(messages);
            string responseText = response.Content[0].Text;
            Console.WriteLine($"AI replied: {responseText}");

            messages.Add(new AssistantChatMessage(responseText));
        }
    }
}
