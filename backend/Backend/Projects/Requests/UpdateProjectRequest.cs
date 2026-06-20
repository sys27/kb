using System.ComponentModel.DataAnnotations;

namespace Backend.Projects.Requests;

public record UpdateProjectRequest([Required] [MaxLength(256)] string Name);