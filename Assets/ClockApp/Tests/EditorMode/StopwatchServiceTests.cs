using NUnit.Framework;
using ClockApp.Domain.Stopwatch;
using System;

namespace ClockApp.Tests.EditorMode
{
    public class StopwatchServiceTests
    {
        private StopwatchService _stopwatchService;
        private MockTimeSource _mockTimeSource;

        [SetUp]
        public void SetUp()
        {
            _mockTimeSource = new MockTimeSource();
            _stopwatchService = new StopwatchService(_mockTimeSource);
        }

        [Test]
        public void StopwatchInitialNotRunningTest()
        {
            Assert.IsFalse(_stopwatchService.IsRunning.Value);
            Assert.AreEqual(TimeSpan.Zero, _stopwatchService.ElapsedTime.Value);
        }

        [Test]
        public void StopwatchStartTest()
        {
            _stopwatchService.Start();

            Assert.IsTrue(_stopwatchService.IsRunning.Value);
        }

        [Test]
        public void StopwatchStopTest()
        {
            _stopwatchService.Start();
            _stopwatchService.Stop();

            Assert.IsFalse(_stopwatchService.IsRunning.Value);
        }

        [Test]
        public void StopwatchLapRecordingTest()
        {
            _stopwatchService.Start();

            _mockTimeSource.Advance(0.1f);
            _stopwatchService.ManualUpdateElapsedTime(); 

            _stopwatchService.RecordLap();

            Assert.AreEqual(1, _stopwatchService.LapTimes.Count);
            Assert.AreEqual(1, _stopwatchService.LapTimes[0].Index);
            Assert.AreEqual(0.1f, _stopwatchService.LapTimes[0].TotalTime.TotalSeconds, 0.001f);
        }

        [Test]
        public void StopwatchResetClearsLapsAndElapsedTime()
        {
            _stopwatchService.Start();
            _stopwatchService.RecordLap();
            _stopwatchService.Reset();

            Assert.AreEqual(0, _stopwatchService.LapTimes.Count);
            Assert.AreEqual(TimeSpan.Zero, _stopwatchService.ElapsedTime.Value);
            Assert.IsFalse(_stopwatchService.IsRunning.Value);
        }

        [TearDown]
        public void TearDown()
        {
            _stopwatchService.Dispose();
        }
    }
}