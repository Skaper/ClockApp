using System;
using UniRx;
using VContainer;

namespace ClockApp.Domain.Stopwatch
{
    public class StopwatchService : IStopwatchService, IDisposable
    {
        private readonly ReactiveProperty<TimeSpan> _elapsedTime = new(TimeSpan.Zero);
        private readonly ReactiveProperty<bool> _isRunning = new(false);
        private readonly ReactiveCollection<LapTime> _lapTimes = new();

        private readonly CompositeDisposable _disposables = new();
        private IDisposable _updateSubscription;

        private readonly ITimeSource _timeSource;

        private float _startTime;
        private float _pausedTime;
        private float _totalPausedDuration;
        private int _lapIndex;

        public IReadOnlyReactiveProperty<TimeSpan> ElapsedTime => _elapsedTime;
        public IReadOnlyReactiveProperty<bool> IsRunning => _isRunning;
        public IReadOnlyReactiveCollection<LapTime> LapTimes => _lapTimes;

        [Inject]
        public StopwatchService(ITimeSource timeSource)
        {
            _timeSource = timeSource;
        }

        public void Start()
        {
            if (_isRunning.Value) return;

            if (_elapsedTime.Value == TimeSpan.Zero)
            {
                _startTime = _timeSource.GetTime();
                _totalPausedDuration = 0f;
                _lapIndex = 0;
            }
            else
            {
                _totalPausedDuration += _timeSource.GetTime() - _pausedTime;
            }

            _isRunning.Value = true;
            StartUpdating();
        }

        public void Stop()
        {
            if (!_isRunning.Value) return;

            _pausedTime = _timeSource.GetTime();
            _isRunning.Value = false;
            StopUpdating();
            UpdateElapsedTime();
        }

        public void Reset()
        {
            _isRunning.Value = false;
            _elapsedTime.Value = TimeSpan.Zero;
            _lapTimes.Clear();
            _lapIndex = 0;
            _totalPausedDuration = 0f;
            StopUpdating();
        }

        public void RecordLap()
        {
            if (!_isRunning.Value || _elapsedTime.Value == TimeSpan.Zero)
                return;

            var currentElapsed = _elapsedTime.Value;
            var previousTotal = _lapTimes.Count > 0
                ? _lapTimes[^1].TotalTime
                : TimeSpan.Zero;

            var lapTime = currentElapsed - previousTotal;

            _lapTimes.Add(new LapTime
            {
                Index = ++_lapIndex,
                Time = lapTime,
                TotalTime = currentElapsed
            });
        }

        private void StartUpdating()
        {
            StopUpdating();

            _updateSubscription = Observable.EveryUpdate()
                .Where(_ => _isRunning.Value)
                .Subscribe(_ => UpdateElapsedTime())
                .AddTo(_disposables);
        }

        private void StopUpdating()
        {
            _updateSubscription?.Dispose();
            _updateSubscription = null;
        }

        public void ManualUpdateElapsedTime() => UpdateElapsedTime();
        
        private void UpdateElapsedTime()
        {
            var currentTime = _timeSource.GetTime();
            var totalElapsed = currentTime - _startTime - _totalPausedDuration;
            _elapsedTime.Value = TimeSpan.FromSeconds(Math.Max(0, totalElapsed));
        }

        public void Dispose()
        {
            StopUpdating();
            _disposables.Dispose();
        }
    }
}
