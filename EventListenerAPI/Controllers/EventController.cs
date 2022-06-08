using EventListenerAPI.Models;
using EventListenerAPI.Services;

using Microsoft.AspNetCore.Mvc;

namespace EventListenerAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EventController : ControllerBase
    {
        private readonly ILogger<TestController> _logger;

        private readonly IActorBridge _bridge;

        public EventController(ILogger<TestController> logger,  IActorBridge bridge)
        {
            _logger = logger;

            _bridge = bridge;
        }

        [HttpPost(nameof(PostEvent))]  
        public async Task<IActionResult> PostEvent(EventLog eventLog)  
        {
            eventLog.uuid = Guid.NewGuid().ToString();
            eventLog.upd_dt = DateTime.Now;
            
            _bridge.Tell(eventLog);            
            return Ok("ok");
        }
    }
}
