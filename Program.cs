using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using Markdown_Note_taking_App.Data;
using Markdown_Note_taking_App.Models;
using Markdig;
using System.IdentityModel.Tokens.Jwt; //dotnet add package Markdig

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrEmpty(jwtKey)) throw new Exception("JWT Key is missing!");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true, // Checks if the token is expired
            ValidateIssuerSigningKey = true, // Proves YOU minted it
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapPost("/api/register", async (RegisterRequest request, AppDbContext db) =>
{
    var userExists = await db.Users.AnyAsync(u => u.Username == request.Username);
    if (userExists) return Results.BadRequest("Username is already taken.");
    string hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);
    var newUser = new User
    {
        Username = request.Username,
        PasswordHash = hashedPassword,
        Role = "User"
    };

    db.Users.Add(newUser);
    await db.SaveChangesAsync();
    return Results.Ok(new { message = "User registered successfully!" });
});

app.MapPost("/api/login", async (LoginRequest request, AppDbContext db, IConfiguration config) =>
{
    var user = await db.Users.SingleOrDefaultAsync(u => u.Username == request.Username);
    if (user == null)
    {
        return Results.Unauthorized();
    }
    bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
    if (!isPasswordValid)
    {
        return Results.Unauthorized();
    }
    var claims = new[]
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Name, user.Username),
        new Claim(ClaimTypes.Role, user.Role)
    };
    var keyString = config["Jwt:Key"];
    if (string.IsNullOrEmpty(keyString)) return Results.InternalServerError("JWT key is missing");
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    var token = new JwtSecurityToken(
        issuer: config["Jwt:Issuer"],
        audience: config["Jwt:Audience"],
        claims: claims,
        expires: DateTime.UtcNow.AddHours(2),
        signingCredentials: creds
    );
    var jwt = new JwtSecurityTokenHandler().WriteToken(token);
    return Results.Ok(new { Token = jwt });
});

app.MapGet("/api/notes/{id}/html", async (int id, AppDbContext db) =>
{
    var note = await db.Notes.FindAsync(id);
    if (note is null) return Results.NotFound();
    var htmlContent = Markdown.ToHtml(note.Content);
    return Results.Content(htmlContent, "text/html");
});

app.MapGet("/api/notes/my", async (AppDbContext db, ClaimsPrincipal user) =>
{
    var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
    if (userIdClaim == null) return Results.Unauthorized();
    int userId = int.Parse(userIdClaim.Value);
    var myNotes = await db.Notes.Where(n => n.AuthorId == userId).ToListAsync();
    return Results.Ok(myNotes);
}).RequireAuthorization();

app.MapGet("/api/notes", async (AppDbContext db) =>
{
    var notes = await db.Notes.ToListAsync();
    return Results.Ok(notes);
});

app.MapPost("/api/notes", async (Note newNote, AppDbContext db, ClaimsPrincipal user) =>
{
    var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
    if (userIdClaim == null) return Results.Unauthorized();
    newNote.AuthorId = int.Parse(userIdClaim.Value);
    newNote.CreatedAt = DateTime.UtcNow;
    db.Notes.Add(newNote);
    await db.SaveChangesAsync();
    return Results.Created($"/api/notes/{newNote.Id}", newNote);
}).RequireAuthorization();

app.MapPost("/api/grammar-check", async (GrammarRequest request) =>
{
    using var httpClient = new HttpClient();
    var content = new FormUrlEncodedContent(new[]
    {
        new KeyValuePair<string, string>("text", request.Text),
        new KeyValuePair<string, string>("language", "en-US")
    });
    var response = await httpClient.PostAsync("https://api.languagetool.org/v2/check", content);
    var jsonResult = await response.Content.ReadAsStringAsync();
    return Results.Content(jsonResult, "application/json");
});

app.MapPut("/api/notes/{id}", async (int id, Note updatedNote, AppDbContext db, ClaimsPrincipal user) =>
{
    var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
    if (userIdClaim == null) return Results.Unauthorized();
    int userId = int.Parse(userIdClaim.Value);
    var note = await db.Notes.FirstOrDefaultAsync(n => n.Id == id && n.AuthorId == userId);
    if (note == null) return Results.NotFound();
    note.Title = updatedNote.Title;
    note.Content = updatedNote.Content;
    await db.SaveChangesAsync();
    return Results.NoContent();
}).RequireAuthorization();

app.MapDelete("/api/notes/{id}", async (int id, AppDbContext db, ClaimsPrincipal user) =>
{
    var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
    if (userIdClaim == null) return Results.Unauthorized();
    int userId = int.Parse(userIdClaim.Value);
    var note = await db.Notes.FirstOrDefaultAsync(n => n.Id == id && n.AuthorId == userId);
    if (note == null) return Results.NotFound();
    db.Notes.Remove(note);
    await db.SaveChangesAsync();
    return Results.NoContent();
}).RequireAuthorization();
app.Run();

public class GrammarRequest
{
    public string Text { get; set; } = string.Empty;
}

public class RegisterRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
