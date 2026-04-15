using Backend.Chats.Requests;
using Backend.Chats.Responses;
using Riok.Mapperly.Abstractions;

namespace Backend.Chats;

[Mapper(
    EnumMappingIgnoreCase = true,
    EnumMappingStrategy = EnumMappingStrategy.ByValue,
    RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public static partial class Mapper
{
    public static partial ChatListResponse ToResponse(this Chat chat);

    public static partial IQueryable<ChatListResponse> ToResponse(this IQueryable<Chat> chats);

    [MapperIgnoreTarget(nameof(Chat.Id))]
    [MapperIgnoreTarget(nameof(Chat.Messages))]
    public static partial Chat ToEntity(this CreateChatRequest request);
}