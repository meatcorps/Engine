using System.Text.Json;
using Meatcorps.Engine.Core.Interfaces.Storage;
using Meatcorps.Engine.Core.ObjectManager;
using Microsoft.Extensions.Logging;

namespace Meatcorps.Engine.Core.Storage.Services;

public class PersistentLoaderAndSaverService : IKeyValueLoader<string>, IKeyValueSaver<string>, IDisposable
{
    private const string Filename = "session.bda";
    private readonly ILogger<PersistentLoaderAndSaverService> _logger;
    private IKeyValueDatabase<string>? _target;
    private CancellationTokenSource _cancellationTokenSource = new();
    private bool _disposed;

    public PersistentLoaderAndSaverService()
    {
        _logger = LoggingService.GetLogger<PersistentLoaderAndSaverService>();
    }

    public async Task MonitorAsync()
    {
        while (!_cancellationTokenSource.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(5000, _cancellationTokenSource.Token);
            }
            catch (TaskCanceledException)
            {
                //
            }

            if (_target!.Dirty)
            {
                Save();
                _target.Dirty = false;
            }
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;
        _disposed = true;
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
        Save();
    }

    public void GetData(IKeyValueDatabase<string> target)
    {
        _logger.LogInformation("Try to load: {Location}", AppContext.BaseDirectory + Filename);
        if (!File.Exists(Filename))
            return;

        try
        {
            var json = File.ReadAllText(AppContext.BaseDirectory + Filename);

            _logger.LogInformation("Loading json information\n{Json}", json);

            var data = JsonSerializer.Deserialize<Dictionary<string, string>>(json);

            foreach (var key in data!.Keys) target.Add(key, data[key]);
        }
        catch (FormatException)
        {
            File.Move(AppContext.BaseDirectory + Filename,
                AppContext.BaseDirectory +
                $"{Filename}_CouldNotLoad_{DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString()}");
        }
    }

    public void SetTarget(IKeyValueDatabase<string> target)
    {
        _target = target;
    }

    public void Save()
    {
        if (File.Exists(AppContext.BaseDirectory + Filename))
            File.Delete(AppContext.BaseDirectory + Filename);

        if (_target is null)
            return;

        var savingDictionary = _target.Keys.ToDictionary(key => key, key => _target[key].ToString());

        var json = JsonSerializer.Serialize(savingDictionary);

        File.WriteAllText(AppContext.BaseDirectory + Filename, json);

        _logger.LogInformation($"Saving json information\n{json}");
        _logger.LogInformation("To: " + AppContext.BaseDirectory + Filename);

        if (!File.Exists(AppContext.BaseDirectory + Filename))
            _logger.LogInformation("Failed to save data! Backup is here: \n" + json);
    }
}