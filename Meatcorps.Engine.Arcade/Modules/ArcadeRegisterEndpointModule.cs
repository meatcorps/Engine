using Meatcorps.Engine.Arcade.Constants;
using Meatcorps.Engine.Arcade.Data;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.MQTT.Modules;

namespace Meatcorps.Engine.Arcade.Modules;

public class ArcadeRegisterEndpointModule
{
    public static ArcadeRegisterEndpointModule Load(MQTTModule mqttModule)
    {
        mqttModule.RegisterComplexObject(ArcadeEndpointTopics.CHANGE_POINTS, false, true, new ArcadePointChange(), false);
        mqttModule.RegisterComplexObject(ArcadeEndpointTopics.REGISTER_GAME, true, false, new ArcadeGame
        {
            MaxPlayers = 1,
            Name = "TEMPLATE!",
            Code = 0,
            PricePoints = 1000,
            Description = "The most gore version of the game ever made.",
        }, false);
        mqttModule.RegisterComplexObject(ArcadeEndpointTopics.GAMESESSION_SIGNIN_AND_UPDATE, true, false, new ArcadePlayer(), false);
        mqttModule.RegisterComplexObject(ArcadeEndpointTopics.GAMESESSION_SIGNOUT, false, true, new ArcadePlayer(), false);
        mqttModule.RegisterComplexObject(ArcadeEndpointTopics.REGISTER_PLAYER, false, true, new ArcadePlayer(), false);
        mqttModule.RegisterComplexObject(ArcadeEndpointTopics.JOIN_GAME, false, true, new ArcadePlayer(), false);
        mqttModule.RegisterComplexObject(ArcadeEndpointTopics.WEB_ALLDATA, true, false, new ArcadeCentralData());
        mqttModule.RegisterComplexObject(ArcadeEndpointTopics.SYSTEM_MESSAGE, false, false, new ArcadeSystemMessage(), false);
        
        return new ArcadeRegisterEndpointModule();
    }
}