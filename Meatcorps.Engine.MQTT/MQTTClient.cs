using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using Meatcorps.Engine.Core.Interfaces.Config;
using Meatcorps.Engine.Core.ObjectManager;
using MQTTnet;
using MQTTnet.Client;

namespace Meatcorps.Engine.MQTT;

public class MQTTClient: IDisposable
{
    private readonly string _host;
    private readonly IMqttClient _client;
    private bool _running = true;
    
    private readonly Subject<Tuple<string, string>> _messageReceived = new();
    private readonly Subject<Unit> _connected = new();
    public IObservable<Unit> Connected => _connected.AsObservable();
    public bool IsConnected => _client.IsConnected;
    private readonly bool _verboseTrafficLogging;
    
    public MQTTClient(string host)
    {
        _host = host;
        var factory = new MqttFactory();
        _client = factory.CreateMqttClient();
        _verboseTrafficLogging = GlobalObjectManager.ObjectManager.Get<IUniversalConfig>()!.GetOrDefault("MQTT", "VerboseTrafficLogging", false, false);
    }

    public async Task Connect(string username = "user", string password = "admin", int port = 1883)
    {
        var options = new MqttClientOptionsBuilder()
            .WithCredentials(username, password)
            .WithTcpServer(_host, port) 
            .WithCleanSession()
            .Build();

        _client.ApplicationMessageReceivedAsync += e =>
        {
            var topic = e.ApplicationMessage.Topic;
            var payload = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);

            if (_verboseTrafficLogging)
                Console.WriteLine($"MQTT INCOMING: {topic} > {payload}");
            
            _messageReceived.OnNext(new Tuple<string, string> (item1: topic, item2: payload));

            return Task.CompletedTask;
        };

        _client.ConnectedAsync += _ =>
        {
            if (_verboseTrafficLogging)
                Console.WriteLine("MQTT CONNECTED!");

            _connected.OnNext(Unit.Default);
            return Task.CompletedTask;
        };
        
        _client.DisconnectedAsync += _ =>
        { 
            if (_verboseTrafficLogging)
                Console.WriteLine($"MQTT DISCONNECTED!");

            return Task.CompletedTask;
        };

        while (_running)
        {
            try
            {
                if (_client.IsConnected)
                    await _client.PingAsync(CancellationToken.None);
                else 
                    await _client.ConnectAsync(options, CancellationToken.None);
                
                await Task.Delay(1000);
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }

    public async Task<IObservable<string>> SubscribeToTopic(string topic)
    {
        var returnObservable = _messageReceived.Where(x => x.Item1.Equals(topic))
            .Select(x => x.Item2)
            .AsObservable();
        
        await _client.SubscribeAsync(new MqttClientSubscribeOptionsBuilder()
            .WithTopicFilter(f => f.WithTopic(topic))
            .Build());

        return returnObservable;
    }
    
    public async Task Publish(string topic, string payload)
    {
        if (_verboseTrafficLogging)
            Console.WriteLine($"MQTT PUSH: {topic} > {payload}");

        await _client.PublishAsync(new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(payload)
            .Build());
    }

    public void Dispose()
    {
        _running = false;
        _client.DisconnectAsync().Wait();
        _messageReceived.Dispose();
        _client.Dispose();
    }
}