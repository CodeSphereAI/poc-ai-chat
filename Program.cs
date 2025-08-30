using Microsoft.Extensions.Configuration;
using OpenAI.Chat;
using System.Text.Json;

namespace ChatWithAI;

public class Program
{
    private static readonly string AiModel = "gpt-5-nano";
    private static readonly string FilenameArgument = "fileName";

    private readonly static ChatTool getFileContentTool = ChatTool.CreateFunctionTool(
        functionName: "GetFileContent",
        functionDescription: "Get the content of a file by its name",
        functionParameters: BinaryData.FromBytes(
            """
            {
                "type": "object",
                "properties": {
                    "fileName": {
                        "type": "string",
                        "description": "The name of the file to retrieve content from"
                    }
                }
            }
            """u8.ToArray()
        )
    );

    private readonly static ChatTool getFileByPartOfItContent = ChatTool.CreateFunctionTool(
        functionName: "GetFileContentByPartOfIt",
        functionDescription: "Get the full content of a file by part of its content",
        functionParameters: BinaryData.FromBytes(
            """
            {
                "type": "object",
                "properties": {
                    "partOfFileContent": {
                        "type": "string",
                        "description": "Part of the content of the file to retrieve the full content from"
                    }
                }
            }
            """u8.ToArray())
    );

    private async static Task Main(string[] args)
    {
        var configuration = new ConfigurationBuilder().AddUserSecrets<Program>().Build();

        string openAiApiKey = configuration["OpenAI:ApiKey"] ?? throw new ArgumentException("OpenAi API Key is not present in configuration");
        ChatClient client = new(AiModel, openAiApiKey);

        Console.WriteLine($"Starting the chat with AI, model: {AiModel}");

        ChatCompletion chat = await client.CompleteChatAsync("Hello");
        Console.WriteLine(chat.Content[0].Text);

        List<ChatMessage> messages = new();

        ChatCompletionOptions options = new()
        {
            Tools = { getFileContentTool, getFileByPartOfItContent }
        };

        while (true)
        {
            Console.Write("Ask anything: ");
            string? request = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(request))
            {
                break;
            }
            messages.Add(new UserChatMessage(request));

            ChatCompletion completion = await client.CompleteChatAsync(messages, options);

            switch (completion.FinishReason)
            {
                case ChatFinishReason.Stop:
                {
                    Console.WriteLine($"AI replied: {completion.Content[0].Text}");
                    messages.Add(new AssistantChatMessage(completion));
                    break;
                }
                case ChatFinishReason.ToolCalls:
                {
                    messages.Add(new AssistantChatMessage(completion));

                    foreach (var chatToolCall in completion.ToolCalls)
                    {
                        switch (chatToolCall.FunctionName)
                        {
                            case nameof(GetFileContent):
                            {
                                using JsonDocument arguments = JsonDocument.Parse(chatToolCall.FunctionArguments);
                                bool hasFileName = arguments.RootElement.TryGetProperty(FilenameArgument, out JsonElement fileNameJson);

                                if (!hasFileName)
                                {
                                    break;
                                }

                                string fileContent = GetFileContent(fileNameJson.GetString() ?? throw new ArgumentException("Error occured when parsing fileName"));
                                messages.Add(new ToolChatMessage(chatToolCall.Id, fileContent));

                                break;
                            }
                            case nameof(GetFileContentByPartOfIt):
                            {
                                using JsonDocument arguments = JsonDocument.Parse(chatToolCall.FunctionArguments);
                                bool hasPartOfFileContent = arguments.RootElement.TryGetProperty("partOfFileContent", out JsonElement partOfFileContentJson);
                                if (!hasPartOfFileContent)
                                {
                                    break;
                                }

                                string fileContent = GetFileContentByPartOfIt(partOfFileContentJson.GetString() ?? throw new ArgumentException("Error occured when parsing partOfFileContent"));
                                messages.Add(new ToolChatMessage(chatToolCall.Id, fileContent));
                                break;
                            }
                        }
                    }

                    ChatCompletion toolResponse = await client.CompleteChatAsync(messages);

                    messages.Add(new AssistantChatMessage(toolResponse));
                    Console.WriteLine($"AI replied: {toolResponse.Content[0].Text}");
                    break;
                }
            }       
        }
    }

    private static string GetFileContent(string fileName)
    {
        var file = Directory.GetFiles(@"../../../").FirstOrDefault(fn => fn.Contains(fileName));
        if (file is null)
        {
            return $"There is no such file: {fileName}";
        }

        using FileStream fileStream = File.OpenRead(file);
        using StreamReader stream = new(fileStream);

        string fileContent = stream.ReadToEnd();
        return $"{fileName} content: {fileContent}";
    }

    private static string GetFileContentByPartOfIt(string partOfFileContent)
    {
        var file = Directory.GetFiles(@"../../../").FirstOrDefault(fn =>
        {
            using FileStream fileStream = File.OpenRead(fn);
            using StreamReader stream = new(fileStream);
            string fileContent = stream.ReadToEnd();
            return fileContent.Contains(partOfFileContent);
        });
        if (file is null)
        {
            return $"There is no such file containing the part of content: {partOfFileContent}";
        }

        using FileStream fileStream = File.OpenRead(file);
        using StreamReader stream = new(fileStream);
        string fullFileContent = stream.ReadToEnd();
        return $"The file containing the part of content '{partOfFileContent}' is '{Path.GetFileName(file)}' and its full content is: {fullFileContent}";
    }
}
