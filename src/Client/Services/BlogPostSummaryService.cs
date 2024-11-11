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
}