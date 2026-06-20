using Backend.Messages.Responses;
using Riok.Mapperly.Abstractions;

namespace Backend.Messages;

[Mapper(
    EnumMappingIgnoreCase = true,
    EnumMappingStrategy = EnumMappingStrategy.ByValueCheckDefined,
    RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public static partial class MessageMapper
{
    public static partial MessageListResponse ToResponse(this Message chat);

    public static partial IQueryable<MessageListResponse> ToResponse(this IQueryable<Message> chats);
}