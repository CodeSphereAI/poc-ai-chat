using ChatWithAI.Extensions;
using ChatWithAI.Plugins;
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


//public class Program
//{
//    private static readonly string AiModel = "gpt-5-nano";

//    private async static Task Main(string[] args)
//    {
//        var configuration = new ConfigurationBuilder().AddUserSecrets<Program>().Build();

//        string openAiApiKey = configuration["OpenAI:ApiKey"] ?? throw new ArgumentException("OpenAi API Key is not present in configuration");

//        AiClientPoc aiClient = new(openAiApiKey, filesStorageDirectoryPath: @"C:\Users\mk099\Desktop\Work\CodeSphereAI\ChatWithAI\Embeddings\files\", embeddingsStorageDirectoryPath: @"C:\Users\mk099\Desktop\Work\CodeSphereAI\ChatWithAI\Embeddings\embeddings\");

//        Console.WriteLine($"Starting the chat with AI, model: {AiModel}");

//        string? userRequest = "Hello!";

//        while (true)
//        {
//            var result = await aiClient.AskAsync(userRequest);
//            ConsoleWriteExtension.WriteColored("AI: ", ConsoleColor.DarkBlue);
//            Console.WriteLine(result);

//            ConsoleWriteExtension.WriteColored("User: ", ConsoleColor.Green);
//            userRequest = Console.ReadLine();

//            if (string.IsNullOrWhiteSpace(userRequest))
//            {
//                break;
//            }
//        }
//    }  
//}

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders().AddConsole();

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddRouting(options => options.LowercaseUrls = true);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policyBuilder => policyBuilder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

app.UseHttpsRedirection();

app.MapControllers();

await app.RunAsync();