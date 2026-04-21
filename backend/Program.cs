using Backend;
using Backend.Chats;
using Backend.Messages;
using Backend.Messages.Pipelines;
using Backend.Projects;
using Backend.Vectors;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.VectorData;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IValidateOptions<LlmOptions>, LlmOptions>();
builder.Services.Configure<LlmOptions>(builder.Configuration.GetSection(LlmOptions.Section));

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHealthChecks();

builder.Services.AddProblemDetails();
builder.Services.AddValidation();

builder.Services.AddHttpContextAccessor();

builder.Services.AddDatabase(builder.Configuration, builder.Environment);
builder.Services.AddAiClient();
builder.Services.AddIngestion(builder.Configuration);
builder.Services.AddConversationPipeline();

builder.Services.AddSpaStaticFiles(options => options.RootPath = "wwwroot");

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

app.MapHealthChecks("/health");
app.MapOpenApi();
app.MapScalarApiReference();

app.MapGroup("/api")
    .MapChatEndpoints()
    .MapMessageEndpoints()
    .MapProjectEndpoints();

if (!app.Environment.IsDevelopment())
{
    app.UseSpaStaticFiles();
    app.UseSpa(_ => { });
}

// TODO: execute as a part of migrations?
var vs = app.Services.GetRequiredService<VectorStoreCollection<int, Embeddings>>();
await vs.EnsureCollectionExistsAsync();

app.Run();