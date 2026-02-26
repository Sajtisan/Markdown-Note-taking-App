using Microsoft.EntityFrameworkCore;
using Markdown_Note_taking_App.Data;
using Markdown_Note_taking_App.Models;
using Markdig; //dotnet add package Markdig

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/api/notes/{id}/html", async (int id, AppDbContext db) =>
{
    var note = await db.Notes.FindAsync(id);
    if (note is null) return Results.NotFound();
    var htmlContent = Markdown.ToHtml(note.Content);
    return Results.Content(htmlContent, "text/html");
});

app.MapGet("/api/notes", async (AppDbContext db) =>
{
    var notes = await db.Notes.ToListAsync();
    return Results.Ok(notes);
});

app.MapPost("/api/notes", async (Note incomingNote, AppDbContext db) =>
{
    incomingNote.CreatedAt = DateTime.UtcNow;
    db.Notes.Add(incomingNote);
    await db.SaveChangesAsync();
    return Results.Created($"/api/notes/{incomingNote.Id}", incomingNote);
});

app.MapPut("/api/notes/{id}", async (int id, Note updatedNote, AppDbContext db) =>
{
    var existingNote = await db.Notes.FindAsync(id);
    if (existingNote is null) return Results.NotFound();
    existingNote.Title = updatedNote.Title;
    existingNote.Content = updatedNote.Content;
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.MapDelete("/api/notes/{id}", async (int id, AppDbContext db) =>
{
    var note = await db.Notes.FindAsync(id);
    if (note is null) return Results.NotFound();
    db.Notes.Remove(note);
    await db.SaveChangesAsync();
    return Results.Ok();
});

app.Run();