using System;
using System.Collections.Generic;
using ClockApp.Scripts.Domain.Events;
using UniRx;
using UnityEngine;
using VContainer;

namespace ClockApp.Domain.Stopwatch
{
    public class StopwatchService : IStopwatchService, IDisposable
    {
        private readonly ReactiveProperty<TimeSpan> _elapsedTime;
        private readonly ReactiveProperty<bool> _isRunning;
        private readonly ReactiveCollection<LapTime> _lapTimes;
        
        private readonly CompositeDisposable _disposables;
        private IDisposable _updateSubscription;
        
        private float _startTime;
        private float _pausedTime;
        private float _totalPausedDuration;
        private int _lapIndex;
        
        public IReadOnlyReactiveProperty<TimeSpan> ElapsedTime => _elapsedTime;
        public IReadOnlyReactiveProperty<bool> IsRunning => _isRunning;
        public IReadOnlyReactiveCollection<LapTime> LapTimes => _lapTimes;
        
        [Inject]
        public StopwatchService()
        {
            _elapsedTime = new ReactiveProperty<TimeSpan>(TimeSpan.Zero);
            _isRunning = new ReactiveProperty<bool>(false);
            _lapTimes = new ReactiveCollection<LapTime>();
            _disposables = new CompositeDisposable();
            
            SubscribeToBackgroundEvents();
        }
        
        private void SubscribeToBackgroundEvents()
        {
            // Handle background resume - adjust for time passed
            MessageBroker.Default.Receive<BackgroundResumeEvent>()
                .Where(_ => _isRunning.Value)
                .Subscribe(e =>
                {
                    UpdateElapsedTime();
                })
                .AddTo(_disposables);
        }
        
        public void Start()
        {
            if (_isRunning.Value)
                return;
            
            if (_elapsedTime.Value == TimeSpan.Zero)
            {
                // Fresh start
                _startTime = Time.realtimeSinceStartup;
                _totalPausedDuration = 0f;
                _lapIndex = 0;
            }
            else
            {
                // Resume from pause
                var currentTime = Time.realtimeSinceStartup;
                _totalPausedDuration += currentTime - _pausedTime;
            }
            
            _isRunning.Value = true;
            StartUpdating();
        }
        
        public void Stop()
        {
            if (!_isRunning.Value)
                return;
            
            _pausedTime = Time.realtimeSinceStartup;
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
            
            // Limit lap history to prevent memory issues
            if (_lapTimes.Count > 100)
            {
                _lapTimes.RemoveAt(0);
            }
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
        
        private void UpdateElapsedTime()
        {
            if (_startTime > 0)
            {
                var currentTime = Time.realtimeSinceStartup;
                var totalElapsed = currentTime - _startTime - _totalPausedDuration;
                _elapsedTime.Value = TimeSpan.FromSeconds(Math.Max(0, totalElapsed));
            }
        }
        
        public void Dispose()
        {
            StopUpdating();
            _disposables?.Dispose();
            _elapsedTime?.Dispose();
            _isRunning?.Dispose();
            _lapTimes?.Dispose();
        }
    }
}