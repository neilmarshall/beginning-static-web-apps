using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Client.Services;
using Microsoft.AspNetCore.Components;
using Models;

public class BlogPostService
{
    private readonly HttpClient http;
    private readonly NavigationManager navigationManager;
    private List<BlogPost> blogPostCache = [];
    private readonly BlogPostSummaryService blogPostSummaryService;

    public BlogPostService(
        HttpClient http,
        NavigationManager navigationManager,
        BlogPostSummaryService blogPostSummaryService)
    {
        ArgumentNullException.ThrowIfNull(http, nameof(http));
        ArgumentNullException.ThrowIfNull(navigationManager, nameof(navigationManager));
        ArgumentNullException.ThrowIfNull(blogPostSummaryService, nameof(blogPostSummaryService));

        this.http = http;
        this.navigationManager = navigationManager;
        this.blogPostSummaryService = blogPostSummaryService;
    }

    public async Task<BlogPost?> GetBlogPost(Guid blogPostId, string author)
    {
        BlogPost? blogPost = blogPostCache.FirstOrDefault(bp => bp.id == blogPostId && bp.Author == author);

        if (blogPost is null)
        {
            var result = await http.GetAsync($"api/blogposts/{author}/{blogPostId}");

            if (!result.IsSuccessStatusCode)
            {
                navigationManager.NavigateTo("404");
                return null;
            }

            blogPost = await result.Content.ReadFromJsonAsync<BlogPost>();

            if (blogPost is null)
            {
                navigationManager.NavigateTo("404");
                return null;
            }

            blogPostCache.Add(blogPost);
        }

        return blogPost;
    }

    public async Task<BlogPost> Create(BlogPost blogPost)
    {
        ArgumentNullException.ThrowIfNull(blogPost, nameof(blogPost));
        var content = JsonSerializer.Serialize(blogPost);
        var data = new StringContent(content, encoding: Encoding.UTF8, mediaType: "application/json");
        var result = await http.PostAsync("api/blogposts", data);
        result.EnsureSuccessStatusCode();
        Console.WriteLine(result.Content.ToString());
        BlogPost? savedBlogPost = await result.Content.ReadFromJsonAsync<BlogPost>();
        blogPostCache.Add(savedBlogPost!);
        blogPostSummaryService.Add(savedBlogPost!);
        return savedBlogPost!;
    }

    public async Task Update(BlogPost blogPost)
    {
        ArgumentNullException.ThrowIfNull(blogPost, nameof(blogPost));
        var content = JsonSerializer.Serialize(blogPost);
        var data = new StringContent(content, encoding: Encoding.UTF8, mediaType: "application/json");
        var result = await http.PutAsync("api/blogposts", data);
        result.EnsureSuccessStatusCode();
        int index = blogPostCache.FindIndex(bp => bp.id == blogPost.id && bp.Author == blogPost.Author);
        if (index >= 0)
        {
            blogPostCache[index] = blogPost;
        }
        blogPostSummaryService.Replace(blogPost);
    }

    public async Task Delete(Guid id, string author)
    {
        var result = await http.DeleteAsync($"api/blogposts/{author}/{id}");
        result.EnsureSuccessStatusCode();
        var blogPost = blogPostCache.FirstOrDefault(bp => bp.id == id && bp.Author == author);
        if (blogPost is not null)
        {
            blogPostCache.Remove(blogPost);
        }
        blogPostSummaryService.Remove(id, author);
    }
}