using AyazDuru.IoT.Web.Hubs;
using AyazDuru.IoT.Web.Models;
using AyazDuru.IoT.Web.Services;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using MQTTnet.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(1883, listenOptions =>
    {
        listenOptions.UseMqtt();
    });
    serverOptions.ListenAnyIP(5000, listenOptions =>
    {
        
    });
    serverOptions.ListenAnyIP(5001, listenOptions =>
    {
        listenOptions.UseHttps();
    });   
});

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddSignalR();
builder.Services.AddControllers();
builder.Services.AddSingleton(new Led());

builder.Services.AddHostedMqttServer(
               optionsBuilder =>
               {
                   optionsBuilder.WithDefaultEndpoint();
                   optionsBuilder.WithDefaultEndpointPort(1883);
               });

builder.Services.AddMqttConnectionHandler();
builder.Services.AddConnections();
builder.Services.AddMqttTcpServerAdapter();
builder.Services.AddSingleton<MqttController>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    //app.UseHsts();
}

//app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();
app.MapHub<TestHub>("/test-hub");
app.MapControllers();




app.MapConnectionHandler<MqttConnectionHandler>(
                       "/mqtt",
                       httpConnectionDispatcherOptions => httpConnectionDispatcherOptions.WebSockets.SubProtocolSelector =
                           protocolList => protocolList.FirstOrDefault() ?? string.Empty);
app.UseMqttServer(
              server =>
              {
                  var mqttController = app.Services.GetRequiredService<MqttController>();
                  server.ValidatingConnectionAsync += mqttController.ValidateConnection;
                  server.ClientConnectedAsync += mqttController.OnClientConnected;
                  server.InterceptingPublishAsync += mqttController.InterceptingPublishAsync;
              });


app.Run();
