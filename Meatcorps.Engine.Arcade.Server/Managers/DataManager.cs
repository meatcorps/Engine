using System.Text.Json;
using Meatcorps.Engine.Arcade.Constants;
using Meatcorps.Engine.Arcade.Data;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Core.Storage.Services;
using Meatcorps.Engine.MQTT.Enums;
using Meatcorps.Engine.Signals.Data;
using Meatcorps.Engine.Signals.Enums;

namespace Meatcorps.Engine.Arcade.Server.Managers;

public sealed class DataManager: IDisposable
{
    private readonly PersistentDatabase _database;
    private readonly SignalValue<ArcadeCentralData, SignalDefault> _dataSignal;
    private const string DataKey = "arcade_data";
    private readonly object _serializeLock = new();
    private readonly SignalValue<ArcadeCentralData, MQTTGroup> _webDataDump;
    private readonly SignalValue<ArcadeSystemMessage, MQTTGroup> _message;

    public DataManager()
    {
        _database = GlobalObjectManager.ObjectManager.Get<PersistentDatabase>()!;

        var data = new ArcadeCentralData();
        if (_database.ContainsKey(DataKey))
            data = JsonSerializer.Deserialize<ArcadeCentralData>((string)_database[DataKey]) ?? new ArcadeCentralData();

        _dataSignal =
            new SignalValue<ArcadeCentralData, SignalDefault>(SignalDefault.Internal, nameof(ArcadeCentralData), data);

        _dataSignal.IncomingValue += (_ => Save());
        
        _webDataDump = new SignalValue<ArcadeCentralData, MQTTGroup>(MQTTGroup.Exchange, ArcadeEndpointTopics.WEB_ALLDATA);
        _message = new SignalValue<ArcadeSystemMessage, MQTTGroup>(MQTTGroup.Exchange,  ArcadeEndpointTopics.SYSTEM_MESSAGE);
        _message.IncomingValue += MessageOnValueChanged;
        
        _webDataDump.Value = _dataSignal.Value;
    }

    private void MessageOnValueChanged(ArcadeSystemMessage value)
    {
        if (value.Message != ArcadeSystemMessageCommands.GET_ALL_DATA)
            return;
            
        _webDataDump.Value = _dataSignal.Value;
        _webDataDump.Push();
    }


    private void Save()
    {
        lock (_serializeLock)
        {
            Console.WriteLine("Saving data");
            _database[DataKey] = JsonSerializer.Serialize(_dataSignal.Value);
            _database.Dirty = true;
        }
    }

    public void Dispose()
    {
        Save();
        _dataSignal.Dispose();
        _webDataDump.Dispose();
        _message.Dispose();
    }
}