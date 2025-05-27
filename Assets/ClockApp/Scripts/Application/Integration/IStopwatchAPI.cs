using System;
using System.Collections.Generic;
using ClockApp.Domain.Stopwatch;

namespace ClockApp.Application.Integration
{
    public interface IStopwatchAPI
    {
        void StartStopwatch();
        void StopStopwatch();
        void ResetStopwatch();
        void RecordLap();
        TimeSpan GetStopwatchElapsedTime();
        IReadOnlyList<LapTime> GetLapTimes();
        bool IsStopwatchRunning();
        
        IObservable<TimeSpan> StopwatchElapsedTime { get; }
        IObservable<bool> StopwatchRunning { get; }
    }
}