using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YTU_test.Data;
using YTU_test.Models;
using Microsoft.AspNetCore.Authorization; 

namespace YTU_test.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(AppDbContext context, ILogger<WeatherForecastController> logger)
        {
            _context = context;
            _logger = logger;
        }

        
        [HttpGet(Name = "GetWeatherForecast")]
        //[AllowAnonymous]
        [Authorize(Roles = "Admin,User")] 
        public async Task<IEnumerable<WeatherForecast>> Get()
        {
            return await _context.WeatherForecasts.ToListAsync();
        }

        
        [HttpPost("generate")]
        //[AllowAnonymous]
        [Authorize(Roles = "Admin")] 
        public async Task<IActionResult> GenerateRandomForecasts()
        {
            var summaries = new[]
            {
                "Freezing", "Bracing", "Chilly", "Cool", "Mild",
                "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
            };

            var forecasts = Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                ForecastDate = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = summaries[Random.Shared.Next(summaries.Length)]
            }).ToList();

            _context.WeatherForecasts.AddRange(forecasts);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Random forecasts added.",
                count = forecasts.Count
            });
        }

        
        [HttpDelete("{id}")]
        //[AllowAnonymous]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteForecast(int id)
        {
            var forecast = await _context.WeatherForecasts.FindAsync(id);
            if (forecast == null)
            {
                return NotFound(new { success = false, message = $"Forecast with ID {id} not found." });
            }

            _context.WeatherForecasts.Remove(forecast);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = $"Forecast with ID {id} deleted." });
        }

        
        [HttpGet("ErrorThrow")]
        //[AllowAnonymous]
        [Authorize(Roles = "Admin,User")] 
        public IActionResult ErrorThrow()
        {
            throw new Exception("This is a test exception to demonstrate error handling.");
        }
    }
}