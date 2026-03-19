using ImGuiNET;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.RayLib.ImGuiTools.Controllers;
using Meatcorps.Engine.RayLib.Interfaces;
using Meatcorps.Engine.RayLib.Resources;

namespace Meatcorps.Engine.RayLib.ImGuiTools;

public class ImGuiTextManager<T>: IResourceLoadOnInit, IDisposable where T : Enum
{
    private bool _loaded;
    private int _totalResources;
    private static Dictionary<T, byte[]> _loadedFonts = new ();
    private static Dictionary<(T, int), ImFontPtr> _loadedFontsPtr = new ();
    private static UseFontDisposable _useFontDisposableDisposable = new();
    private static bool _disposed;
    
    public int TotalResources
    {
        get
        {
            if (_loaded)
                return _totalResources;

            _totalResources = GlobalObjectManager.ObjectManager.Get<TextManager<T>>()!.TotalResources;
            _loaded = true;
            return _totalResources;
        }
    }

    public int ResourcesLoaded { get; private set; }
    
    public Task Load()
    {
        foreach (var fonts in GlobalObjectManager.ObjectManager.Get<TextManager<T>>()!.GetFontRegistrations())
        {
            _loadedFonts.TryAdd(fonts.Item1,
                GlobalObjectManager.ObjectManager.Get<IRaylibResource>()!.LoadBytes(fonts.Item2));
        }
        
        return Task.CompletedTask;
    }

    public void RequireFont(ImGuiIOPtr io, T font, int size)
    {
        if (!_loadedFonts.ContainsKey(font))
            throw new ArgumentException("Font not loaded");

        if (_loadedFontsPtr.ContainsKey((font, size)))
            return;
        
        var fontData = _loadedFonts[font];
        unsafe
        {
            /*font data comes from managed byte arrays
               atlas must not own/free this memory
               do not remove FontDataOwnedByAtlas = 0! */
            var config = ImGuiNative.ImFontConfig_ImFontConfig();
            config->FontDataOwnedByAtlas = 0;
            fixed (byte* fontPtr = fontData)
                _loadedFontsPtr.Add((font, size), io.Fonts.AddFontFromMemoryTTF((IntPtr)fontPtr, fontData.Length, size, config));
            
            ResourcesLoaded++;
        }
    }

    public static UseFontDisposable UnsafeUseFont(T font, int size)
    {
        // no safety checks. This is used in hot loops.
        ImGui.PushFont(_loadedFontsPtr[(font, size)]);
        
        return _useFontDisposableDisposable;
    }

    public static ImFontPtr GetFont(T font, int size)
    {
        if (!_loadedFontsPtr.ContainsKey((font, size)))
            throw new ArgumentException("Font not loaded");
        
        return _loadedFontsPtr[(font, size)];
    }
    
    public void Dispose()
    {
        if (_disposed)
            return;
        
        _disposed = true;
        
        //_loadedFonts.Clear();
        //_loadedFontsPtr.Clear();
    }
    
    
    public class UseFontDisposable: IDisposable
    {
        public void Dispose()
        {
            ImGui.PopFont();
        }
    }
}