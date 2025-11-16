namespace Meatcorps.Engine.RayLib.Enums;

public enum GameLoopType
{
    PreRaylibInit,
    PostRaylibInit,
    BeforeUpdate,
    PreUpdate,
    Update,
    LateUpdate,
    AfterUpdate,
    PreRender,
    Render,
    PostRender,
    AfterClosingWindow
}