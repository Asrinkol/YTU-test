using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YTU_test.Data;
using YTU_test.Models;
using YTU_test.Models.Requests;
using System.Linq;
using System.Reflection;

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

        
        [HttpGet("GetForecasts")]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> GetForecasts([FromQuery] WeatherForecastQueryParameters queryParameters)
        {
            _logger.LogInformation("GetForecasts isteði alýndý. Query: {@Query}", queryParameters);

            IQueryable<WeatherForecast> forecasts = _context.WeatherForecasts;

            
            if (queryParameters.MinDate.HasValue)
            {
                forecasts = forecasts.Where(f => f.ForecastDate >= queryParameters.MinDate.Value);
            }
            if (queryParameters.MaxDate.HasValue)
            {
                forecasts = forecasts.Where(f => f.ForecastDate <= queryParameters.MaxDate.Value);
            }
            if (queryParameters.MinTemperatureC.HasValue)
            {
                forecasts = forecasts.Where(f => f.TemperatureC >= queryParameters.MinTemperatureC.Value);
            }
            if (queryParameters.MaxTemperatureC.HasValue)
            {
                forecasts = forecasts.Where(f => f.TemperatureC <= queryParameters.MaxTemperatureC.Value);
            }
            if (!string.IsNullOrWhiteSpace(queryParameters.SummaryContains))
            {
                forecasts = forecasts.Where(f => f.Summary != null && f.Summary.ToLowerInvariant().Contains(queryParameters.SummaryContains.ToLowerInvariant()));
            }

            
            var totalCount = await forecasts.CountAsync();

            
            if (!string.IsNullOrWhiteSpace(queryParameters.SortBy))
            {
                var propertyInfo = typeof(WeatherForecast).GetProperty(queryParameters.SortBy, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                if (propertyInfo == null)
                {
                    _logger.LogWarning("Geçersiz sýralama sütunu: {SortBy}", queryParameters.SortBy);
                    return BadRequest($"Sýralama için geçersiz sütun adý: {queryParameters.SortBy}");
                }

                if (queryParameters.SortOrder?.ToLower() == "desc")
                {
                    forecasts = forecasts.OrderByDescending(f => propertyInfo.GetValue(f));
                }
                else
                {
                    forecasts = forecasts.OrderBy(f => propertyInfo.GetValue(f));
                }
            }
            else
            {
                
                forecasts = forecasts.OrderBy(f => f.Id);
            }

            
            var pagedForecasts = await forecasts
                .Skip((queryParameters.PageNumber - 1) * queryParameters.PageSize)
                .Take(queryParameters.PageSize)
                .ToListAsync();

            
            return Ok(new
            {
                TotalCount = totalCount,
                PageNumber = queryParameters.PageNumber,
                PageSize = queryParameters.PageSize,
                Data = pagedForecasts
            });
        }

        
        /*
        [HttpGet(Name = "GetWeatherForecast")]
        [Authorize(Roles = "Admin,User")]
        public async Task<IEnumerable<WeatherForecast>> Get()
        {
            return await _context.WeatherForecasts.ToListAsync();
        }
        */

        [HttpPost("generate")]
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
        [Authorize(Roles = "Admin,User")]
        public IActionResult ErrorThrow()
        {
            throw new Exception("This is a test exception to demonstrate error handling.");
        }
    }
}