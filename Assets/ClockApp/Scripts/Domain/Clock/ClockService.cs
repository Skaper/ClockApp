using System;
using UniRx;
using UnityEngine;
using VContainer;

namespace ClockApp.Domain.Clock
{
    /// <summary>
    /// Clock service that manages time display and synchronization
    /// Uses ITimeProvider for actual time source
    /// </summary>
    public class ClockService : IClockService, IDisposable
    {
        private readonly ITimeProvider _timeProvider;
        
        private readonly ReactiveProperty<DateTime> _currentTime;
        private readonly ReactiveProperty<DateTime> _utcTime;
        private readonly ReactiveProperty<DateTime> _jstTime;
        private readonly ReactiveProperty<bool> _isSynchronized;
        
        private readonly CompositeDisposable _disposables;
        private IDisposable _updateSubscription;
        private IDisposable _syncSubscription;
        
        private const int SyncIntervalSeconds = 300; // 5 minutes
        
        public IReadOnlyReactiveProperty<DateTime> CurrentTime => _currentTime;
        public IReadOnlyReactiveProperty<DateTime> UtcTime => _utcTime;
        public IReadOnlyReactiveProperty<DateTime> JstTime => _jstTime;
        public IReadOnlyReactiveProperty<bool> IsSynchronized => _isSynchronized;
        
        [Inject]
        public ClockService(ITimeProvider timeProvider)
        {
            _timeProvider = timeProvider;
            
            _currentTime = new ReactiveProperty<DateTime>(DateTime.Now);
            _utcTime = new ReactiveProperty<DateTime>(DateTime.UtcNow);
            _jstTime = new ReactiveProperty<DateTime>(ConvertToJst(DateTime.UtcNow));
            _isSynchronized = new ReactiveProperty<bool>(false);
            _disposables = new CompositeDisposable();
            
            StartSync();
        }
        
        public void StartSync()
        {
            StopSync();
            
            // Initial sync
            ForceSync();
            
            _updateSubscription = Observable.Interval(TimeSpan.FromSeconds(1))
                .Subscribe(_ => UpdateTimes())
                .AddTo(_disposables);
    
            _syncSubscription = Observable.Timer(
                    TimeSpan.FromSeconds(SyncIntervalSeconds), 
                    TimeSpan.FromSeconds(SyncIntervalSeconds))
                .Subscribe(_ => ForceSync())
                .AddTo(_disposables);
        }
        
        public void StopSync()
        {
            _updateSubscription?.Dispose();
            _syncSubscription?.Dispose();
        }
        
        public void ForceSync()
        {
            _timeProvider.GetNetworkTime()
                .ObserveOnMainThread()
                .Subscribe(
                    networkTime =>
                    {
                        _isSynchronized.Value = networkTime.HasValue;
                        
                        if (networkTime.HasValue)
                        {
                            Debug.Log($"Clock synchronized with network time");
                        }
                        else
                        {
                            Debug.LogWarning("Failed to sync with network time, using system time");
                        }
                        
                        UpdateTimes();
                    },
                    error =>
                    {
                        Debug.LogError($"Clock sync error: {error.Message}");
                        _isSynchronized.Value = false;
                    })
                .AddTo(_disposables);
        }
        
        private void UpdateTimes()
        {
            var utcNow = _timeProvider.GetUtcTime();
            var localNow = _timeProvider.GetCurrentTime();
            
            _utcTime.Value = utcNow;
            _currentTime.Value = localNow;
            _jstTime.Value = ConvertToJst(utcNow);
        }
        
        private DateTime ConvertToJst(DateTime utcTime)
        {
            try
            {
                var jstZone = TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time");
                return TimeZoneInfo.ConvertTimeFromUtc(utcTime, jstZone);
            }
            catch
            {
                try
                {
                    var jstZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Tokyo");
                    return TimeZoneInfo.ConvertTimeFromUtc(utcTime, jstZone);
                }
                catch
                {
                    // Manual offset as last resort
                    return utcTime.AddHours(9);
                }
            }
        }
        
        public void Dispose()
        {
            StopSync();
            _disposables?.Dispose();
            _currentTime?.Dispose();
            _utcTime?.Dispose();
            _jstTime?.Dispose();
            _isSynchronized?.Dispose();
        }
    }
}