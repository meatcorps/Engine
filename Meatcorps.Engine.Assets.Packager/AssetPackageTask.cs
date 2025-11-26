using System.Reflection;
using Meatcorps.Engine.Assets.Interfaces;
using Meatcorps.Engine.Assets.Packager.Services;

namespace Meatcorps.Engine.Assets.Packager;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

public class AssetPackageTask: Task
{
    [Required]
    public string GameAssemblyPath { get; set; } = string.Empty;

    [Required]
    public string ProjectDirectory { get; set; } = string.Empty;

    [Required]
    public string OutputPath { get; set; } = string.Empty;
    
    public override bool Execute()
    {
        try
        {
            var asm = Assembly.LoadFrom(GameAssemblyPath);

            Log.LogWarning("Assembly: " + GameAssemblyPath);
            var configType = asm.GetTypes()
                .FirstOrDefault(t =>
                    t is { IsClass: true, IsAbstract: false } &&
                    t.GetInterfaces().Any(i =>
                        i.FullName == "Meatcorps.Engine.Assets.Interfaces.IAssetPackagerConfig"));
            
            if (configType == null)
            {
                Log.LogError($"No IAssetPackagerConfig implementation found in {GameAssemblyPath}.");
                return false;
            }

            var config = new ReflectedConfigAdapter(Activator.CreateInstance(configType)!);

            foreach (var toPack in config.AssetPacks)
            {
                var job = new AssetPackager(ProjectDirectory, toPack.RelativePath, toPack.Name, OutputPath,
                    toPack.EncryptDecryptSink ?? config.EncryptDecryptSink, Log);

                job.Start();
            }

            return true;
        }
        catch (Exception ex)
        {
            Log.LogErrorFromException(ex, showStackTrace: true);
            return false;
        }
    }
}