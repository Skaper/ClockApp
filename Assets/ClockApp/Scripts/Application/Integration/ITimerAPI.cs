using System;
using ClockApp.Domain.Timer;
using UniRx;

namespace ClockApp.Application.Integration
{
    public interface ITimerAPI
    {
        void StartTimer(TimeSpan duration);
        void PauseTimer();
        void StopTimer();
        void ResetTimer();
        TimeSpan GetTimerRemainingTime();
        TimerState GetTimerState();
        IObservable<Unit> OnTimerCompleted { get; }
        
        IObservable<TimeSpan> TimerRemainingTime { get; }
        IObservable<TimerState> TimerState { get; }
    }
}