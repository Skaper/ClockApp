using UnityEngine;

namespace ClockApp.Domain.Stopwatch
{
    public class UnityTimeSource : ITimeSource
    {
        public float GetTime() => Time.realtimeSinceStartup;
    }
}