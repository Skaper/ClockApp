using System;
using ClockApp.Application.UseCases;
using ClockApp.Infrastructure.Audio;
using UniRx;
using VContainer;

namespace ClockApp.Presentation.ViewModels
{
    public class StopwatchViewModel : IDisposable
    {
        private readonly StopwatchUseCase _stopwatchUseCase;
        private readonly AudioManager _audioManager;
        private readonly CompositeDisposable _disposables;
        
        private readonly ReactiveProperty<string> _timeDisplay;
        private readonly ReactiveProperty<string> _lapButtonText;
        private readonly ReactiveCollection<string> _lapDisplayList;
        
        public IReadOnlyReactiveProperty<string> TimeDisplay => _timeDisplay;
        public IReadOnlyReactiveProperty<string> LapButtonText => _lapButtonText;
        public IReadOnlyReactiveCollection<string> LapDisplayList => _lapDisplayList;
        
        public IReadOnlyReactiveProperty<bool> IsRunning => _stopwatchUseCase.IsRunning;
        public IReadOnlyReactiveProperty<TimeSpan> ElapsedTime => _stopwatchUseCase.ElapsedTime;
        
        [Inject]
        public StopwatchViewModel(StopwatchUseCase stopwatchUseCase, AudioManager audioManager)
        {
            _stopwatchUseCase = stopwatchUseCase;
            _audioManager = audioManager;
            _disposables = new CompositeDisposable();
            
            _timeDisplay = new ReactiveProperty<string>("00:00.00");
            _lapButtonText = new ReactiveProperty<string>("Lap");
            _lapDisplayList = new ReactiveCollection<string>();
            
            SetupBindings();
        }
        
        private void SetupBindings()
        {
            _stopwatchUseCase.ElapsedTime
                .Subscribe(time =>
                {
                    var minutes = (int)time.TotalMinutes;
                    var seconds = time.Seconds;
                    var centiseconds = time.Milliseconds / 10;
                    _timeDisplay.Value = $"{minutes:D2}:{seconds:D2}.{centiseconds:D2}";
                })
                .AddTo(_disposables);
            
            _stopwatchUseCase.IsRunning
                .CombineLatest(_stopwatchUseCase.ElapsedTime, (running, elapsed) => new { running, elapsed })
                .Subscribe(x =>
                {
                    if (!x.running && x.elapsed == TimeSpan.Zero)
                        _lapButtonText.Value = "Lap";
                    else if (x.running)
                        _lapButtonText.Value = "Lap";
                    else
                        _lapButtonText.Value = "Reset";
                })
                .AddTo(_disposables);
            
            _stopwatchUseCase.LapTimes
                .ObserveAdd()
                .Subscribe(x =>
                {
                    var lap = x.Value;
                    var display = $"Lap {lap.Index}: {FormatLapTime(lap.Time)} ({FormatLapTime(lap.TotalTime)})";
                    _lapDisplayList.Insert(0, display); // Add to top
                    while (_lapDisplayList.Count > 50)
                    {
                        _lapDisplayList.RemoveAt(_lapDisplayList.Count - 1);
                    }
                })
                .AddTo(_disposables);
            
            _stopwatchUseCase.LapTimes
                .ObserveReset()
                .Subscribe(_ => _lapDisplayList.Clear())
                .AddTo(_disposables);
        }
        
        public void ToggleStartStop()
        {
            if (_stopwatchUseCase.IsRunning.Value)
            {
                _stopwatchUseCase.StopStopwatch();
            }
            else
            {
                _stopwatchUseCase.StartStopwatch();
            }
            _audioManager.PlayButtonClick();
        }
        
        public void LapOrReset()
        {
            if (_stopwatchUseCase.IsRunning.Value)
            {
                _stopwatchUseCase.RecordLap();
                _audioManager.PlayLapRecord();
            }
            else if (_stopwatchUseCase.ElapsedTime.Value != TimeSpan.Zero)
            {
                _stopwatchUseCase.ResetStopwatch();
                _audioManager.PlayButtonClick();
            }
        }
        
        private string FormatLapTime(TimeSpan time)
        {
            var minutes = (int)time.TotalMinutes;
            var seconds = time.Seconds;
            var centiseconds = time.Milliseconds / 10;
            return $"{minutes:D2}:{seconds:D2}.{centiseconds:D2}";
        }
        
        public void Dispose()
        {
            _disposables?.Dispose();
            _timeDisplay?.Dispose();
            _lapButtonText?.Dispose();
            _lapDisplayList?.Dispose();
        }
    }
}