using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using VContainer;
using ClockApp.Domain.Clock;
using ClockApp.Domain.Timer;
using ClockApp.Domain.Stopwatch;

namespace ClockApp.Application.Integration
{
    [ApiVersion("1.0.0")]
    public class ClockApplicationAPI : ITimeAPI, ITimerAPI, IStopwatchAPI
    {
        private readonly IClockService _clockService;
        private readonly ITimerService _timerService;
        private readonly IStopwatchService _stopwatchService;
        
        [Inject]
        public ClockApplicationAPI(
            IClockService clockService,
            ITimerService timerService,
            IStopwatchService stopwatchService)
        {
            _clockService = clockService;
            _timerService = timerService;
            _stopwatchService = stopwatchService;
        }
        
        #region Clock API
        
        public DateTime GetCurrentTime() => _clockService.CurrentTime.Value;
        public DateTime GetUtcTime() => _clockService.UtcTime.Value;
        public DateTime GetJstTime() => _clockService.JstTime.Value;
        public bool IsClockSynchronized() => _clockService.IsSynchronized.Value;
        
        #endregion
        
        #region Timer API
        
        public void StartTimer(TimeSpan duration)
        {
            _timerService.SetDuration(duration);
            _timerService.Start();
        }
        
        public void PauseTimer() => _timerService.Pause();
        public void StopTimer() => _timerService.Stop();
        public void ResetTimer() => _timerService.Reset();
        public TimeSpan GetTimerRemainingTime() => _timerService.RemainingTime.Value;
        public TimerState GetTimerState() => _timerService.State.Value;
        
        public IObservable<Unit> OnTimerCompleted => _timerService.OnTimerCompleted;
        public IObservable<TimeSpan> TimerRemainingTime => _timerService.RemainingTime;
        public IObservable<TimerState> TimerState => _timerService.State;
        
        #endregion
        
        #region Stopwatch API
        
        public void StartStopwatch() => _stopwatchService.Start();
        public void StopStopwatch() => _stopwatchService.Stop();
        public void ResetStopwatch() => _stopwatchService.Reset();
        public void RecordLap() => _stopwatchService.RecordLap();
        public TimeSpan GetStopwatchElapsedTime() => _stopwatchService.ElapsedTime.Value;
        public IReadOnlyList<LapTime> GetLapTimes() => _stopwatchService.LapTimes.ToList();
        public bool IsStopwatchRunning() => _stopwatchService.IsRunning.Value;
        
        public IObservable<TimeSpan> StopwatchElapsedTime => _stopwatchService.ElapsedTime;
        public IObservable<bool> StopwatchRunning => _stopwatchService.IsRunning;
        
        #endregion
    }
}