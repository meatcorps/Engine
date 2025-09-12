using Meatcorps.Engine.Arcade.Constants;
using Meatcorps.Engine.Arcade.Data;
using Meatcorps.Engine.Arcade.Modules;
using Meatcorps.Engine.Arcade.Services;
using Meatcorps.Engine.ArcadeClient;
using Meatcorps.Engine.ArcadeClient.Components;
using Meatcorps.Engine.ArcadeClient.Interfaces;
using Meatcorps.Engine.ArcadeClient.Providers;
using Meatcorps.Engine.ArcadeClient.Services;
using Meatcorps.Engine.Core.Modules;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Core.Storage.Data;
using Meatcorps.Engine.Logging.Module;
using Meatcorps.Engine.MQTT.Modules;

ConsoleLoggingModule.Load();
CoreModule.Load();
BasicConfig.Load();
GlobalObjectManager.ObjectManager.Register(new TestService());

var mqttModule = MQTTModule.Load();
ArcadeRegisterEndpointModule.Load(mqttModule);
mqttModule.Create();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSingleton<TestService>(GlobalObjectManager.ObjectManager.Get<TestService>()!);
builder.Services.AddScoped<IUserIdProvider, BrowserUserIdProvider>();
builder.Services.AddSingleton<ArcadeDataService>();
builder.Services.AddSingleton<PlayerService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

GlobalObjectManager.ObjectManager.Dispose();