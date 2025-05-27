using System;
using UniRx;

namespace ClockApp.Domain.Clock
{
    public interface IClockService
    {
        IReadOnlyReactiveProperty<DateTime> CurrentTime { get; }
        IReadOnlyReactiveProperty<DateTime> UtcTime { get; }
        IReadOnlyReactiveProperty<DateTime> JstTime { get; }
        IReadOnlyReactiveProperty<bool> IsSynchronized { get; }
        
        void StartSync();
        void StopSync();
        void ForceSync();
    }
}