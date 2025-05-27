using System;
using UniRx;
using VContainer;
using ClockApp.Application.UseCases;
using ClockApp.Domain.Timer;
using ClockApp.Infrastructure.Audio;

namespace ClockApp.Presentation.ViewModels
{
    public class TimerViewModel : IDisposable
    {
        private readonly TimerUseCase _timerUseCase;
        private readonly AudioManager _audioManager;
        private readonly CompositeDisposable _disposables;
        
        private readonly ReactiveProperty<string> _timeDisplay;
        private readonly ReactiveProperty<bool> _canStart;
        private readonly ReactiveProperty<bool> _canPause;
        private readonly ReactiveProperty<bool> _canStop;
        
        private readonly ReactiveProperty<int> _hours;
        private readonly ReactiveProperty<int> _minutes;
        private readonly ReactiveProperty<int> _seconds;
        
        public IReadOnlyReactiveProperty<string> TimeDisplay => _timeDisplay;
        public IReadOnlyReactiveProperty<bool> CanStart => _canStart;
        public IReadOnlyReactiveProperty<bool> CanPause => _canPause;
        public IReadOnlyReactiveProperty<bool> CanStop => _canStop;
        
        public IReactiveProperty<int> Hours => _hours;
        public IReactiveProperty<int> Minutes => _minutes;
        public IReactiveProperty<int> Seconds => _seconds;
        
        public IReadOnlyReactiveProperty<TimerState> State => _timerUseCase.State;
        
        [Inject]
        public TimerViewModel(TimerUseCase timerUseCase, AudioManager audioManager)
        {
            _timerUseCase = timerUseCase;
            _audioManager = audioManager;
            _disposables = new CompositeDisposable();
            
            _timeDisplay = new ReactiveProperty<string>("00:00:00");
            _canStart = new ReactiveProperty<bool>(true);
            _canPause = new ReactiveProperty<bool>(false);
            _canStop = new ReactiveProperty<bool>(false);
            
            _hours = new ReactiveProperty<int>(0);
            _minutes = new ReactiveProperty<int>(0);
            _seconds = new ReactiveProperty<int>(0);
            
            SetupBindings();
        }
        
        private void SetupBindings()
        {
            _timerUseCase.RemainingTime
                .Subscribe(time =>
                {
                    _timeDisplay.Value = $"{time.Hours:D2}:{time.Minutes:D2}:{time.Seconds:D2}";
                })
                .AddTo(_disposables);
            
            _timerUseCase.State
                .Subscribe(state =>
                {
                    _canStart.Value = state == TimerState.Idle || state == TimerState.Paused;
                    _canPause.Value = state == TimerState.Running;
                    _canStop.Value = state == TimerState.Running || state == TimerState.Paused;
                })
                .AddTo(_disposables);
            
            _timerUseCase.OnTimerCompleted
                .Subscribe(_ =>
                {
                    _audioManager.PlayTimerComplete();
                })
                .AddTo(_disposables);
            
            _hours.Subscribe(h => _hours.Value = Math.Max(0, Math.Min(23, h))).AddTo(_disposables);
            _minutes.Subscribe(m => _minutes.Value = Math.Max(0, Math.Min(59, m))).AddTo(_disposables);
            _seconds.Subscribe(s => _seconds.Value = Math.Max(0, Math.Min(59, s))).AddTo(_disposables);
        }
        
        public void StartTimer()
        {
            _timerUseCase.SetTimer(_hours.Value, _minutes.Value, _seconds.Value);
            
            _timerUseCase.StartTimer();
            _audioManager.PlayButtonClick();
        }
        
        public void PauseTimer()
        {
            _timerUseCase.PauseTimer();
            _audioManager.PlayButtonClick();
        }
        
        public void StopTimer()
        {
            _timerUseCase.StopTimer();
            _audioManager.PlayButtonClick();
        }
        
        public void ResetTimer()
        {
            _timerUseCase.ResetTimer();
            _audioManager.PlayButtonClick();
        }
        
        public void SetPreset(int minutes)
        {
            _hours.Value = minutes / 60;
            _minutes.Value = minutes % 60;
            _seconds.Value = 0;
            _audioManager.PlayButtonClick();
        }
        
        public void Dispose()
        {
            _disposables?.Dispose();
            _timeDisplay?.Dispose();
            _canStart?.Dispose();
            _canPause?.Dispose();
            _canStop?.Dispose();
            _hours?.Dispose();
            _minutes?.Dispose();
            _seconds?.Dispose();
        }
    }
}