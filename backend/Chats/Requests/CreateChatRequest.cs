using System.ComponentModel.DataAnnotations;

namespace Backend.Chats.Requests;

public class CreateChatRequest
{
    [Required]
    [MaxLength(256)]
    public required string Title { get; set; }
}