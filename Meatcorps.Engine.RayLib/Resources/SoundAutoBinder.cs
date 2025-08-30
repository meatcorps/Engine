using System.Text;

namespace Meatcorps.Engine.RayLib.Resources;

public static class AudioEnumBinder
{
    private static readonly string[] SoundExt = { ".wav", ".ogg" };
    private static readonly string[] MusicExt = { ".ogg", ".mp3", ".wav" };

    public static SoundFxResource<T> BindAllSounds<T>(SoundFxResource<T> res, string baseDir, bool strict = false)
        where T : struct, System.Enum
    {
        var files = Directory.Exists(baseDir)
            ? Directory.GetFiles(baseDir, "*.*", SearchOption.TopDirectoryOnly)
            : System.Array.Empty<string>();

        var map = files.ToLookup(f => Path.GetFileName(f)!.ToLowerInvariant());

        var nonExistent = new List<string>();

        foreach (var value in System.Enum.GetValues(typeof(T)).Cast<T>())
        {
            var name = value.ToString()!;
            var candidates = Variants(name, SoundExt);

            var match = candidates.Select(c => c.ToLowerInvariant()).FirstOrDefault(map.Contains);
            if (match != null)
            {
                var full = files.First(f =>
                    Path.GetFileName(f)!.Equals(match, System.StringComparison.OrdinalIgnoreCase));

                if (File.Exists(full) == false)
                {
                    if (strict)
                        nonExistent.Add($"{value} -> {full} does not map to a file");
                }

                res.AddSound(value, full);
            }
            // else: silently skip if missing (or throw if you prefer strict)
        }

        if (nonExistent.Any())
            throw new Exception("Missing sound files: \n" + string.Join("\n ", nonExistent));

        return res;
    }

    public static MusicResource<T> BindAllMusic<T>(MusicResource<T> res, string baseDir, bool strict = false) where T : struct, System.Enum
    {
        var files = Directory.Exists(baseDir)
            ? Directory.GetFiles(baseDir, "*.*", SearchOption.TopDirectoryOnly)
            : System.Array.Empty<string>();

        var map = files.ToLookup(f => Path.GetFileName(f)!.ToLowerInvariant());

        var nonExistent = new List<string>();
        foreach (var value in System.Enum.GetValues(typeof(T)).Cast<T>())
        {
            var name = value.ToString()!;
            var candidates = Variants(name, MusicExt);

            var match = candidates.Select(c => c.ToLowerInvariant()).FirstOrDefault(map.Contains);
            if (match != null)
            {
                var full = files.First(f =>
                    Path.GetFileName(f)!.Equals(match, System.StringComparison.OrdinalIgnoreCase));
                
                if (File.Exists(full) == false)
                {
                    if (strict)
                        nonExistent.Add($"{value} -> {full} does not map to a file");
                }
                
                res.AddMusic(value, full);
            }
        }

        return res;
    }

    private static IEnumerable<string> Variants(string pascal, string[] exts)
    {
        // PascalCase
        foreach (var ext in exts) yield return pascal + ext;

        // kebab-case
        var kebab = ToSeparated(pascal, '-');
        foreach (var ext in exts) yield return kebab + ext;

        // snake_case
        var snake = ToSeparated(pascal, '_');
        foreach (var ext in exts) yield return snake + ext;

        // lowercase (no separators)
        var lower = new string(pascal.Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();
        foreach (var ext in exts) yield return lower + ext;
    }

    private static string ToSeparated(string pascal, char sep)
    {
        var sb = new StringBuilder();
        for (var i = 0; i < pascal.Length; i++)
        {
            var ch = pascal[i];
            if (i > 0 && char.IsUpper(ch) &&
                (char.IsLower(pascal[i - 1]) || (i + 1 < pascal.Length && char.IsLower(pascal[i + 1]))))
            {
                sb.Append(sep);
            }

            sb.Append(char.ToLowerInvariant(ch));
        }

        return sb.ToString();
    }
}