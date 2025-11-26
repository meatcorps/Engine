using Raylib_cs;

namespace Meatcorps.Engine.RayLib.Interfaces;

public interface IPostProcessor: IResourceLoadOnInit
{
    bool Enabled { get; set; }
    bool IncludeUI { get; set; }
    void Apply(Texture2D source, RenderTexture2D target);
    void BeginFrame(float deltaTime);
    void EndFrame();
}