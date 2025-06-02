using NUnit.Framework;
using UnityEngine.TestTools;
using UnityEngine;
using System.Collections;
using ClockApp.Domain.Clock;
using ClockApp.Domain.Timer;
using ClockApp.Domain.Stopwatch;
using UniRx;
using System;
using ClockApp.Scripts.Domain.Common;

namespace ClockApp.Tests.PlayMode
{
    public class IntegrationTests
    {
        private ClockService _clockService;
        private TimerService _timerService;
        private StopwatchService _stopwatchService;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            var timeProvider = new SystemTimeProvider();
            var unityTimeSource = new UnityTimeSource();

            _clockService = new ClockService(timeProvider);
            _timerService = new TimerService();
            _stopwatchService = new StopwatchService(unityTimeSource);

            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            _clockService.Dispose();
            _timerService.Dispose();
            _stopwatchService.Dispose();
            yield return null;
        }

        [UnityTest]
        public IEnumerator ClockService_SynchronizesSuccessfully()
        {
            var isSync = false;
            _clockService.IsSynchronized.Subscribe(sync => isSync = sync);
            _clockService.ForceSync();
            var timer = 0f;
            while (!isSync && timer < 5f)
            {
                timer += Time.deltaTime;
                yield return null;
            }
            Assert.IsTrue(isSync);
        }

        [UnityTest]
        public IEnumerator TimerService_CompletesAfterSetDuration()
        {
            _timerService.SetDuration(TimeSpan.FromSeconds(1));
            _timerService.Start();

            var completed = false;
            _timerService.OnTimerCompleted.Subscribe(_ => completed = true);

            var timer = 0f;
            while ((!completed || _timerService.State.Value != TimerState.Completed) && timer < 5f)
            {
                timer += Time.deltaTime;
                yield return null;
            }
            Assert.AreEqual(TimerState.Completed, _timerService.State.Value);
            Assert.IsTrue(completed);
        }

        [UnityTest]
        public IEnumerator TimerService_PausesCorrectly()
        {
            _timerService.SetDuration(TimeSpan.FromSeconds(3));
            _timerService.Start();

            var timer = 0f;
            while (timer < 1f)
            {
                timer += Time.deltaTime;
                yield return null;
            }
            _timerService.Pause();

            var timeAfterPause = _timerService.RemainingTime.Value;
            var pauseTimer = 0f;
            while (pauseTimer < 1f)
            {
                pauseTimer += Time.deltaTime;
                yield return null;
            }

            Assert.AreEqual(TimerState.Paused, _timerService.State.Value);
            Assert.AreEqual(timeAfterPause.Seconds, _timerService.RemainingTime.Value.Seconds);
        }

        [UnityTest]
        public IEnumerator StopwatchService_RecordsLapTimesCorrectly()
        {
            _stopwatchService.Start();

            var timer = 0f;
            while (timer < 0.55f)
            {
                timer += Time.deltaTime;
                yield return null;
            }
            _stopwatchService.RecordLap();

            timer = 0f;
            while (timer < 0.55f)
            {
                timer += Time.deltaTime;
                yield return null;
            }
            _stopwatchService.RecordLap();

            Assert.AreEqual(2, _stopwatchService.LapTimes.Count);
            Assert.IsTrue(_stopwatchService.LapTimes[0].Time.TotalSeconds >= 0.5);
            Assert.IsTrue(_stopwatchService.LapTimes[1].Time.TotalSeconds >= 0.5);
            yield return null;
        }

        [UnityTest]
        public IEnumerator StopwatchService_StopsCorrectly()
        {
            _stopwatchService.Start();

            var timer = 0f;
            while (timer < 1f)
            {
                timer += Time.deltaTime;
                yield return null;
            }
            _stopwatchService.Stop();

            var timeAtStop = _stopwatchService.ElapsedTime.Value;

            var waitTimer = 0f;
            while (waitTimer < 1f)
            {
                waitTimer += Time.deltaTime;
                yield return null;
            }

            Assert.IsFalse(_stopwatchService.IsRunning.Value);
            Assert.AreEqual(timeAtStop.Seconds, _stopwatchService.ElapsedTime.Value.Seconds);
        }

        [UnityTest]
        public IEnumerator ClockService_UpdatesTimeEverySecond()
        {
            DateTime initialTime = _clockService.CurrentTime.Value;
            var timer = 0f;
            while (((_clockService.CurrentTime.Value - initialTime).TotalSeconds < 1) && timer < 5f)
            {
                timer += Time.deltaTime;
                yield return null;
            }
            DateTime updatedTime = _clockService.CurrentTime.Value;
            Assert.IsTrue((updatedTime - initialTime).TotalSeconds >= 1);
        }

        [UnityTest]
        public IEnumerator TimerService_ResetToInitialState()
        {
            _timerService.SetDuration(TimeSpan.FromSeconds(2));
            _timerService.Start();

            var timer = 0f;
            while (timer < 1f)
            {
                timer += Time.deltaTime;
                yield return null;
            }
            _timerService.Reset();

            Assert.AreEqual(TimerState.Idle, _timerService.State.Value);
            Assert.AreEqual(TimeSpan.Zero, _timerService.RemainingTime.Value);
            yield return null;
        }

        [UnityTest]
        public IEnumerator StopwatchService_ResetClearsElapsedAndLaps()
        {
            _stopwatchService.Start();
            var timer = 0f;
            while (timer < 0.5f)
            {
                timer += Time.deltaTime;
                yield return null;
            }
            _stopwatchService.RecordLap();
            _stopwatchService.Reset();

            Assert.IsFalse(_stopwatchService.IsRunning.Value);
            Assert.AreEqual(0, _stopwatchService.LapTimes.Count);
            Assert.AreEqual(TimeSpan.Zero, _stopwatchService.ElapsedTime.Value);
            yield return null;
        }
    }
}
