using AyazDuru.IoT.MAUI.Models;
using Microsoft.AspNetCore.SignalR.Client;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AyazDuru.IoT.MAUI
{
    
    public partial class MainPage : ContentPage
    {
        public ObservableCollection<ESP32Log> ESP32Logs { get; set; } = new ObservableCollection<ESP32Log>();
        private readonly HubConnection _connection;
        bool _ledStatus = false;
        public MainPage()
        {
            InitializeComponent();
            BindingContext = this;
            try
            {
                _connection = new HubConnectionBuilder()
               .WithUrl("https://192.168.1.100:5001/test-hub")
            .WithAutomaticReconnect(new[] { TimeSpan.Zero, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(30) })
               .Build();
                _connection.StartAsync().Wait();
                LedStatus = _connection.InvokeAsync<bool>("GetLedStatus").Result;
                _connection.On<string, string>("ReceiveMessage", (user, message) =>
                {
                    var newMessage = $"{user}: {message}";
                    ESP32Logs.Add(new ESP32Log() { LogMessage = newMessage});
                });
                
            }
            catch (Exception ex)
            {
                DisplayAlert("Error", ex.Message, "OK").Wait();
            }
        }

        public bool LedStatus
        {
            get
            {
                return _ledStatus;
            }
            set
            {
                _ledStatus = value;
                SetTextLedStatus();
            }
        }

        private void SetTextLedStatus()
        {
            CounterBtn.Text = LedStatus == true ? "LED AÇIK 🔥" : "LED KAPALI ❌";
        }
        private async void OnCounterClicked(object sender, EventArgs e)
        {
            try
            {
                //HttpsClientHandlerService handler = new HttpsClientHandlerService();
                //using HttpClient client = new HttpClient(handler.GetPlatformMessageHandler());
                //var result = await client.GetStringAsync("https://192.168.1.100/led");
                //_ledStatus = bool.Parse(result);
                LedStatus = await _connection.InvokeAsync<bool>("GetLedStatus");
                LedStatus = !LedStatus;
                await _connection.InvokeAsync("ChangeLedStatus", LedStatus);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
            SemanticScreenReader.Announce(CounterBtn.Text);
        }
    }
}