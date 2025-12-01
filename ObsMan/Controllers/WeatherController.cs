using Microsoft.AspNetCore.Mvc;
namespace ObsMan.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WeatherController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetForecast()
        {
            var forecast = new[]
            {
                new { Date = DateTime.Now, TemperatureC = 12, Summary = "Cloudy" },
                new { Date = DateTime.Now.AddDays(1), TemperatureC = 15, Summary = "Sunny" }
            };

            return Ok(forecast);
        }
    }
}
