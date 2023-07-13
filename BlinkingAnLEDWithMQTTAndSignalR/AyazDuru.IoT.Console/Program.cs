using MQTTnet.Client;
using MQTTnet;
using System.Text;

Console.WriteLine("Hello, World!");
var mqttFactory = new MqttFactory();

using (var mqttClient = mqttFactory.CreateMqttClient())
{
    mqttClient.ApplicationMessageReceivedAsync += e =>
    {
        Console.WriteLine("Received application message.");

        string message = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
        Console.WriteLine(message);
        return Task.CompletedTask;
    };
    var mqttClientOptions = new MqttClientOptionsBuilder().WithTcpServer("192.168.1.100", 1883).Build();
    await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);


    await mqttClient.SubscribeAsync("devices/nano-device/led/data", MQTTnet.Protocol.MqttQualityOfServiceLevel.ExactlyOnce);

    await mqttClient.PublishStringAsync("devices/nano-device/led", "led=true", MQTTnet.Protocol.MqttQualityOfServiceLevel.ExactlyOnce);

    Console.WriteLine("Press enter to exit.");
    Console.ReadLine();
    // This will send the DISCONNECT packet. Calling _Dispose_ without DisconnectAsync the 
    // connection is closed in a "not clean" way. See MQTT specification for more details.
    await mqttClient.DisconnectAsync(new MqttClientDisconnectOptionsBuilder().WithReason(MqttClientDisconnectOptionsReason.NormalDisconnection).Build());
}