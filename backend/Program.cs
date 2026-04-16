using Backend;
using Backend.Chats;
using Backend.Messages;
using Backend.Projects;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IValidateOptions<LlmOptions>, LlmOptions>();
builder.Services.Configure<LlmOptions>(builder.Configuration.GetSection(LlmOptions.Section));

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();

builder.Services.AddProblemDetails();
builder.Services.AddValidation();

builder.Services.AddHttpContextAccessor();

builder.Services.AddDatabase(builder.Configuration, builder.Environment);
builder.Services.AddAiClient();

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

app.Run();