using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.KernelMemory;
using Microsoft.KernelMemory.DocumentStorage.DevTools;
using Microsoft.KernelMemory.FileSystem.DevTools;
using Microsoft.KernelMemory.MemoryStorage.DevTools;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.ComponentModel;
using System.Text;
using System.Text.Json;

namespace ChatWithAI;

public class Program
{
    private static readonly string AiModel = "gpt-5-nano";

    private async static Task Main(string[] args)
    {
        var configuration = new ConfigurationBuilder().AddUserSecrets<Program>().Build();

        string openAiApiKey = configuration["OpenAI:ApiKey"] ?? throw new ArgumentException("OpenAi API Key is not present in configuration");
        var builder = Kernel.CreateBuilder().AddOpenAIChatCompletion(AiModel, openAiApiKey);

        var openAiConfig = new OpenAIConfig()
        {
            EmbeddingModel = "text-embedding-3-small",
            TextModel = "gtp-5-nano",
            APIKey = openAiApiKey,
        };

        var memory = new KernelMemoryBuilder()
            .WithSimpleFileStorage(
                new SimpleFileStorageConfig()
                {
                    Directory = @"C:\Users\mk099\Desktop\Work\CodeSphereAI\ChatWithAI\Embeddings\files\",
                    StorageType = FileSystemTypes.Disk
                }
            )
            .WithSimpleVectorDb(
                new SimpleVectorDbConfig()
                {
                    Directory = @"C:\Users\mk099\Desktop\Work\CodeSphereAI\ChatWithAI\Embeddings\embeddings\",
                    StorageType = FileSystemTypes.Disk
                }
            )
            .WithOpenAI(openAiConfig)
        .Build();

        builder.Services.AddSingleton(memory);

        var kernel = builder.Build();

        var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

        kernel.Plugins.AddFromType<FilePlugin>(nameof(FilePlugin), kernel.Services);

        OpenAIPromptExecutionSettings settings = new()
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };

        Console.WriteLine($"Starting the chat with AI, model: {AiModel}");

        ChatHistory history = new();

        string? userRequest = "Hello!";

        while (true)
        {
            history.AddUserMessage(userRequest);
            var result = chatCompletionService.GetStreamingChatMessageContentsAsync(history, executionSettings: settings, kernel);
            WriteColored("AI: ", ConsoleColor.DarkBlue);

            StringBuilder stringBuilder = new();

            await foreach (var message in result)
            {
                Console.Write(message);
                stringBuilder.Append(message);
            }
            Console.WriteLine();
            history.AddAssistantMessage(stringBuilder.ToString() ?? string.Empty);

            WriteColored("User: ", ConsoleColor.Green);
            userRequest = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(userRequest))
            {
                break;
            }
        }
    }

    private static void WriteColored(string message, ConsoleColor color)
    {
        var previousColor = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.Write(message);
        Console.ForegroundColor = previousColor;
    }
}

public class FilePlugin(IKernelMemory memory)
{
    [KernelFunction(nameof(SearchFiles))]
    [Description("Search for file by its name or part of its content")]
    public async Task<string> SearchFiles(string query)
    {
        var searchResult = await memory.SearchAsync(query, limit: 5);

        return searchResult.ToJson();
    }
}