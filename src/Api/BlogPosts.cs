using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Models;
using StaticWebAppAuthentication.Api;
using static System.Security.Cryptography.SHA256;
using static System.Text.Encoding;
using FromBody = Microsoft.Azure.Functions.Worker.Http.FromBodyAttribute;

namespace Api;

public class BlogPosts()
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

    [Function($"{nameof(BlogPosts)}_Post")]
    public static async Task<IActionResult> PostBlogPost(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "blogposts")] HttpRequest req, [@FromBody] BlogPost blogPost,
        [CosmosDBInput("SwaBlog", "BlogContainer", Connection = "CosmosDbConnectionString")] Container container,
        ILogger log)
    {
        if (blogPost.id != default)
            throw new ArgumentException("id must be null");

        try
        {
            var clientPrincipal = StaticWebAppApiAuthorization.ParseHttpHeaderForClientPrincipal(req.Headers);

            BlogPostCreatable blogPostToCreate = new()
            {
                id = Guid.NewGuid(),
                Author = System.Convert.ToHexString(HashData(UTF8.GetBytes(clientPrincipal.UserDetails))),
                Title = blogPost.Title,
                PublishedDate = blogPost.PublishedDate,
                Tags = blogPost.Tags,
                BlogpostMarkdown = blogPost.BlogpostMarkdown,
                Status = 2
            };

            await container.CreateItemAsync(blogPostToCreate);

            return new OkObjectResult(blogPostToCreate);
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Failed to create entry in CosmosDB");
            throw;
        }
    }

    [Function($"{nameof(BlogPosts)}_Put")]
    [CosmosDBOutput("SwaBlog", "BlogContainer", Connection = "CosmosDbConnectionString")]
    public static BlogPost PutBlogPost(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "blogposts")] HttpRequest req, [@FromBody] BlogPost updatedBlogPost,
        [CosmosDBInput(
            "SwaBlog",
            "BlogContainer",
            Connection = "CosmosDbConnectionString",
            PartitionKey = "{Author}",
            Id = "{Id}")] BlogPost blogPost,
        ILogger log)
    {
        try
        {
            throw new NotImplementedException();
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Failed to update entry in CosmosDB");
            throw;
        }
    }

    [Function($"{nameof(BlogPosts)}_Delete")]
    public static async Task<IActionResult> DeleteBlogPost(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "blogposts/{author}/{id}")] HttpRequest req, string author, string id,
        [CosmosDBInput("SwaBlog", "BlogContainer", Connection = "CosmosDbConnectionString")] Container container,
        ILogger log)
    {
        try
        {
            await container.DeleteItemAsync<BlogPost>(id, partitionKey: new PartitionKey(author));

            return new NoContentResult();
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Failed to delete entry in CosmosDB");
            throw;
        }
    }
}