using Meatcorps.Engine.Core.Interfaces.Resource;
using Raylib_cs;

namespace Meatcorps.Engine.RayLib.Interfaces;

public interface IRaylibResource : IResource
{
    Image LoadImage(string path);
    Texture2D LoadTexture(string path);
    Font LoadFontEx(string fileName, int fontSize, int[]? codepoints, int codepointCount);
    Wave LoadWave(string path);
    Sound LoadSound(string path);
    Music LoadMusic(string path);
    Model LoadModel(string path);
    Shader LoadShader(string? vertexPath, string? fragmentPath);
    
}