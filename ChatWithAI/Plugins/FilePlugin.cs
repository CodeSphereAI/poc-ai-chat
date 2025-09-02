using Microsoft.KernelMemory;
using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatWithAI.Plugins;

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