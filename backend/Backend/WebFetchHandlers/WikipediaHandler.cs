namespace Backend.WebFetchHandlers;

// TODO: implement a better parsing
public class WikipediaHandler : DefaultHandler
{
    public WikipediaHandler(HttpClient client)
        : base(client)
    {
    }

    public override ISet<string> SupportedDomains => new HashSet<string>
    {
        "wikipedia.org"
    };
}