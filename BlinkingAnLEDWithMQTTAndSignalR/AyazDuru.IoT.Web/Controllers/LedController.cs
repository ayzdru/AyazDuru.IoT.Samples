using AyazDuru.IoT.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace AyazDuru.IoT.Web.Controllers
{    
    public class LedController : Controller
    {
        private readonly Led _led;

        public LedController(Led led)
        {
            _led = led;
        }
        [HttpGet("led/{status?}")]
        public IActionResult Led(bool? status)
        {
            if (status.HasValue)
            {
                _led.Status = status.Value;
            }
            return Content(_led.Status.ToString().ToLowerInvariant());
        }
    }
}
