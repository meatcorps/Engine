using Meatcorps.Engine.Core.Interfaces.Resource;
using Raylib_cs;

namespace Meatcorps.Engine.RayLib.Interfaces;

public interface IRaylibResource : IResource
{
    Task<Image> LoadImage(string path);
    Task<Texture2D> LoadTexture(string path);
    Task<Font> LoadFontEx(string fileName, int fontSize, int[]? codepoints, int codepointCount);
    Task<Wave> LoadWave(string path);
    Task<Sound> LoadSound(string path);
    Task<Music> LoadMusic(string path);
    Task<Model> LoadModel(string path);
    Task<Shader> LoadShader(string? vertexPath, string? fragmentPath);
}