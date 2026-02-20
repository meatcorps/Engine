using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Meatcorps.Engine.Arcade.Constants;
using Meatcorps.Engine.Arcade.Data;
using Meatcorps.Engine.Core.Interfaces.Services;
using Meatcorps.Engine.Core.Utilities;
using Meatcorps.Engine.MQTT.Enums;
using Meatcorps.Engine.Signals.Data;

namespace Meatcorps.Engine.Arcade.Services;

public class ArcadeResponseService : IBackgroundService, IDisposable
{
    private readonly SignalValue<ArcadeQuestion, MQTTGroup> _questionTracker;
    private readonly SignalValue<ArcadeResponse, MQTTGroup> _questionResponse;
    private readonly HashSet<string> _answeredQuestions = new();
    private string _currentQuestion = string.Empty;
    private TimerOn _timeoutTimer = new TimerOn(1000);
    private readonly FixedTimer _updateTickingTimer = new FixedTimer(100);
    private bool _questionRunning;
    private readonly Subject<Unit> _questionChangeSubject = new();
    public IObservable<Unit> QuestionChange => _questionChangeSubject.AsObservable();
    private readonly Subject<float> _timeoutTickingSubject = new();
    public IObservable<float> TimeoutTicking => _timeoutTickingSubject.Where(_ => _questionRunning).AsObservable();
    private readonly object _lock = new();
    
    public ArcadeResponseService()
    {
        _questionTracker = new SignalValue<ArcadeQuestion, MQTTGroup>(MQTTGroup.Exchange, ArcadeEndpointTopics.QUESTION);
        _questionTracker.ValueChanged += QuestionTrackerOnIncomingValue;
        _questionResponse =
            new SignalValue<ArcadeResponse, MQTTGroup>(MQTTGroup.Exchange, ArcadeEndpointTopics.QUESTIONRESPONSE, new ArcadeResponse());
    }

    private void QuestionTrackerOnIncomingValue(ArcadeQuestion value)
    {
        if (_currentQuestion == value.Id)
            return;
        
        _answeredQuestions.Clear();
        _currentQuestion = value.Id;
        _questionRunning = value.Question != "REMOVE";
        _questionChangeSubject.OnNext(Unit.Default);
        _timeoutTimer = new TimerOn(value.Timeout);
    }

    public void PreUpdate(float deltaTime)
    {
    }

    public ArcadeQuestion? GetQuestion(string username)
    {
        lock (_lock) 
        {
            if (!_questionRunning)
                return null;
        
            if (_answeredQuestions.Contains(username))
                return null;
        
            if (_questionTracker.Value.IgnorePlayers.Contains(username))
                return null;
        
            return _questionTracker.Value;
        }
    }

    public void SubmitResponse(string username, string answer)
    {
        if (!_questionRunning)
            return;
        
        lock (_lock)
        {
            _answeredQuestions.Add(username);
            _questionResponse.Value = new ArcadeResponse
            {
                Id = Guid.NewGuid().ToString(),
                From = username,
                Message = answer,
                QuestionId = _questionTracker.Value.Id,
            };
            _questionResponse.Push();
        }
    }
    
    public void Update(float deltaTime)
    {
        _updateTickingTimer.Update(deltaTime);
        _timeoutTimer.Update(_questionRunning, deltaTime);
        if (_timeoutTimer.Output)
        {
            _questionRunning = false;
            lock (_lock)
            {
                _answeredQuestions.Clear();
            }
            _questionChangeSubject.OnNext(Unit.Default);
        }
        else
        {
            if (_updateTickingTimer.Output)
                _timeoutTickingSubject.OnNext(_timeoutTimer.TimeRemaining);
        }
    }

    public void LateUpdate(float deltaTime)
    {
    }

    public void Dispose()
    {
        _questionTracker.Dispose();
        _questionResponse.Dispose();
        _questionChangeSubject.Dispose();
        _timeoutTickingSubject.Dispose();
    }
}