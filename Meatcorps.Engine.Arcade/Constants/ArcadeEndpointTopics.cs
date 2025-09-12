namespace Meatcorps.Engine.Arcade.Constants;

public static class ArcadeEndpointTopics
{
    public const string REGISTER_GAME = "arcade/registergame";
    public const string SYSTEM_MESSAGE = "arcade/systemmessage";
    public const string WEB_ALLDATA = "arcade/weballdata";
    public const string CHANGE_POINTS = "arcade/changepoints";
    public const string GAMESESSION_SIGNIN_AND_UPDATE = "arcade/playersigninupdate";
    public const string GAMESESSION_SIGNOUT = "arcade/playersignout";
    public const string JOIN_GAME = "arcade/playerjoingame";
    public const string REGISTER_PLAYER = "arcade/registerplayer";
}

public static class ArcadeSystemMessageCommands
{
    public const string GET_ALL_DATA = "GET_ALL_DATA";
}