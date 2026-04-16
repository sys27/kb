using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.Extensions.VectorData;

namespace Backend.Projects;

public class Document
{
    [VectorStoreKey]
    public int Id { get; init; }

    [VectorStoreData]
    public required string Name { get; set; }

    [VectorStoreVector(768)]
    [NotMapped]
    public required string Content { get; set; }

    [VectorStoreData]
    public int ProjectId { get; init; }
}