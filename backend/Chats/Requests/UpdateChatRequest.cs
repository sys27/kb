using System.ComponentModel.DataAnnotations;

namespace Backend.Chats.Requests;

public class UpdateChatRequest
{
    [Required]
    [MaxLength(256)]
    public required string Title { get; set; }
}