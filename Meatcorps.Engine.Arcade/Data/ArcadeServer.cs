namespace Meatcorps.Engine.Arcade.Data;

public class ArcadeServer
{
    public string Url { get; init; } = "HTTP://LOCALHOST:8080/";
    
    public string AutoSignIn(ArcadeGame game) => Url + "SIGNIN/" + game.Code;
}