using System.ComponentModel;
using Anthropic;
using Anthropic.Core;
using Microsoft.Agents.AI.DevUI;
using Microsoft.Agents.AI.Hosting;
using Microsoft.Extensions.AI;

var builder = WebApplication.CreateBuilder(args);

var apiKey = builder.Configuration["Anthropic:ApiKey"]
    ?? throw new InvalidOperationException(
        "No API key. Set it by: dotnet user-secrets set \"Anthropic:ApiKey\" \"<your-key>\"");

var model = builder.Configuration["Anthropic:Model"] ?? "claude-haiku-4-5-20251001";

IChatClient chatClient = new AnthropicClient(new ClientOptions { ApiKey = apiKey })
    .AsIChatClient(model);

builder.Services.AddChatClient(chatClient);

const string systemPrompt = """
    You are a novice mage, a young apprentice assistant. You are helpful, polite,
    and a little nervous. Always answer using simple, plain English. Use short
    sentences and easy words. Avoid fancy or complex language.
    """;

[Description("Cast Fireball at a target. The novice has no control over the outcome.")]
static string Fireball([Description("What to aim the fireball at")] string target)
{
    string[] outcomes =
    [
        $"You aim at {target}. You miss. The drapes are on fire now.",
        $"You aim at {target}. You miss. The bread on the windowsill is toasted now.",
        $"You aim at {target}. It was the Archmage's coat. It is on fire now.",
        $"You aim at {target}. You panic and hit your own reflection instead. Your eyebrows are not fine.",
        $"You aim at {target}. You hit it. Everyone is shocked, including you.",
    ];

    return outcomes[Random.Shared.Next(outcomes.Length)];
}

builder.AddAIAgent("NoviceMage", systemPrompt)
    .WithAITool(AIFunctionFactory.Create(Fireball, name: "fireball"));

builder.Services.AddOpenAIResponses();
builder.Services.AddOpenAIConversations();
builder.Services.AddDevUI();

var app = builder.Build();

app.MapOpenAIResponses();
app.MapOpenAIConversations();

if (app.Environment.IsDevelopment())
{
    app.MapDevUI();
}

app.Run();
