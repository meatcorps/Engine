namespace Meatcorps.Engine.Arcade.Constants;

public static class ArcadeEndpointTopics
{
    public const string REGISTER_GAME = "out/arcade/registergame";
    public const string SYSTEM_MESSAGE = "out/arcade/systemmessage";
    public const string WEB_ALLDATA = "out/arcade/weballdata";
    public const string CHANGE_POINTS = "out/arcade/changepoints";
    public const string GAMESESSION_SIGNIN_AND_UPDATE = "out/arcade/playersigninupdate";
    public const string GAMESESSION_SIGNOUT = "out/arcade/playersignout";
    public const string JOIN_GAME = "out/arcade/playerjoingame";
    public const string REGISTER_PLAYER = "out/arcade/registerplayer";
    public const string QUESTION = "out/arcade/question";
    public const string QUESTIONRESPONSE = "out/arcade/questionresponse";
    public const string ADMIN_ACTIONS = "arcade/adminactions";
}

public static class ArcadeSystemMessageCommands
{
    public const string GET_ALL_DATA = "GET_ALL_DATA";
}