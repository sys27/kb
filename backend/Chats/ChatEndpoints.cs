using System.Text.Json;
using Backend.Chats.Requests;
using Backend.Chats.Responses;
using Backend.Ingestion;
using Backend.Llama;
using Backend.Messages;
using Backend.WebFetchHandlers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using JsonOptions = Microsoft.AspNetCore.Http.Json.JsonOptions;

namespace Backend.Chats;

public static partial class ChatEndpoints
{
    public static IEndpointRouteBuilder MapChatEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/chats");

        group.MapGet("", async (KbDbContext context, CancellationToken cancellationToken) =>
            {
                var chats = await context.Chats
                    .OrderBy(x => x.LastMessageAt != null)
                    .ThenByDescending(x => x.LastMessageAt)
                    .AsNoTracking()
                    .ToResponse()
                    .ToListAsync(cancellationToken);

                return chats;
            })
            .Produces<List<ChatListResponse>>()
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithName("GetChats")
            .WithSummary("Get all chats");

        group.MapGet("/{id:int}", async (int id, KbDbContext context, CancellationToken cancellationToken) =>
            {
                var chat = await context.Chats.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
                if (chat is null)
                    return Results.NotFound();

                return Results.Ok(chat.ToResponse());
            })
            .Produces<ChatListResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithName("GetChat")
            .WithSummary("Get a chat by id");

        group.MapPost("", async (CreateChatRequest request, KbDbContext context, CancellationToken cancellationToken) =>
            {
                var chat = request.ToEntity();
                await context.Chats.AddAsync(chat, cancellationToken);
                await context.SaveChangesAsync(cancellationToken);

                return Results.Created($"/chats/{chat.Id}", chat.ToResponse());
            })
            .Produces<ChatListResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithName("CreateChat")
            .WithSummary("Create a new chat");

        group.MapPut("/{id:int}", async (
                int id,
                UpdateChatRequest request,
                KbDbContext context,
                CancellationToken cancellationToken) =>
            {
                var chat = await context.Chats.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
                if (chat is null)
                    return Results.NotFound();

                chat.Name = request.Name;
                chat.UpdateProject(request.ProjectId);
                await context.SaveChangesAsync(cancellationToken);

                var response = chat.ToResponse();

                return Results.Ok(response);
            })
            .Produces<ChatListResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithName("UpdateChat")
            .WithSummary("Update a chat by id");

        group.MapDelete("/{id:int}", async (int id, KbDbContext context, CancellationToken cancellationToken) =>
            {
                var chat = await context.Chats.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
                if (chat is not null)
                {
                    context.Chats.Remove(chat);
                    await context.SaveChangesAsync(cancellationToken);
                }

                return Results.NoContent();
            })
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithName("DeleteChat")
            .WithSummary("Delete a chat by id");

