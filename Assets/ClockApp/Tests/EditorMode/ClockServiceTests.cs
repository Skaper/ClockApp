using ClockApp.Domain.Clock;
using ClockApp.Scripts.Domain.Common;
using NUnit.Framework;

namespace ClockApp.Tests.EditorMode
{
    public class ClockServiceTests
    {
        private ClockService _clockService;

        [SetUp]
        public void SetUp()
        {
            var timeProvider = new SystemTimeProvider();
            _clockService = new ClockService(timeProvider);
        }

        [Test]
        public void InitialClockSynchronizationTest()
        {
            Assert.IsNotNull(_clockService.CurrentTime.Value);
            Assert.IsNotNull(_clockService.UtcTime.Value);
            Assert.IsNotNull(_clockService.JstTime.Value);
            Assert.IsFalse(_clockService.IsSynchronized.Value);
        }

        [TearDown]
        public void TearDown()
        {
            _clockService.Dispose();
        }
    }
}