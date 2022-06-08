using EventListenerAPI.Models;
using EventListenerAPI.Services;

using Microsoft.AspNetCore.Mvc;

namespace EventListenerAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly EventService _eventService;

        private readonly ILogger<TestController> _logger;

        private readonly IActorBridge _bridge;

        public TestController(ILogger<TestController> logger, EventService eventService, IActorBridge bridge)
        {
            _logger = logger;
            _bridge = bridge;
            _eventService = eventService;
        }

        [HttpGet(Name = "GetWeatherForecast")]
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

        [HttpPost(nameof(AddBulkData))]  
        public async Task<IActionResult> AddBulkData(int repeat)  
        {  
            var response = await _eventService.AddBulkDataAsync(repeat);  
            return Ok(response);  
        }
        
        [HttpPost(nameof(AddEventBulkData))]  
        public async Task<IActionResult> AddEventBulkData(int repeat)  
        {              
            for(int i = 0; i < repeat; i++)
            {
                _bridge.Tell(new EventLog()
                {
                    uuid = Guid.NewGuid().ToString(),
                    event_type = "test1",
                    event_action = "test2",
                    etc_num1 = i,
                    etc_num2 = i+1,
                    etc_num3 = i+2,
                    etc_str1 = "test3",
                    etc_str2 = "test4",
                    etc_str3 = "test5",
                    event_ver = "1.0.0",
                    user_ip = "127.0.0.1",
                    upd_dt = DateTime.Now
                } );
            }
            return Ok(repeat);  
        } 
    }
}