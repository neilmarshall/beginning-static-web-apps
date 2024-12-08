using System.Net.Http.Json;
using Models;

namespace Client.Services;

public class BlogPostSummaryService
{
    private readonly HttpClient http;

    public List<BlogPost>? Summaries { get; private set; }

    public BlogPostSummaryService(HttpClient http)
    {
        ArgumentNullException.ThrowIfNull(http, nameof(http));

        this.http = http;
    }

    public async Task LoadBlogPostSummaries() =>
        Summaries = await http.GetFromJsonAsync<List<BlogPost>>("api/blogposts");

    public void Add(BlogPost blogPost)
    {
        ArgumentNullException.ThrowIfNull(blogPost, nameof(blogPost));

        if (Summaries is null)
            return;
        
        if (!Summaries.Any(summary => summary.id == blogPost.id && summary.Author == blogPost.Author))
        {
            var summary = new BlogPost
            {
                id = blogPost.id,
                Author = blogPost.Author,
                BlogpostMarkdown = blogPost.BlogpostMarkdown,
                PublishedDate = blogPost.PublishedDate,
                Tags = blogPost.Tags,
                Title = blogPost.Title
            };

            if (summary.BlogpostMarkdown?.Length > 500)
            {
                summary.BlogpostMarkdown = summary.BlogpostMarkdown[..500];
            }

            Summaries.Add(summary);
        }
    }

    public void Replace(BlogPost blogPost)
    {
        ArgumentNullException.ThrowIfNull(blogPost, nameof(blogPost));

        if (Summaries is null || !Summaries.Any(bp => bp.id == blogPost.id && bp.Author == blogPost.Author))
            return;
        
        var summary = Summaries.Find(summary => summary.id == blogPost.id && summary.Author == blogPost.Author);

        if (summary is not null)
        {
            summary.Title = blogPost.Title;
            summary.Author = blogPost.Author;
            summary.BlogpostMarkdown = blogPost.BlogpostMarkdown;

            if (summary.BlogpostMarkdown?.Length > 500)
            {
                summary.BlogpostMarkdown = summary.BlogpostMarkdown[..500];
            }

            Summaries.Add(summary);
        }
    }

    public void Remove(Guid id, string author)
    {
        if (Summaries is null || !Summaries.Any(bp => bp.id == id && bp.Author == author))
            return;
        
        var summary = Summaries.First(summary => summary.id == id && summary.Author == author);

        Summaries.Remove(summary);
    }
}