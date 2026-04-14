using Backend.Chats;
using Backend.Messages;
using Microsoft.EntityFrameworkCore;

namespace Backend;

public class KbDbContext : DbContext
{
    public KbDbContext(DbContextOptions<KbDbContext> options)
        : base(options)
    {
    }

    public DbSet<Chat> Chats { get; set; }

    public DbSet<Message> Messages { get; set; }
}