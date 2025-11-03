using System.Text.Json;
using System.Text.Json.Serialization;
using Meatcorps.Engine.Arcade.Constants;
using Meatcorps.Engine.Arcade.Data;
using Meatcorps.Engine.Arcade.Modules;
using Meatcorps.Engine.Arcade.Services;
using Meatcorps.Engine.ArcadeClient;
using Meatcorps.Engine.ArcadeClient.Components;
using Meatcorps.Engine.ArcadeClient.Interfaces;
using Meatcorps.Engine.ArcadeClient.Providers;
using Meatcorps.Engine.ArcadeClient.Services;
using Meatcorps.Engine.Core.Interfaces.Services;
using Meatcorps.Engine.Core.Modules;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Core.Server;
using Meatcorps.Engine.Core.Storage.Data;
using Meatcorps.Engine.Logging.Module;
using Meatcorps.Engine.MQTT.Modules;

LoggingModule.Load();
CoreModule.Load();
BasicConfig.Load();

GlobalObjectManager.ObjectManager.Register(new TestService());

var simpleGameLoop = new SimpleGameLoop();
GlobalObjectManager.ObjectManager.Register(simpleGameLoop);
var mqttModule = MQTTModule.Load();
ArcadeRegisterEndpointModule.Load(mqttModule);
mqttModule.RegisterComplexObject<ArcadeQuestion>(ArcadeEndpointTopics.QUESTION, true, false, new ArcadeQuestion(), false);
mqttModule.RegisterComplexObject<ArcadeResponse>(ArcadeEndpointTopics.QUESTIONRESPONSE, false, true, new ArcadeResponse(), false);
mqttModule.RegisterComplexObject<ArcadeAdminActions>(ArcadeEndpointTopics.ADMIN_ACTIONS, false, true, new ArcadeAdminActions(), false);
mqttModule.Create();

GlobalObjectManager.ObjectManager.Register(new ArcadeResponseService());
GlobalObjectManager.ObjectManager.Add<IBackgroundService>(GlobalObjectManager.ObjectManager.Get<ArcadeResponseService>()!);
simpleGameLoop.Start();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSingleton<TestService>(GlobalObjectManager.ObjectManager.Get<TestService>()!);
builder.Services.AddSingleton<ArcadeResponseService>(GlobalObjectManager.ObjectManager.Get<ArcadeResponseService>()!);
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