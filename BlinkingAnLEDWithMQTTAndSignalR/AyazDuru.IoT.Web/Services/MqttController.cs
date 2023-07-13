using AyazDuru.IoT.Web.Models;
using MQTTnet.Server;
using System.Text;

namespace AyazDuru.IoT.Web.Services
{
    public class MqttController
    {
        private readonly Led _led;

        public MqttController(Led led)
        {
            _led = led;
        }

        public Task OnClientConnected(ClientConnectedEventArgs eventArgs)
        {
            Console.WriteLine($"Client '{eventArgs.ClientId}' connected.");
            return Task.CompletedTask;
        }


        public Task ValidateConnection(ValidatingConnectionEventArgs eventArgs)
        {
            Console.WriteLine($"Client '{eventArgs.ClientId}' wants to connect. Accepting!");
            return Task.CompletedTask;
        }
        public Task InterceptingPublishAsync(InterceptingPublishEventArgs arg)
        {
            if(arg.ApplicationMessage.Topic == "devices/nano-device/led")
            {
                var message = Encoding.UTF8.GetString(arg.ApplicationMessage.Payload);
                switch (message)
                {
                    case "led=true":
                        _led.Status = true;
                        break;
                    case "led=false":
                        _led.Status = false;
                        break;
                }
            }
            return Task.CompletedTask;
        }

    }
}
