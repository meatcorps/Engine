using System.Reactive.Linq;
using System.Reactive.Subjects;
using Meatcorps.Engine.Arcade.Constants;
using Meatcorps.Engine.Arcade.Data;
using Meatcorps.Engine.Arcade.Interfaces;
using Meatcorps.Engine.Core.Interfaces.Services;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Core.Utilities;
using Meatcorps.Engine.MQTT.Enums;
using Meatcorps.Engine.Signals.Data;

namespace Meatcorps.Engine.Arcade.Services;

public class ArcadeQuestionService: IBackgroundService, IDisposable
{
    private readonly Subject<ArcadeResponse> _responseSubject = new();
    public IObservable<ArcadeResponse> Responses => _responseSubject
        .Where(response => _isQuestioning && _questionTracker.Value.Id == response.QuestionId)
        .AsObservable();
    
    private readonly SignalValue<ArcadeQuestion, MQTTGroup> _questionTracker;
    private bool _isQuestioning;
    private readonly IPlayerCheckin _playerCheckin;
    private TimerOn _timer = new TimerOn(1000);
    private readonly FixedTimer _pushTimer = new FixedTimer(1000);
    private readonly SignalValue<ArcadeResponse, MQTTGroup> _questionResponse;
    private readonly HashSet<string> _answeredQuestions = new();
    
    public ArcadeQuestionService()
    {
        _questionTracker = new SignalValue<ArcadeQuestion, MQTTGroup>(MQTTGroup.Exchange, ArcadeEndpointTopics.QUESTION,
            new ArcadeQuestion());
        _questionResponse =
            new SignalValue<ArcadeResponse, MQTTGroup>(MQTTGroup.Exchange, ArcadeEndpointTopics.QUESTIONRESPONSE);
        _playerCheckin = GlobalObjectManager.ObjectManager.Get<IPlayerCheckin>()!;
        
        _questionResponse.ValueChanged += value =>
        {
            Console.Write("Incoming response: " + value.Id + " from " + value.From + " for question " + value.QuestionId + " with answer: " + value.Message + "");
            if (!_answeredQuestions.Add(value.Id))
            {
                Console.WriteLine(" [BLOCKED]");
                return;
            }
            Console.WriteLine(" [ACCEPTED]");

            _responseSubject.OnNext(value);
            
        };
    }

    public void AskQuestion(string question, int player, int timeout, params string[] answers)
    {
        if (!_playerCheckin.IsPlayerCheckedIn(player, out var _))
            return;
        
        var ignorePlayers = new List<string>();
        
        for (var i = 0; i < _playerCheckin.TotalPlayers; i++)
        {
            if (_playerCheckin.IsPlayerCheckedIn(i + 1, out var name))
                ignorePlayers.Add(name);
        }
        Console.WriteLine("Push question: " + question);
        _questionTracker.Value = new ArcadeQuestion
        {
             Answers = answers.ToList(),
             From = _playerCheckin.GetPlayerName(player),
             Id = Guid.NewGuid().ToString(),
             Question = question,
             Timeout = timeout,
             IgnorePlayers = ignorePlayers
        };
        _questionTracker.Push();
        
        _isQuestioning = true;
        _timer = new TimerOn(timeout);
    }
    
    public void PreUpdate(float deltaTime)
    {
    }

    public void Update(float deltaTime)
    {
        _timer.Update(_isQuestioning, deltaTime);
        _pushTimer.Update(deltaTime);

        if (_pushTimer.Output && _isQuestioning)
        {
            _questionTracker.Push();
            Console.WriteLine("Pushed question " + _questionTracker.Value.Question + "|" + _timer.TimeRemaining + " milliseconds left");
        }

        if (_timer.Output)
            _isQuestioning = false;
    }

    public void LateUpdate(float deltaTime)
    {
    }

    public void Dispose()
    {
        _responseSubject.Dispose();
        _questionTracker.Dispose();
        _questionResponse.Dispose();
    }
}