using System;
using ClockApp.Domain.Timer;
using NUnit.Framework;

namespace ClockApp.Tests.EditorMode
{
    public class TimerServiceTests
    {
        private TimerService _timerService;

        [SetUp]
        public void SetUp()
        {
            _timerService = new TimerService();
        }

        [Test]
        public void TimerInitialIdleStateTest()
        {
            Assert.AreEqual(TimerState.Idle, _timerService.State.Value);
        }

        [Test]
        public void TimerSetDurationTest()
        {
            var duration = TimeSpan.FromMinutes(1);
            _timerService.SetDuration(duration);

            Assert.AreEqual(duration, _timerService.RemainingTime.Value);
        }

        [Test]
        public void TimerStartChangesStateToRunning()
        {
            _timerService.SetDuration(TimeSpan.FromSeconds(10));
            _timerService.Start();

            Assert.AreEqual(TimerState.Running, _timerService.State.Value);
        }

        [Test]
        public void TimerPauseChangesStateToPaused()
        {
            _timerService.SetDuration(TimeSpan.FromSeconds(10));
            _timerService.Start();
            _timerService.Pause();

            Assert.AreEqual(TimerState.Paused, _timerService.State.Value);
        }

        [Test]
        public void TimerResetReturnsToIdle()
        {
            _timerService.SetDuration(TimeSpan.FromSeconds(10));
            _timerService.Start();
            _timerService.Reset();

            Assert.AreEqual(TimerState.Idle, _timerService.State.Value);
            Assert.AreEqual(TimeSpan.Zero, _timerService.RemainingTime.Value);
        }

        [TearDown]
        public void TearDown()
        {
            _timerService.Dispose();
        }
    }
}