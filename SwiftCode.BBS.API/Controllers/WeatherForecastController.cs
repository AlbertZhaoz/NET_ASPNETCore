using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SwiftCode.BBS.API.Controllers
{
    /// <summary>
    /// Weather forecast
    /// </summary>
    [ApiController]
    [Route("[controller]/[action]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

        private readonly ILogger<WeatherForecastController> _logger;

        /// <summary>
        /// WeatherForecastController
        /// </summary>
        /// <param name="logger"></param>
        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Get Random Forecast
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Policy = "SystemOrAdmin")]
        public IEnumerable<WeatherForecast> Get()
        {
            
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }

        /// <summary>
        /// GetAdminName
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        //[ApiExplorerSettings(IgnoreApi =true)]
        public ActionResult<String> GetAdminName()
        {
            return "Admin";
        }

        /// <summary>
        /// GetSystemName
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = "System")]
        //[ApiExplorerSettings(IgnoreApi =true)]
        public ActionResult<String> GetSystemName()
        {
            return "System";
        }

        /// <summary>
        /// GetSystemAndAdminName
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Policy = "SystemAndAdmin")]
        //[ApiExplorerSettings(IgnoreApi =true)]
        public ActionResult<String> GetSystemAndAdminName()
        {
            return "SystemAndAdmin";
        }


    }
}