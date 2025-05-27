using System;
using UniRx;
using VContainer;
using ClockApp.Application.UseCases;

namespace ClockApp.Presentation.ViewModels
{
    public class ClockViewModel : IDisposable
    {
        private readonly ClockUseCase _clockUseCase;
        private readonly CompositeDisposable _disposables;
        
        private readonly ReactiveProperty<string> _localTimeDisplay;
        private readonly ReactiveProperty<string> _localDateDisplay;
        private readonly ReactiveProperty<string> _utcTimeDisplay;
        private readonly ReactiveProperty<string> _jstTimeDisplay;
        
        public IReadOnlyReactiveProperty<string> LocalTimeDisplay => _localTimeDisplay;
        public IReadOnlyReactiveProperty<string> LocalDateDisplay => _localDateDisplay;
        public IReadOnlyReactiveProperty<string> UtcTimeDisplay => _utcTimeDisplay;
        public IReadOnlyReactiveProperty<string> JstTimeDisplay => _jstTimeDisplay;
        public IReadOnlyReactiveProperty<bool> IsSynchronized => _clockUseCase.IsSynchronized;
        
        [Inject]
        public ClockViewModel(ClockUseCase clockUseCase)
        {
            _clockUseCase = clockUseCase;
            _disposables = new CompositeDisposable();
            
            _localTimeDisplay = new ReactiveProperty<string>("");
            _localDateDisplay = new ReactiveProperty<string>("");
            _utcTimeDisplay = new ReactiveProperty<string>("");
            _jstTimeDisplay = new ReactiveProperty<string>("");
            
            SubscribeToTimeUpdates();
            StartClock();
        }
        
        public void StartClock()
        {
            _clockUseCase.StartClock();
        }
        
        private void SubscribeToTimeUpdates()
        {
            _clockUseCase.CurrentTime
                .Subscribe(time =>
                {
                    _localTimeDisplay.Value = time.ToString("HH:mm:ss");
                    _localDateDisplay.Value = time.ToString("yyyy-MM-dd dddd");
                })
                .AddTo(_disposables);
            
            _clockUseCase.UtcTime
                .Subscribe(time =>
                {
                    _utcTimeDisplay.Value = $"UTC: {time:HH:mm:ss}";
                })
                .AddTo(_disposables);
            
            _clockUseCase.JstTime
                .Subscribe(time =>
                {
                    _jstTimeDisplay.Value = $"JST: {time:HH:mm:ss}";
                })
                .AddTo(_disposables);
        }
        
        public void RefreshTime()
        {
            _clockUseCase.ForceSync();
        }
        
        public void Dispose()
        {
            _disposables?.Dispose();
            _localTimeDisplay?.Dispose();
            _localDateDisplay?.Dispose();
            _utcTimeDisplay?.Dispose();
            _jstTimeDisplay?.Dispose();
        }
    }
}