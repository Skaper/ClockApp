using System;
using ClockApp.Domain.Stopwatch;
using ClockApp.Domain.Timer;
using UniRx;
using VContainer;

namespace ClockApp.Application.Integration
{
    public interface IClockEventBroker
    {
        IObservable<ClockEvent> Events { get; }
    }
    
    public class ClockEventBroker : IClockEventBroker, IDisposable
    {
        private readonly Subject<ClockEvent> _events;
        private readonly CompositeDisposable _disposables;
        
        public IObservable<ClockEvent> Events => _events;
        
        [Inject]
        public ClockEventBroker(
            ITimerService timerService,
            IStopwatchService stopwatchService)
        {
            _events = new Subject<ClockEvent>();
            _disposables = new CompositeDisposable();
            
            timerService.OnTimerCompleted
                .Subscribe(_ => _events.OnNext(new ClockEvent 
                { 
                    Type = ClockEventType.TimerCompleted, 
                    Timestamp = DateTime.Now 
                }))
                .AddTo(_disposables);
                
            timerService.State
                .DistinctUntilChanged()
                .Subscribe(state => _events.OnNext(new ClockEvent 
                { 
                    Type = ClockEventType.TimerStateChanged, 
                    Timestamp = DateTime.Now,
                    Data = state
                }))
                .AddTo(_disposables);
        }
        
        public void Dispose()
        {
            _disposables?.Dispose();
            _events?.OnCompleted();
            _events?.Dispose();
        }
    }
    
    public class ClockEvent
    {
        public ClockEventType Type { get; set; }
        public DateTime Timestamp { get; set; }
        public object Data { get; set; }
    }
    
    public enum ClockEventType
    {
        TimerStarted,
        TimerPaused,
        TimerStopped,
        TimerCompleted,
        TimerStateChanged,
        StopwatchStarted,
        StopwatchStopped,
        StopwatchReset,
        LapRecorded
    }
}