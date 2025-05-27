using System;
using ClockApp.Domain.Timer;
using UniRx;
using VContainer;

namespace ClockApp.Application.UseCases
{
    /// <summary>
    /// Use case for timer functionality
    /// </summary>
    public class TimerUseCase : IDisposable
    {
        private readonly ITimerService _timerService;
        private readonly CompositeDisposable _disposables;

        public IReadOnlyReactiveProperty<TimeSpan> RemainingTime => _timerService.RemainingTime;
        public IReadOnlyReactiveProperty<TimerState> State => _timerService.State;
        public IObservable<Unit> OnTimerCompleted => _timerService.OnTimerCompleted;

        [Inject]
        public TimerUseCase(ITimerService timerService)
        {
            _timerService = timerService;
            _disposables = new CompositeDisposable();

            // Log timer events for analytics
            _timerService.OnTimerCompleted
                .Subscribe(_ => LogTimerCompletion())
                .AddTo(_disposables);
        }

        public void SetTimer(int hours, int minutes, int seconds)
        {
            var duration = new TimeSpan(hours, minutes, seconds);
            _timerService.SetDuration(duration);
        }

        public void StartTimer()
        {
            _timerService.Start();
            LogTimerEvent("Started");
        }

        public void PauseTimer()
        {
            _timerService.Pause();
            LogTimerEvent("Paused");
        }

        public void StopTimer()
        {
            _timerService.Stop();
            LogTimerEvent("Stopped");
        }

        public void ResetTimer()
        {
            _timerService.Reset();
            LogTimerEvent("Reset");
        }

        private void LogTimerCompletion()
        {
            // Here we could send analytics, show notification, etc.
            UnityEngine.Debug.Log($"Timer completed at {DateTime.Now}");
        }

        private void LogTimerEvent(string eventName)
        {
            // Analytics logging
            UnityEngine.Debug.Log($"Timer event: {eventName} at {DateTime.Now}");
        }

        public void Dispose()
        {
            _disposables?.Dispose();
        }
    }
}