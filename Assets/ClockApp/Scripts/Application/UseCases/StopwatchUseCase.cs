using System;
using ClockApp.Domain.Stopwatch;
using UniRx;
using VContainer;

namespace ClockApp.Application.UseCases
{
    /// <summary>
    /// Use case for stopwatch functionality
    /// </summary>
    public class StopwatchUseCase : IDisposable
    {
        private readonly IStopwatchService _stopwatchService;
        private readonly CompositeDisposable _disposables;
        
        public IReadOnlyReactiveProperty<TimeSpan> ElapsedTime => _stopwatchService.ElapsedTime;
        public IReadOnlyReactiveProperty<bool> IsRunning => _stopwatchService.IsRunning;
        public IReadOnlyReactiveCollection<LapTime> LapTimes => _stopwatchService.LapTimes;
        
        [Inject]
        public StopwatchUseCase(IStopwatchService stopwatchService)
        {
            _stopwatchService = stopwatchService;
            _disposables = new CompositeDisposable();
            
            // Track lap records
            _stopwatchService.LapTimes
                .ObserveAdd()
                .Subscribe(x => LogLapTime(x.Value))
                .AddTo(_disposables);
        }
        
        public void StartStopwatch()
        {
            _stopwatchService.Start();
            LogStopwatchEvent("Started");
        }
        
        public void StopStopwatch()
        {
            _stopwatchService.Stop();
            LogStopwatchEvent("Stopped");
        }
        
        public void ResetStopwatch()
        {
            _stopwatchService.Reset();
            LogStopwatchEvent("Reset");
        }
        
        public void RecordLap()
        {
            _stopwatchService.RecordLap();
        }
        
        public string GetFormattedElapsedTime()
        {
            var time = _stopwatchService.ElapsedTime.Value;
            return $"{time.Hours:D2}:{time.Minutes:D2}:{time.Seconds:D2}.{time.Milliseconds / 10:D2}";
        }
        
        public string GetFormattedLapTime(LapTime lap)
        {
            return $"Lap {lap.Index}: {lap.Time.Minutes:D2}:{lap.Time.Seconds:D2}.{lap.Time.Milliseconds / 10:D2}";
        }
        
        private void LogLapTime(LapTime lap)
        {
            UnityEngine.Debug.Log($"Lap {lap.Index} recorded: {GetFormattedLapTime(lap)}");
        }
        
        private void LogStopwatchEvent(string eventName)
        {
            UnityEngine.Debug.Log($"Stopwatch event: {eventName} at {DateTime.Now}");
        }
        
        public void Dispose()
        {
            _disposables?.Dispose();
        }
    }
}