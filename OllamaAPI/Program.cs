using Microsoft.OpenApi.Models;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using OllamaAPI.Entities;
using OllamaAPI.Services;
using OllamaSharp;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var ollamaSettings = builder.Configuration.GetSection("OllamaSettings")
                                          .Get<OllamaSettings>();

// Registra o serviço IChatCompletionService com a URI e o ModelName do Ollama
// O método AddOllamaChatCompletion é fornecido pelo pacote
// Microsoft.SemanticKernel.Connectors.Ollama.
// Ele cria e configura um IChatCompletionService que o Semantic Kernel vai
// usar para enviar prompts e receber respostas.
builder.Services.AddOllamaChatCompletion(ollamaSettings!.ModelName,
                                             new Uri(ollamaSettings.Uri));

// Registra OllamaService como implementação de IOllamaService no ciclo
// de vida scoped (uma instância por request HTTP).
// OllamaApiClient → é a classe do pacote OllamaSharp que encapsula toda
// a comunicação HTTP com o servidor Ollama.
builder.Services.AddScoped<IOllamaService, OllamaService>(provider =>
                           new OllamaService(new OllamaApiClient(
                           new Uri(ollamaSettings.Uri))));

// Registra um ChatHistory como um serviço transient
// ChatHistory → é uma classe do pacote Microsoft.SemanticKernel.
// Ela representa o histórico de uma conversa com um modelo de
// chat (ex.: perguntas e respostas anteriores).
builder.Services.AddTransient<ChatHistory>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "OllamaAPI",
        Version = "v1",
        Description = "API para integração com Ollama usando Semantic Kernel",
        Contact = new OpenApiContact
        {
            Name = "Macoratti",
            Email = "macoratti@yahoo"
        }
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "OllamaAPI v1");
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
