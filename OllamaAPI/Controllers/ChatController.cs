using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel.ChatCompletion;
using OllamaAPI.Entities;

namespace OllamaAPI.Controllers;

[ApiController]
[Route("api/chat")]
public class ChatController : ControllerBase
{
    private readonly IChatCompletionService _chatCompletionService;
    private readonly ChatHistory _history;
    private readonly IConfiguration _configuration;

    public ChatController(IChatCompletionService chatCompletionService,
                          ChatHistory history, IConfiguration configuration)
    {
        _chatCompletionService = chatCompletionService;
        _history = history;
        _configuration = configuration;

        // Adiciona as mensagens do sistema e do assistente a partir de appsettings.json
        var systemMessage = _configuration["ModelContextSettings:SystemMessage"];
        var assistantMessage = _configuration["ModelContextSettings:AssistantMessage"];

        if (!string.IsNullOrEmpty(systemMessage))
            _history.AddSystemMessage(systemMessage);

        if (!string.IsNullOrEmpty(assistantMessage))
            _history.AddAssistantMessage(assistantMessage);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Post([FromBody] ChatPrompt chatPrompt)
    {
        if (string.IsNullOrEmpty(chatPrompt.Message))
            return BadRequest("Informe uma mensagem");

        _history.AddUserMessage(chatPrompt.Message);
        var response = await _chatCompletionService
                             .GetChatMessageContentAsync(chatPrompt.Message);

        if (response.Content == null)
        {
            return StatusCode(500, "Internal Server Error: " +
                                   "O serviço de Chat retornou uma resposta nula.");
        }
        _history.AddUserMessage(response.Content ?? string.Empty);
        return Ok(new { Message = response.Content });
    }
}
