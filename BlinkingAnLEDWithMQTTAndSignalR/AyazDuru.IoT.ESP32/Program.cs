using nanoFramework.Networking;
using nanoFramework.SignalR.Client;
using System;
using System.Device.Gpio;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Net;
using nanoFramework.M2Mqtt;
using nanoFramework.Json;
using nanoFramework.M2Mqtt.Messages;

namespace AyazDuru.IoT.ESP32
{
    public class Program
    {
        const string DeviceID = "nano-device";
        const string MQTTServerAddress = "192.168.1.100";
        const string SignalRServerUrl = "http://192.168.1.100:5000/test-hub";
        const string Ssid = "WifiAd";
        const string Password = "WifiÞifre";
        private static bool _ledStatus = false;
        private static GpioController _gpioController = new GpioController();
        private static GpioPin _led;
        public static bool LedStatus
        {
            get
            {
                return _ledStatus;
            }
            set
            {
                _ledStatus = value;
                if (_led != null)
                {
                    _led.Write(_ledStatus == true ? PinValue.High : PinValue.Low);
                }
            }
        }
        static void ClientMqttMsgReceived(object sender, MqttMsgPublishEventArgs e)
        {
            try
            {
                Console.WriteLine($"Message received on topic: {e.Topic}");
                string message = Encoding.UTF8.GetString(e.Message, 0, e.Message.Length);
                Console.WriteLine($"and message: {message}");
                switch(message)
                {
                    case "led=true":
                        LedStatus = true;
                        break;
                    case "led=false":
                        LedStatus = false;
                        break;
                }
                

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in event: {ex}");
            }
        }

        static void ClientMqttMsgPublished(object sender, MqttMsgPublishedEventArgs e)
        {
            Console.WriteLine($"Response from publish with message id: {e.MessageId}");
           
        }
        public static void Main()
        {
            _led = _gpioController.OpenPin(2, PinMode.Output);
            _led.Write(PinValue.Low);
            CancellationTokenSource cs = new(60000);
            var success = WifiNetworkHelper.ScanAndConnectDhcp(Ssid, Password, System.Device.Wifi.WifiReconnectionKind.Automatic, requiresDateTime: true, token: cs.Token);
            if (success == true)
            {
                MqttClient mqtt = new MqttClient(MQTTServerAddress, 1883,false,null,null,MqttSslProtocols.TLSv1_2);
                mqtt.ProtocolVersion = MqttProtocolVersion.Version_5;
                // Handler for received messages on the subscribed topics
                mqtt.MqttMsgPublishReceived += ClientMqttMsgReceived;
                // Handler for publisher
                mqtt.MqttMsgPublished += ClientMqttMsgPublished;
                mqtt.Connect(DeviceID, true);
                var token = new CancellationTokenSource(30000).Token;
                while (!mqtt.IsConnected && !token.IsCancellationRequested)
                {
                    Thread.Sleep(200);
                }
                mqtt.Subscribe(new string[] { $"devices/{DeviceID}/led" }, new MqttQoSLevel[] {  MqttQoSLevel.AtLeastOnce });
                var options = new HubConnectionOptions() { Reconnect = true, Certificate = null, SslProtocol = System.Net.Security.SslProtocols.None, SslVerification = System.Net.Security.SslVerification.NoVerification };
                HubConnection hubConnection = new HubConnection(SignalRServerUrl, options: options);

                hubConnection.Closed += HubConnection_Closed;
                hubConnection.Start();
                var ledStatusResponse = hubConnection.InvokeCore("GetLedStatus", typeof(bool), new object[] { }, -1);
                LedStatus = (bool)ledStatusResponse;
                hubConnection.On("ReceiveMessage", new Type[] { typeof(string), typeof(string) }, (sender, args) =>
                {
                    var name = (string)args[0];
                    var message = (string)args[1];
                    if (name == "\"LED\"")
                    {
                        bool ledStatus = message == "\"" + bool.TrueString.ToLower() + "\"" ? true : false;
                        LedStatus = ledStatus;
                    }
                    Console.WriteLine($"{name} : {message}");
                });


                while (true)
                {
                    hubConnection.InvokeCore("SendMessage", null, new object[] { DateTime.UtcNow.ToString(), "THIS IS TEST MESSAGE FROM ESP32" });
                    mqtt.Publish($"devices/{DeviceID}/led/data", Encoding.UTF8.GetBytes(DateTime.UtcNow.ToString()), null, null, MqttQoSLevel.AtLeastOnce, false);
                    Thread.Sleep(15000);
                }
            }
            else
            {
                throw new Exception("Wifi failed to connect");
            }
        }
        private static void HubConnection_Closed(object sender, SignalrEventMessageArgs message)
        {
            Debug.WriteLine($"closed received with message: {message.Message}");
        }
    }
}
