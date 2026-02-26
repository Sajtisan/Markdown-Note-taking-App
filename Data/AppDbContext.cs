using Microsoft.EntityFrameworkCore;
using Markdown_Note_taking_App.Models;

namespace Markdown_Note_taking_App.Data;

public class AppDbContext : DbContext
{
    // The constructor passes database configuration options to the base DbContext class
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // This is the magic line. It tells Entity Framework to create a table 
    // in PostgreSQL called "Notes" based on your C# Note class.
    public DbSet<Note> Notes { get; set; }
}