using Meatcorps.Engine.Core.Input;
using Raylib_cs;

namespace Meatcorps.Engine.RayLib.Extensions;

public static class GenericInputExtensions
{
    public static GenericMapper<T> AddInputKeyboard<T>(this GenericMapper<T> mapper, int player, T key, KeyboardKey keyboardKey) where T : Enum
    {
        mapper.AddInput(player, key, keyboardKey.ToString(), () => Raylib.IsKeyDown(keyboardKey) ? 1 : 0);
        return mapper;
    }
}