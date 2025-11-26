using System.Text;
using Meatcorps.Engine.Assets.Data;
using Meatcorps.Engine.Assets.Interfaces;
using Meatcorps.Engine.Assets.Manager;
using Meatcorps.Engine.Core.Interfaces.Security;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Newtonsoft.Json;

namespace Meatcorps.Engine.Assets.Packager.Services;

public class AssetPackager
{
    private readonly string _basePath;
    private readonly string _outputPath;
    private readonly string _name;
    private readonly string _target;
    private readonly IEncryptDecryptSink _encryptDecrypt;
    private readonly TaskLoggingHelper _log;

    public AssetPackager(string basePath, string target, string name, string outputPath, IEncryptDecryptSink encryptDecrypt, TaskLoggingHelper log)
    {
        _basePath = basePath;
        _outputPath = outputPath;
        _name = name;
        _target = target.Replace(_basePath, "");
        _encryptDecrypt = encryptDecrypt;
        _log = log;
    }

    public void Start()
    {
        var assetFileName = Path.Combine(_outputPath, _name);

        const string ASSETS_TEMP = "Assets.temp";

        var files = Directory.GetFiles(Path.Combine(_basePath, _target), "*.*", SearchOption.AllDirectories);

        if (File.Exists(ASSETS_TEMP))
            File.Delete(ASSETS_TEMP);
        var fileTempWriter = File.Create(ASSETS_TEMP);

        var map = new AssetMapData();
        var currentPosition = 0;
        var counter = 0;
        foreach (var file in files)
        {
            var dataOriginal = File.ReadAllBytes(file);
            var data = _encryptDecrypt.Encrypt(dataOriginal);
            fileTempWriter.Write(data);
            map.Items.Add(new AssetMapItemData()
            {
                Path = file.Replace(_basePath, "").Replace("/", "\\"),
                Length = data.Length,
                PlainLength = dataOriginal.Length,
                Position = currentPosition
            });
            currentPosition += data.Length;

            counter++;
            _log.LogMessage(MessageImportance.High, "[" + counter + "/" + files.Length + "] Added: " + file);
        }

        fileTempWriter.Close();
        _log.LogMessage(MessageImportance.High, "Asset data done. Let's finalize it.");

        var jsonData = JsonConvert.SerializeObject(map, Formatting.None);
        var mapData = _encryptDecrypt.Encrypt(Encoding.UTF8.GetBytes(jsonData));

        var length = mapData.Length;

        _log.LogMessage(MessageImportance.High, "Let's encrypt the length.");
        var lengthString = _encryptDecrypt.Encrypt(
            Encoding.UTF8.GetBytes(
                length.ToString())).ToList();

        if (File.Exists(assetFileName))
            File.Delete(assetFileName);


        while (lengthString.Count < 256)
            lengthString.Insert(0, 0);

        _log.LogMessage(MessageImportance.High, "Let's write everything out.");
        var fileWriter = File.Create(assetFileName);
        fileWriter.Write(lengthString.ToArray());
        fileWriter.Write(mapData);
        fileWriter.Write(File.ReadAllBytes(ASSETS_TEMP));
        fileWriter.Close();

        _log.LogMessage(MessageImportance.High, "File created: " + assetFileName);
        File.Delete(ASSETS_TEMP);

        _log.LogMessage(MessageImportance.High, "Verifying: " + assetFileName);
        var assetPackager = new AssetPackageManager(assetFileName, _encryptDecrypt);
        assetPackager.Load();

        foreach (var file in files)
            _log.LogMessage(MessageImportance.High, "Exist: " + file + " " + assetPackager.Exists(file));
    }
}