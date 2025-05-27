using System;
using ClockApp.Scripts.Domain.Events;
using UniRx;
using UnityEngine;
using VContainer;

namespace ClockApp.Domain.Timer
{
    public class TimerService : ITimerService, IDisposable
    {
        private readonly ReactiveProperty<TimeSpan> _remainingTime;
        private readonly ReactiveProperty<TimerState> _state;
        private readonly Subject<Unit> _onTimerCompleted;
        
        private readonly CompositeDisposable _disposables;
        private IDisposable _timerSubscription;
        
        private TimeSpan _duration;
        private float _pausedTime;
        private float _startTime;
        private bool _isBackgrounded;
        
        public IReadOnlyReactiveProperty<TimeSpan> RemainingTime => _remainingTime;
        public IReadOnlyReactiveProperty<TimerState> State => _state;
        public IObservable<Unit> OnTimerCompleted => _onTimerCompleted;
        
        [Inject]
        public TimerService()
        {
            _remainingTime = new ReactiveProperty<TimeSpan>(TimeSpan.Zero);
            _state = new ReactiveProperty<TimerState>(TimerState.Idle);
            _onTimerCompleted = new Subject<Unit>();
            _disposables = new CompositeDisposable();
            
            SubscribeToBackgroundEvents();
        }
        
        private void SubscribeToBackgroundEvents()
        {
            // Handle background resume
            MessageBroker.Default.Receive<BackgroundResumeEvent>()
                .Where(_ => _state.Value == TimerState.Running)
                .Subscribe(e =>
                {
                    // Adjust for time passed in background
                    _startTime -= e.PauseDuration;
                    UpdateRemainingTime();
                })
                .AddTo(_disposables);
            
            // Handle focus changes
            MessageBroker.Default.Receive<ApplicationFocusEvent>()
                .Subscribe(e =>
                {
                    _isBackgrounded = !e.HasFocus;
                })
                .AddTo(_disposables);
        }
        
        public void SetDuration(TimeSpan duration)
        {
            if (_state.Value == TimerState.Running)
            {
                Debug.LogWarning("Cannot set duration while timer is running");
                return;
            }
            
            _duration = duration;
            _remainingTime.Value = duration;
        }
        
        public void Start()
        {
            if (_duration == TimeSpan.Zero)
            {
                Debug.LogWarning("Timer duration not set");
                return;
            }
            
            if (_state.Value == TimerState.Paused)
            {
                // Resume from pause
                _startTime = Time.realtimeSinceStartup - _pausedTime;
            }
            else
            {
                // Fresh start
                _startTime = Time.realtimeSinceStartup;
                _remainingTime.Value = _duration;
            }
            
            _state.Value = TimerState.Running;
            StartTimerUpdate();
        }
        
        public void Pause()
        {
            if (_state.Value != TimerState.Running)
                return;
            
            _pausedTime = Time.realtimeSinceStartup - _startTime;
            _state.Value = TimerState.Paused;
            StopTimerUpdate();
        }
        
        public void Stop()
        {
            _state.Value = TimerState.Idle;
            _remainingTime.Value = new TimeSpan();
            StopTimerUpdate();
        }
        
        public void Reset()
        {
            Stop();
        }
        
        private void StartTimerUpdate()
        {
            StopTimerUpdate();
            
            _timerSubscription = Observable.EveryUpdate()
                .Where(_ => _state.Value == TimerState.Running)
                .Subscribe(_ => UpdateRemainingTime())
                .AddTo(_disposables);
        }
        
        private void StopTimerUpdate()
        {
            _timerSubscription?.Dispose();
            _timerSubscription = null;
        }
        
        private void UpdateRemainingTime()
        {
            var elapsed = Time.realtimeSinceStartup - _startTime;
            var remaining = _duration.TotalSeconds - elapsed;
            
            if (remaining <= 0)
            {
                _remainingTime.Value = TimeSpan.Zero;
                _state.Value = TimerState.Completed;
                _onTimerCompleted.OnNext(Unit.Default);
                StopTimerUpdate();
            }
            else
            {
                _remainingTime.Value = TimeSpan.FromSeconds(remaining);
            }
        }
        
        public void Dispose()
        {
            StopTimerUpdate();
            _disposables?.Dispose();
            _remainingTime?.Dispose();
            _state?.Dispose();
            _onTimerCompleted?.Dispose();
        }
    }
}