using System.ComponentModel.DataAnnotations;

namespace Backend.Projects.Requests;

public class AddWebSiteRequest
{
    [Required]
    [MaxLength(2000)]
    public required string Url { get; set; }
}