using System.Text;
using Backend.Chats;

namespace Backend.Messages.Pipelines;

public class InsertSystemMessage : IConversationPipelineStep
{
    public Task ExecuteAsync(ConversationPipelineContext context, CancellationToken cancellationToken = default)
    {
        var chat = context.Get<Chat>("chat");
        var enableWebSearch = context.Get<bool>("enableWebSearch");

        if (chat.Messages.Count == 0)
        {
            var systemPrompt = new StringBuilder("You are a helpful assistant.\n\n");
            if (enableWebSearch)
            {
                // TODO: use chat instructions?
                systemPrompt.AppendLine(
                    """
                    You have access to two tools:

                    1. web_search(query)
                       - Use to find relevant web pages.
                       - Returns a list of results with title, url, and snippet.

                    2. web_fetch(urls)
                       - Use to retrieve and read full content of selected web pages.
                       - Accepts a small list of URLs (1–3).

                    Use web_search when:
                    - The question requires up-to-date information
                    - The answer is not in the provided context
                    - The topic is likely external or unknown

                    Do NOT use web_search if:
                    - The answer is already known or provided
                    - The question is simple or conversational

                    When calling web_search:
                    - Rewrite the user query into a concise, specific search query
                    - Prefer keywords over full sentences
                    - Include technical terms when relevant

                    After web_search:
                    - Review results carefully
                    - Select the most relevant and reliable URLs
                    - Avoid duplicates or low-quality sources
                    - Prefer authoritative or specific sources

                    Use web_fetch only after web_search:
                    - Fetch at most 3 URLs
                    - Only fetch pages that are clearly relevant
                    - Do not fetch all results

                    You may:
                    - Perform at most 2 web_search calls
                    - Perform at most 2 web_fetch calls

                    If results are insufficient:
                    - Refine the query and try again
                    - Otherwise, answer with best available information

                    After retrieving content:
                    - Extract the relevant information
                    - Combine information across sources if needed

                    When using web content:
                    - Prefer facts from retrieved pages
                    - Avoid hallucinating missing details

                    When possible, reference the source URLs in your answer.
                    """);
            }

            chat.AddMessage(Message.ForSystem(chat.Id, systemPrompt.ToString()));
        }

        return Task.CompletedTask;
    }
}