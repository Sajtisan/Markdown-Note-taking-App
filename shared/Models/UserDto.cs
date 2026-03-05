namespace shared.Models;

public class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = "User";
    public List<Note> Notes { get; set; } = new();
}