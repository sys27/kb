using Backend.Chats;
using Backend.Messages;
using Backend.Projects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Backend;

public class KbDbContext : DbContext
{
    public KbDbContext(DbContextOptions<KbDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var properties = entityType.ClrType
                .GetProperties()
                .Where(p => p.PropertyType == typeof(DateTimeOffset) ||
                            p.PropertyType == typeof(DateTimeOffset?));

            foreach (var property in properties)
            {
                modelBuilder
                    .Entity(entityType.Name)
                    .Property(property.Name)
                    .HasConversion(new DateTimeOffsetToBinaryConverter());
            }
        }
    }

    public DbSet<Chat> Chats { get; set; }

    public DbSet<Message> Messages { get; set; }

    public DbSet<Project> Projects { get; set; }

    public DbSet<Document> Documents { get; set; }
}