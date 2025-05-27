using System;
using UniRx;

namespace ClockApp.Domain.Timer
{
    public interface ITimerService
    {
        IReadOnlyReactiveProperty<TimeSpan> RemainingTime { get; }
        IReadOnlyReactiveProperty<TimerState> State { get; }
        IObservable<Unit> OnTimerCompleted { get; }
        
        void SetDuration(TimeSpan duration);
        void Start();
        void Pause();
        void Stop();
        void Reset();
    }
    
    public enum TimerState
    {
        Idle,
        Running,
        Paused,
        Completed
    }
}