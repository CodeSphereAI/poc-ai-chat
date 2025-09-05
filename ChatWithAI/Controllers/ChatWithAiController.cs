using Microsoft.AspNetCore.Mvc;

namespace ChatWithAI.Controllers;

[ApiController]
[Route("/api/[controller]/")]
public class ChatWithAiController : ControllerBase
{
    [HttpGet]
    public string GetMessage()
    {
        return "Hello, world!";
    }
}
