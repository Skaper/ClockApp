using System.Collections;
using ClockApp.Domain.Clock;
using ClockApp.Scripts.Domain.Common;
using NUnit.Framework;
using UniRx;
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

        [UnityEngine.TestTools.UnityTest]
        public IEnumerator ForceSyncUpdatesSynchronizedFlag()
        {
            bool isSync = false;
            _clockService.IsSynchronized.Skip(1).Subscribe(sync => isSync = sync);

            _clockService.ForceSync();

            // Ждём небольшое время, пока ForceSync не завершится
            yield return new UnityEngine.WaitForSeconds(1.2f);

            Assert.IsTrue(isSync);
        }

        [TearDown]
        public void TearDown()
        {
            _clockService.Dispose();
        }
    }
}