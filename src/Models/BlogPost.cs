namespace Models;

public class BlogPost
{
    public Guid? id { get; set; }
    public required string Title { get; set; }
    public required string Author { get; set; }
    public DateTime PublishedDate { get; set; }
    public string[] Tags { get; set; } = [];
    public required string BlogpostMarkdown { get; set; }
    public bool PreviewIsComplete { get; set; }
}
