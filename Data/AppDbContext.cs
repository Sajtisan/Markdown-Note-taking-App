using Microsoft.EntityFrameworkCore;
using Markdown_Note_taking_App.Models;

namespace Markdown_Note_taking_App.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<User> Users { get; set; }
    public DbSet<Note> Notes { get; set; }
}