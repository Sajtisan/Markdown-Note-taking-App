using System;

namespace MarkdownNotesClient.Data;

public class LocalNote
{
    public int Id { get; set; }
    public int? RemoteId { get; set; }
    
    public string Title { get; set; } = "";
    public string Content { get; set; } = "";
    public DateTime LastModified { get; set; }
    public bool IsSynced { get; set; }
    public bool IsDeleted { get; set; }
}