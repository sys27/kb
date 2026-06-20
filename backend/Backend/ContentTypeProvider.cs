using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.StaticFiles;

namespace Backend;

public class ContentTypeProvider : IContentTypeProvider
{
    private readonly FileExtensionContentTypeProvider provider;
    private readonly Dictionary<string, string> mappings;

    public ContentTypeProvider(FileExtensionContentTypeProvider provider)
    {
        this.provider = provider;
        this.mappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { ".epub", "application/epub+zip" }
        };
    }

    public bool TryGetContentType(string subpath, [MaybeNullWhen(false)] out string contentType)
    {
        var fileInfo = new FileInfo(subpath);

        if (!mappings.TryGetValue(fileInfo.Extension, out contentType))
            return provider.TryGetContentType(subpath, out contentType);

        return true;
    }
}