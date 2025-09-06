using ChatWithAI.Plugins;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.KernelMemory;
using Microsoft.KernelMemory.DocumentStorage.DevTools;
using Microsoft.KernelMemory.FileSystem.DevTools;
using Microsoft.KernelMemory.MemoryStorage.DevTools;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatWithAI;

public class AiClientPoc
{
    private readonly Kernel _kernel;
    private readonly OpenAIPromptExecutionSettings _settings;
    private readonly IChatCompletionService _chatCompletionService;

    private readonly ChatHistory _history = [];

    public AiClientPoc(
        string apiKey,
        string textModel = "gpt-5-nano",
        string embeddingModel = "text-embedding-3-small",
        string filesStorageDirectoryPath = "./files",
        string embeddingsStorageDirectoryPath = "./embeddings")
    {
        var builder = Kernel.CreateBuilder().AddOpenAIChatCompletion(textModel, apiKey);

        var openAiConfig = new OpenAIConfig()
        {
            EmbeddingModel = embeddingModel,
            TextModel = textModel,
            APIKey = apiKey,
        };
        var memory = new KernelMemoryBuilder()
            .WithSimpleFileStorage(
                new SimpleFileStorageConfig()
                {
                    Directory = filesStorageDirectoryPath,
                    StorageType = FileSystemTypes.Disk
                }
            )
            .WithSimpleVectorDb(
                new SimpleVectorDbConfig()
                {
                    Directory = embeddingsStorageDirectoryPath,
                    StorageType = FileSystemTypes.Disk
                }
            )
            .WithOpenAI(openAiConfig)
        .Build();

        builder.Services.AddSingleton(memory);

        _kernel = builder.Build();

        _chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();
        _kernel.Plugins.AddFromType<FilePlugin>(nameof(FilePlugin), _kernel.Services);

        _settings = new OpenAIPromptExecutionSettings()
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };
    }

    public async Task<string> AskAsync(string request)
    {
        _history.AddUserMessage(request);
        var response = await _chatCompletionService.GetChatMessageContentAsync(_history, executionSettings: _settings, _kernel);
        string responseText = response.Content ?? string.Empty;

        _history.AddAssistantMessage(responseText);
        return responseText;
    }
}
