using System.Text.Json.Serialization;
using Backend;
using Backend.Chats;
using Backend.Export;
using Backend.Messages;
using Backend.Messages.Pipelines;
using Backend.Projects;
using Microsoft.AspNetCore.Http.Json;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<JsonOptions>(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHealthChecks();

builder.Services.AddProblemDetails();
builder.Services.AddValidation();

builder.Services.AddHttpContextAccessor();

builder.Services.AddDatabase(builder.Configuration, builder.Environment);
builder.Services.AddAi(builder.Configuration);
builder.Services.AddIngestion(builder.Configuration);
builder.Services.AddConversationPipeline(builder.Configuration);
builder.Services.AddKnowledge();

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
    .MapProjectEndpoints()
    .MapExportEndpoints();

if (!app.Environment.IsDevelopment())
{
    app.UseSpaStaticFiles();
    app.UseSpa(_ => { });
}

await app.InitializeDatabase();

app.Run();