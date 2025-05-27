using System;

namespace ClockApp.Application.Integration
{
    public interface ITimeAPI
    {
        DateTime GetCurrentTime();
        DateTime GetUtcTime();
        DateTime GetJstTime();
        bool IsClockSynchronized();
    }
}