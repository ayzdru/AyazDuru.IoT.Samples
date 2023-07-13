using AyazDuru.IoT.Web.Models;
using Microsoft.AspNetCore.SignalR;

namespace AyazDuru.IoT.Web.Hubs
{
    public class TestHub : Hub
    {
        private readonly Led _led;

        public TestHub(Led led)
        {
            _led = led;
        }
        public async Task<bool> GetLedStatus()
        {
            return await Task.FromResult(_led.Status = _led.Status);
        }
        public async Task ChangeLedStatus(bool status)
        { 
            _led.Status = status;
            await Clients.All.SendAsync("ReceiveMessage", "LED", _led.Status.ToString().ToLowerInvariant());
        }
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
    }
}
