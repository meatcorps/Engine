
// TODO: Arguments

using System.Text;
using Meatcorps.Engine.Assets.Data;
using Meatcorps.Engine.Assets.Manager;
using Meatcorps.Engine.Assets.Sinks;
using Newtonsoft.Json;

var assetFileName = "Assets.bin";
var encryptDecrypt = new AesResourceSink("HELLO_EVERYBODY");

const string ASSETS_TEMP = "Assets.temp";

var files = Directory.GetFiles("Assets", "*.*", SearchOption.AllDirectories);

if (File.Exists(ASSETS_TEMP))
    File.Delete(ASSETS_TEMP);
var fileTempWriter = File.Create(ASSETS_TEMP);

var map = new AssetMapData();
var currentPosition = 0;
var counter = 0;
foreach (var file in files)
{
    var dataOriginal = File.ReadAllBytes(file);
    var data = encryptDecrypt.Encrypt(dataOriginal);
    fileTempWriter.Write(data);
    map.Items.Add(new AssetMapItemData()
    {
        Path = file,
        Length = data.Length,
        PlainLength = dataOriginal.Length,
        Position = currentPosition
    });
    currentPosition += data.Length;
    
    counter++;
    Console.WriteLine("[" + counter + "/" + files.Length + "] Added: " + file);
}

fileTempWriter.Close();
Console.WriteLine("Asset data done. Let's finalize it.");

var jsonData = JsonConvert.SerializeObject(map, Formatting.None);
var mapData = encryptDecrypt.Encrypt(Encoding.UTF8.GetBytes(jsonData));

var length = mapData.Length;

Console.WriteLine("Let's encrypt the length.");
var lengthString = encryptDecrypt.Encrypt(
    Encoding.UTF8.GetBytes(
        length.ToString())).ToList();

if (File.Exists(assetFileName))
    File.Delete(assetFileName);


while (lengthString.Count < 256)
    lengthString.Insert(0, 0);

Console.WriteLine("Let's write everything out.");
var fileWriter = File.Create(assetFileName);
fileWriter.Write(lengthString.ToArray());
fileWriter.Write(mapData);
fileWriter.Write(File.ReadAllBytes(ASSETS_TEMP));
fileWriter.Close();

Console.WriteLine("File created: " + assetFileName);
File.Delete(ASSETS_TEMP);

Console.WriteLine("Verifying: " + assetFileName);
var assetPackager = new AssetPackageManager(assetFileName, encryptDecrypt);
assetPackager.Load();

foreach (var file in files)
    Console.WriteLine("Exist: " + file + " " + assetPackager.Exists(file));