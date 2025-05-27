using System;
using System.Collections.Generic;
using UniRx;

namespace ClockApp.Domain.Stopwatch
{
    public interface IStopwatchService
    {
        IReadOnlyReactiveProperty<TimeSpan> ElapsedTime { get; }
        IReadOnlyReactiveProperty<bool> IsRunning { get; }
        IReadOnlyReactiveCollection<LapTime> LapTimes { get; }
        
        void Start();
        void Stop();
        void Reset();
        void RecordLap();
    }
    
    public class LapTime
    {
        public int Index { get; set; }
        public TimeSpan Time { get; set; }
        public TimeSpan TotalTime { get; set; }
    }
}