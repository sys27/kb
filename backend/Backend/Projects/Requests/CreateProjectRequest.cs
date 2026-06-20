using System.ComponentModel.DataAnnotations;

namespace Backend.Projects.Requests;

public record CreateProjectRequest([Required] [MaxLength(256)] string Name);