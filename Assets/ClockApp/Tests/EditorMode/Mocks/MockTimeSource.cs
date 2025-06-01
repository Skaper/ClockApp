namespace ClockApp.Domain.Stopwatch
{
    public class MockTimeSource : ITimeSource
    {
        public float CurrentTime { get; set; } = 0f;

        public float GetTime() => CurrentTime;

        public void Advance(float seconds) => CurrentTime += seconds;
    }
}