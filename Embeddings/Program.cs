using Microsoft.Extensions.Configuration;
using Microsoft.KernelMemory;
using Microsoft.KernelMemory.DocumentStorage.DevTools;
using Microsoft.KernelMemory.FileSystem.DevTools;
using Microsoft.KernelMemory.MemoryStorage.DevTools;

namespace PoCEmbeddings;

public class Program
{
    private static readonly string RepoPath = "../../../";

    private async static Task Main(string[] args)
    {
        var configuration = new ConfigurationBuilder().AddUserSecrets<Program>().Build();

        string openAiApiKey = configuration["OpenAI:ApiKey"] ?? throw new ArgumentException("OpenAi API Key is not present in configuration");

        var openAiConfig = new OpenAIConfig()
        {
            EmbeddingModel = "text-embedding-3-small",
            TextModel = "gtp-5-nano",
            APIKey = openAiApiKey,
        };

        var memory = new KernelMemoryBuilder()
            .WithSimpleFileStorage(
                new SimpleFileStorageConfig() { 
                    Directory = @"C:\Users\mk099\Desktop\Work\CodeSphereAI\ChatWithAI\Embeddings\files\",
                    StorageType = FileSystemTypes.Disk
                }
            )
            .WithSimpleVectorDb(
                new SimpleVectorDbConfig() {
                    Directory = @"C:\Users\mk099\Desktop\Work\CodeSphereAI\ChatWithAI\Embeddings\embeddings\",
                    StorageType = FileSystemTypes.Disk
                }
            )
            .WithOpenAI(openAiConfig)
        .Build();

        var files = Directory.GetFiles(RepoPath, "*", SearchOption.TopDirectoryOnly);
        foreach (var file in files)
        {
            Console.WriteLine($"Importing {Path.GetFileName(file)}");

            string content = await File.ReadAllTextAsync(file);
            await memory.ImportTextAsync(content, Guid.NewGuid().ToString());
        }
    }
}