        group.MapPost("/import-gpt", async (
                IFormFile? file,
                ILoggerFactory loggerFactory,
                ChatGptImporter importer,
                CancellationToken cancellationToken) =>
            {
                var logger = loggerFactory.CreateLogger("ChatImport");

                if (file is null)
                {
                    logger.LogWarning("No file was uploaded.");
                    return Results.BadRequest();
                }

                if (file.Length == 0)
                {
                    logger.LogWarning("'{FileFileName}' is empty.", file.FileName);
                    return Results.BadRequest();
                }

                if (file.ContentType != "application/json")
                {
                    logger.LogWarning("'{FileFileName}' is not a JSON file.", file.FileName);
                    return Results.BadRequest();
                }

                await using var stream = file.OpenReadStream();
                await importer.Import(stream, cancellationToken);

                return Results.Ok();
            })
            .DisableAntiforgery()
            .ProducesProblem(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithName("ImportChatsFromGpt")
            .WithSummary("Import chats from a file");

        group.MapPost("/{id:int}/generate-name", async (
                int id,
                KbDbContext context,
                LlamaCppClient chatClient,
                CancellationToken cancellationToken) =>
            {
                var chat = await context.Chats
                    .Include(x => x.Messages
                        .Where(m => m.MessageTypeId == MessageType.UserRequestId ||
                                    m.MessageTypeId == MessageType.AssistantAnswerId))
                    .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

                if (chat is null)
                    return Results.NotFound();

                if (chat.Messages.Count == 0)
                    return Results.Ok(new ChatNameResponse(chat.Name));

                // TODO: handle long conversations
                var conversation = new Conversation(
                    chat.Messages
                        .OrderBy(x => x.Id)
                        .Select(m => new ConversationMessage(
                            m.MessageTypeId == MessageType.UserRequestId ? "user" : "assistant",
                            m.Text))
                        .ToList());

                var json = JsonSerializer.Serialize(conversation, JsonSerializerOptions.Web);
                var prompt = $"""
                              Generate a short, descriptive name (1-5 words) for this conversation.
                              The name should capture the main topic or purpose of the discussion.
                              Output ONLY the name, nothing else.

                              Conversation:
                              {json}
                              """;

                var generatedName = await chatClient.GetResponse(
                    LlamaMessage.ForUser(prompt),
                    GetResponseOptions.NoThinking,
                    cancellationToken);

                if (string.IsNullOrWhiteSpace(generatedName))
                    generatedName = chat.Name;

                return Results.Ok(new ChatNameResponse(generatedName));
            })
            .Produces<ChatNameResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithName("GenerateChatName")
            .WithSummary("Generate a chat name from conversation history");

        group.MapPost("/{id:int}/follow-ups", async (
                int id,
                KbDbContext context,
                LlamaCppClient chatClient,
                CancellationToken cancellationToken) =>
            {
                var chat = await context.Chats
                    .Include(x => x.Messages
                        .Where(m => m.MessageTypeId == MessageType.UserRequestId ||
                                    m.MessageTypeId == MessageType.AssistantAnswerId))
                    .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

                if (chat is null)
                    return Results.NotFound();

                if (chat.Messages.Count == 0)
                    return Results.Ok(new FollowUpQuestionsResponse([]));

                var messages = chat.Messages
                    .OrderBy(x => x.Id)
                    .ToList();

                var lastUserMessageIndex = messages
                    .FindLastIndex(m => m.MessageTypeId == MessageType.UserRequestId);

                if (lastUserMessageIndex < 0)
                    return Results.Ok(new FollowUpQuestionsResponse([]));

                var conversation = new Conversation(
                    messages
                        .Skip(lastUserMessageIndex)
                        .Select(m => new ConversationMessage(
                            m.MessageTypeId == MessageType.UserRequestId ? "user" : "assistant",
                            m.Text))
                        .ToList());

                var json = JsonSerializer.Serialize(conversation, JsonSerializerOptions.Web);
                var prompt = $"""
                              Given this conversation turn, generate up to 3 short follow-up questions the user might naturally ask next.

                              Return the questions as a JSON array of strings. For example: ["What is X?", "How do I do Y?", "Can you show an example?"]

                              {json}
                              """;

                var response = await chatClient.GetResponse(
                    LlamaMessage.ForUser(prompt),
                    GetResponseOptions.NoThinking,
                    cancellationToken);

                var followUps = JsonSerializer.Deserialize<List<string>>(response)?.Take(3).ToList() ?? [];

                return Results.Ok(new FollowUpQuestionsResponse(followUps));
            })
            .Produces<FollowUpQuestionsResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithName("GenerateFollowUps")
            .WithSummary("Generate follow-up questions for a chat");

        group.MapPost("/{chatId:int}/sources/upload", async (
                int chatId,
                ILoggerFactory loggerFactory,
                IOptions<JsonOptions> jsonOptions,
                KbDbContext context,
                IngestionService ingestionService,
                IFormFile? file,
                CancellationToken cancellationToken) =>
            {
                var logger = loggerFactory.CreateLogger("ChatDocumentUpload");
                if (file is null || file.Length == 0)
                {
                    logger.LogWarning("File is empty");
                    return Results.BadRequest();
                }

                var chat = await context.Chats.FirstOrDefaultAsync(x => x.Id == chatId, cancellationToken);
                if (chat is null)
                {
                    logger.LogWarning("Chat not found: {chatId}", chatId);
                    return Results.NotFound();
                }

                var document = new Document
                {
                    Name = file.FileName,
                    Status = DocumentStatus.Pending,
                    ChatId = chat.Id,
                    Chat = chat,
                };
                chat.AddDocument(document);

                await using var stream = file.OpenReadStream();
                await ingestionService.UploadDocument(document, stream, cancellationToken);
                await ingestionService.Ingest(document, cancellationToken);

                var uploadMessage = Message.ForDocument(chat.Id, document.Name, jsonOptions.Value.SerializerOptions);
                chat.AddMessage(uploadMessage);
                await context.SaveChangesAsync(cancellationToken);

                return Results.Ok();
            })
            .DisableAntiforgery()
            .WithMetadata(new RequestSizeLimitAttribute(100L * 1024 * 1024 * 1024))
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithName("UploadSourceToChat")
            .WithSummary("Upload a source to a chat")
            .WithRequestTimeout(TimeSpan.FromMinutes(5));

        group.MapPost("/{chatId:int}/sources/add-web-site", async (
                int chatId,
                AddWebSiteRequest request,
                ILoggerFactory loggerFactory,
                IOptions<JsonOptions> jsonOptions,
                KbDbContext context,
                WebFetchHandlerFactory webFetchHandlerFactory,
                IngestionService ingestionService,
                CancellationToken cancellationToken) =>
            {
                var logger = loggerFactory.CreateLogger("ChatAddWebSite");
                var chat = await context.Chats.FirstOrDefaultAsync(x => x.Id == chatId, cancellationToken);
                if (chat is null)
                {
                    logger.LogWarning("Chat not found: {chatId}", chatId);
                    return Results.NotFound();
                }

                var fetcher = webFetchHandlerFactory.GetHandler(request.Url);
                var (fetchStream, fileName, _) = await fetcher.Fetch(request.Url, cancellationToken);

                var document = new Document
                {
                    Name = fileName,
                    Status = DocumentStatus.Pending,
                    ChatId = chat.Id,
                    Chat = chat,
                };
                chat.AddDocument(document);

                await ingestionService.UploadDocument(document, fetchStream, cancellationToken);
                await ingestionService.Ingest(document, cancellationToken);

                var uploadMessage = Message.ForWebSite(chat.Id, request.Url, jsonOptions.Value.SerializerOptions);
                chat.AddMessage(uploadMessage);
                await context.SaveChangesAsync(cancellationToken);

                return Results.Ok();
            })
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithName("AddWebSiteToChat")
            .WithSummary("Add a web site to a chat")
            .WithRequestTimeout(TimeSpan.FromMinutes(5));

        return app;
    }
}