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
            bool isSync = false;
            _clockService.IsSynchronized.Subscribe(sync => isSync = sync);
            _clockService.ForceSync();
            yield return new WaitForSeconds(1f);
            Assert.IsTrue(isSync);
        }

        [UnityTest]
        public IEnumerator TimerService_CompletesAfterSetDuration()
        {
            _timerService.SetDuration(TimeSpan.FromSeconds(1));
            _timerService.Start();

            bool completed = false;
            _timerService.OnTimerCompleted.Subscribe(_ => completed = true);

            yield return new WaitForSeconds(1.2f);

            Assert.AreEqual(TimerState.Completed, _timerService.State.Value);
            Assert.IsTrue(completed);
        }

        [UnityTest]
        public IEnumerator TimerService_PausesCorrectly()
        {
            _timerService.SetDuration(TimeSpan.FromSeconds(3));
            _timerService.Start();

            yield return new WaitForSeconds(1f);
            _timerService.Pause();

            var timeAfterPause = _timerService.RemainingTime.Value;
            yield return new WaitForSeconds(1f);

            Assert.AreEqual(TimerState.Paused, _timerService.State.Value);
            Assert.AreEqual(timeAfterPause.Seconds, _timerService.RemainingTime.Value.Seconds);
        }

        [UnityTest]
        public IEnumerator StopwatchService_RecordsLapTimesCorrectly()
        {
            _stopwatchService.Start();

            yield return new WaitForSeconds(0.5f);
            _stopwatchService.RecordLap();

            yield return new WaitForSeconds(0.5f);
            _stopwatchService.RecordLap();

            Assert.AreEqual(2, _stopwatchService.LapTimes.Count);

            Assert.IsTrue(_stopwatchService.LapTimes[0].Time.TotalMilliseconds >= 500);
            Assert.IsTrue(_stopwatchService.LapTimes[1].Time.TotalMilliseconds >= 500);
        }

        [UnityTest]
        public IEnumerator StopwatchService_StopsCorrectly()
        {
            _stopwatchService.Start();

            yield return new WaitForSeconds(1f);
            _stopwatchService.Stop();

            var timeAtStop = _stopwatchService.ElapsedTime.Value;

            yield return new WaitForSeconds(1f);

            Assert.IsFalse(_stopwatchService.IsRunning.Value);
            Assert.AreEqual(timeAtStop.Seconds, _stopwatchService.ElapsedTime.Value.Seconds);
        }

        [UnityTest]
        public IEnumerator ClockService_UpdatesTimeEverySecond()
        {
            DateTime initialTime = _clockService.CurrentTime.Value;

            yield return new WaitForSeconds(1.1f);

            DateTime updatedTime = _clockService.CurrentTime.Value;

            Assert.IsTrue((updatedTime - initialTime).TotalSeconds >= 1);
        }

        [UnityTest]
        public IEnumerator TimerService_ResetToInitialState()
        {
            _timerService.SetDuration(TimeSpan.FromSeconds(2));
            _timerService.Start();

            yield return new WaitForSeconds(1f);
            _timerService.Reset();

            Assert.AreEqual(TimerState.Idle, _timerService.State.Value);
            Assert.AreEqual(TimeSpan.Zero, _timerService.RemainingTime.Value);
        }

        [UnityTest]
        public IEnumerator StopwatchService_ResetClearsElapsedAndLaps()
        {
            _stopwatchService.Start();
            yield return new WaitForSeconds(0.5f);
            _stopwatchService.RecordLap();
            _stopwatchService.Reset();

            Assert.IsFalse(_stopwatchService.IsRunning.Value);
            Assert.AreEqual(0, _stopwatchService.LapTimes.Count);
            Assert.AreEqual(TimeSpan.Zero, _stopwatchService.ElapsedTime.Value);
        }
    }
}
