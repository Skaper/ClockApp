using System;
using UniRx;
using VContainer;
using ClockApp.Domain.Clock;
using ClockApp.Domain.Stopwatch;

namespace ClockApp.Application.UseCases
{
    /// <summary>
    /// Use case for clock functionality, providing a bridge between domain and presentation
    /// </summary>
    public class ClockUseCase : IDisposable
    {
        private readonly IClockService _clockService;
        
        public IReadOnlyReactiveProperty<DateTime> CurrentTime => _clockService.CurrentTime;
        public IReadOnlyReactiveProperty<DateTime> UtcTime => _clockService.UtcTime;
        public IReadOnlyReactiveProperty<DateTime> JstTime => _clockService.JstTime;
        public IReadOnlyReactiveProperty<bool> IsSynchronized => _clockService.IsSynchronized;
        
        [Inject]
        public ClockUseCase(IClockService clockService)
        {
            _clockService = clockService;
        }
        
        public void StartClock()
        {
            _clockService.StartSync();
        }
        
        public void StopClock()
        {
            _clockService.StopSync();
        }
        
        public void ForceSync()
        {
            _clockService.ForceSync();
        }
        
        public void Dispose() { }
    }
}