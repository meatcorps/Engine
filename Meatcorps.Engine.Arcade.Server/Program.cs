// See https://aka.ms/new-console-template for more information

using Meatcorps.Engine.Arcade.Constants;
using Meatcorps.Engine.Arcade.Data;
using Meatcorps.Engine.Arcade.Server.Managers;
using Meatcorps.Engine.Core.Modules;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Core.Server;
using Meatcorps.Engine.Core.Storage.Data;
using Meatcorps.Engine.Logging.Module;
using Meatcorps.Engine.MQTT.Modules;
using Meatcorps.Engine.Signals.Modules;

ConsoleLoggingModule.Load();
CoreModule.Load();
BasicConfig.Load();
SignalModule.Load();

var serverApplication = new ServerApplication();

var mqttModule = MQTTModule.Load();

mqttModule.RegisterComplexObject(ArcadeEndpointTopics.CHANGE_POINTS, true, false, new ArcadePointChange());
mqttModule.RegisterComplexObject(ArcadeEndpointTopics.REGISTER_GAME, true, false, new ArcadeGame
{
    MaxPlayers = 1,
    Name = "TEMPLATE!",
    Code = 0,
    PricePoints = 1000,
    Description = "The most gore version of the game ever made.",
});
mqttModule.RegisterComplexObject(ArcadeEndpointTopics.GAMESESSION_SIGNIN_AND_UPDATE, false, true, new ArcadePlayer(), false);
mqttModule.RegisterComplexObject(ArcadeEndpointTopics.GAMESESSION_SIGNOUT, true, false, new ArcadePlayer());
mqttModule.RegisterComplexObject(ArcadeEndpointTopics.REGISTER_PLAYER, true, false, new ArcadePlayer());
mqttModule.RegisterComplexObject(ArcadeEndpointTopics.JOIN_GAME, true, false, new ArcadePlayer());
mqttModule.RegisterComplexObject(ArcadeEndpointTopics.WEB_ALLDATA, false, true, new ArcadeCentralData(),false);
mqttModule.RegisterComplexObject(ArcadeEndpointTopics.SYSTEM_MESSAGE, true, false, new ArcadeSystemMessage(), false);

mqttModule.Create();

GlobalObjectManager.ObjectManager.Register(new DataManager());
GlobalObjectManager.ObjectManager.Register(new GamesManager());
GlobalObjectManager.ObjectManager.Register(new PlayerManager());

await serverApplication.Run();