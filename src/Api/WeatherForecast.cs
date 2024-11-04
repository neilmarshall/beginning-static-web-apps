using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api
{
    public class WeatherForecast
    {
        private readonly ILogger<WeatherForecast> _logger;

        public WeatherForecast(ILogger<WeatherForecast> logger)
        {
            _logger = logger;
        }

        [Function("WeatherForecast")]
        public IActionResult Run([HttpTrigger(
            AuthorizationLevel.Function,
            "get",
            Route = "weather-forecast/{daysToForecast=5}")]
            HttpRequest req,
            int daysToForecast)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            return new OkObjectResult(GetWeather(daysToForecast));
        }
        
        private static dynamic[] GetWeather(int daysToForecast)
        {
            var enumerator = Enumerable.Range(1, daysToForecast);
            var result = new List<dynamic>();
            var rnd = new Random();

            foreach (var day in enumerator)
            {
                var temp = rnd.Next(25);
                var summary = GetSummary(temp);
                result.Add(new
                {
                    Date = DateTime.Now.AddDays(day),
                    Summary = summary,
                    TemperatureC = temp
                });
            }

            return result.ToArray();
        }

        private static object GetSummary(int temp)
        {
            return temp switch
            {
                int i when (i > 20) => "Hot!",
                int i when (i > 15) => "Warm",
                int i when (i > 10) => "Cool",
                int i when (i > 5) => "Cold",
                _ => "Too cold!"
            };
        }
    }
}
