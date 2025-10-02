using Meatcorps.Engine.Arcade.Constants;
using Meatcorps.Engine.Arcade.Data;
using Meatcorps.Engine.Arcade.Modules;
using Meatcorps.Engine.Arcade.Services;
using Meatcorps.Engine.Core.Interfaces.Services;
using Meatcorps.Engine.Core.Modules;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Core.Server;
using Meatcorps.Engine.Logging.Module;
using Meatcorps.Engine.MQTT.Modules;
using Meatcorps.Game.Pacman.Data;
using Meatcorps.Game.Pacman.GameEnums;
using Meatcorps.Game.Web.TruthOrDare.Components;
using Meatcorps.Game.Web.TruthOrDare.GameEnums;
using Meatcorps.Game.Web.TruthOrDare.Services;

var simpleGameLoop = new SimpleGameLoop();
ConsoleLoggingModule.Load();
CoreModule.Load();
HighScoreModule.Load();
GlobalObjectManager.ObjectManager.Register(simpleGameLoop);
var settings = GameConfig<GameSettings>.Create();

var mqtt = MQTTModule.Load();

var game = new ArcadeGame
{
    MaxPlayers = 1,
    Name = "Truth / Dare!",
    Code = settings.GetOrDefault("ArcadeGame", "Code", 0666),
    PricePoints = settings.GetOrDefault("ArcadeGame", "PricePoints", 100),
    Description = "Ever seen the movie Nerve? Let do in real live!",
};

ArcadeGameSystemModule.Load(game, mqtt);
mqtt.RegisterComplexObject(ArcadeEndpointTopics.QUESTION, false, true, new ArcadeQuestion(), false);
mqtt.RegisterComplexObject(ArcadeEndpointTopics.QUESTIONRESPONSE, true, false, new ArcadeResponse(), false);
mqtt.Create();

GameSession.Load();

GlobalObjectManager.ObjectManager.Register(new ArcadeQuestionService());
GlobalObjectManager.ObjectManager.Add<IBackgroundService>(GlobalObjectManager.ObjectManager.Get<ArcadeQuestionService>()!);

simpleGameLoop.Start();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddSingleton<ObjectManager>(GlobalObjectManager.ObjectManager);
builder.Services.AddSingleton<TruthOrDareService>();
builder.Services.AddScoped<SfxService>();
builder.Services.AddSingleton<ArcadeQuestionService>(GlobalObjectManager.ObjectManager.Get<ArcadeQuestionService>()!);;

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

GlobalObjectManager.ObjectManager.Dispose();