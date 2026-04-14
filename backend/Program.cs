using Backend;
using Backend.Chats;
using Backend.Messages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();

builder.Services.AddProblemDetails();
builder.Services.AddValidation();

builder.Services.AddHttpContextAccessor();

builder.Services
    .AddDbContext<KbDbContext>((provider, options) =>
    {
        var env = provider.GetRequiredService<IHostEnvironment>();

        options
            .UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"))
            .EnableDetailedErrors(env.IsDevelopment())
            .EnableSensitiveDataLogging(env.IsDevelopment())
            .ConfigureWarnings(w =>
            {
#if DEBUG
                w.Throw(RelationalEventId.MultipleCollectionIncludeWarning);
#endif
            });
    });

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

app.MapHealthChecks("/health");
app.MapOpenApi();
app.MapScalarApiReference();

app.MapGroup("/api")
    .MapChatEndpoints()
    .MapMessageEndpoints();

app.Run();