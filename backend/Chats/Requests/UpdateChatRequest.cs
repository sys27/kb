using System.ComponentModel.DataAnnotations;

namespace Backend.Chats.Requests;

public class UpdateChatRequest
{
    [Required]
    [MaxLength(256)]
    public required string Name { get; set; }

    public int? ProjectId { get; set; }
}