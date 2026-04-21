using Microsoft.EntityFrameworkCore;

namespace Backend.Messages.Pipelines;

public class GetChat : IConversationPipelineStep
{
    private readonly ILogger<GetChat> logger;
    private readonly KbDbContext dbContext;
    private readonly HttpResponse httpResponse;

    public GetChat(
        ILogger<GetChat> logger,
        KbDbContext dbContext,
        IHttpContextAccessor httpContextAccessor)
    {
        if (httpContextAccessor.HttpContext is null)
            throw new ArgumentNullException(nameof(httpContextAccessor));

        this.logger = logger;
        this.dbContext = dbContext;
        this.httpResponse = httpContextAccessor.HttpContext.Response;
    }

    public async Task ExecuteAsync(ConversationPipelineContext context, CancellationToken cancellationToken = default)
    {
        var chatId = context.Get<int>("chatId");
        var chat = await dbContext.Chats
            .Include(x => x.Messages)
            .FirstOrDefaultAsync(c => c.Id == chatId, cancellationToken);

        if (chat is null)
        {
            logger.LogWarning("Chat not found: {chatId}", chatId);

            httpResponse.StatusCode = 404;
            return;
        }

        context.Set("chat", chat);
    }
}