using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models;

namespace Api;

public class BlogPosts(ILogger<BlogPosts> logger)
{
    // public static object UriFactory { get; private set; }

    [Function($"{nameof(BlogPosts)}_Get")]
    public IActionResult GetAllBlogPosts(
        [HttpTrigger( AuthorizationLevel.Anonymous, "get", Route = "blogposts")] HttpRequest req,
        [CosmosDBInput(
            "SwaBlog",
            "BlogContainer",
            Connection = "CosmosDbConnectionString",
            SqlQuery = @"
                SELECT c.id
                    ,c.Title
                    ,c.Author
                    ,c.PublishedDate
                    ,LEFT(c.BlogpostMarkdown, 500) AS BlogPostMarkdown
                    ,LENGTH(c.BlogpostMarkdown) <= 500 AS PreviewIsComplete
                    ,c.Tags
                FROM c
                WHERE c.Status = 2")] IEnumerable<BlogPost> blogPosts)
    {
        return new OkObjectResult(blogPosts);
    }

    [Function($"{nameof(BlogPosts)}_GetId")]
    public IActionResult GetBlogPost(
        [HttpTrigger( AuthorizationLevel.Anonymous, "get", Route = "blogposts/{author}/{id}")] HttpRequest req,
        [CosmosDBInput(
            "SwaBlog",
            "BlogContainer",
            Connection = "CosmosDbConnectionString",
            PartitionKey = "{author}",
            Id = "{id}")] BlogPost blogPost)
    {
        if (blogPost is null)
            return new NotFoundResult();

        return new OkObjectResult(blogPost);
    }
}
