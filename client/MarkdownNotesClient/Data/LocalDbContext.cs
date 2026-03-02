using Microsoft.EntityFrameworkCore;

namespace MarkdownNotesClient.Data;

public class LocalDbContext : DbContext
{
    public DbSet<LocalNote> Notes { get; set; }
    public DbSet<LocalSession> Sessions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=localnotes.db");
    }
}