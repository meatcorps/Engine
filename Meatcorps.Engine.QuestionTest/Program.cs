// See https://aka.ms/new-console-template for more information

using Meatcorps.Engine.Arcade.Constants;
using Meatcorps.Engine.Arcade.Data;
using Meatcorps.Engine.Arcade.Interfaces;
using Meatcorps.Engine.Arcade.Services;
using Meatcorps.Engine.Core.Interfaces.Services;
using Meatcorps.Engine.Core.Modules;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Core.Server;
using Meatcorps.Engine.Logging.Module;
using Meatcorps.Engine.MQTT;
using Meatcorps.Engine.MQTT.Modules;
using Meatcorps.Engine.QuestionTest;
using Meatcorps.Game.CyberPlayer.GameEnums;

LoggingModule.Load();
var serverApplication = new ServerApplication();
var simpleGameLoop = new SimpleGameLoop();
CoreModule.Load();
GlobalObjectManager.ObjectManager.Register(simpleGameLoop);
var settings = GameConfig<GameSettings>.Create();

var mqtt = MQTTModule.Load();
mqtt.RegisterComplexObject(ArcadeEndpointTopics.QUESTION, false, true, new ArcadeQuestion(), false);
mqtt.RegisterComplexObject(ArcadeEndpointTopics.QUESTIONRESPONSE, true, false, new ArcadeResponse(), false);
mqtt.Create();

GlobalObjectManager.ObjectManager.Register<IPlayerCheckin>(new SimplePlayerCheckin());
var questionService = new ArcadeQuestionService();
GlobalObjectManager.ObjectManager.Register(questionService);
GlobalObjectManager.ObjectManager.Add<IBackgroundService>(GlobalObjectManager.ObjectManager.Get<ArcadeQuestionService>()!);

questionService.Responses.Subscribe(response =>
{
    Console.WriteLine("Received response " + response.From + ": " + response.Message);
});

simpleGameLoop.Start();

while (!GlobalObjectManager.ObjectManager.Get<MQTTClient>()!.IsConnected)
{
    if (!serverApplication.Running)
        break;
    Console.WriteLine("Waiting for MQTT to connect...");
    await Task.Delay(100);
}

if (serverApplication.Running)
{
    Console.WriteLine("Asking the question!");
    questionService.AskQuestion("NOG 15 MINUTEN!!!", 1, 30000, "Jason", "Hum");
}

await serverApplication.Run();