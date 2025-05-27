using System;

public interface ITimeProvider
{
    IObservable<DateTime?> GetNetworkTime();
    DateTime GetCurrentTime();
    DateTime GetUtcTime();
}