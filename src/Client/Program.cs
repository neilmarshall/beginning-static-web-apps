using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Client;
using Client.Services;
using StaticWebAppAuthentication.Client;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<BlogPostSummaryService>();
builder.Services.AddScoped<BlogPostService>();
builder.Services.AddStaticWebsAuthentication();

builder.Services.AddBlazorBootstrap();

await builder.Build().RunAsync();
