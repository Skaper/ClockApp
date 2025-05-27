namespace ClockApp.Scripts.Infrastructure.Logging
{
    public interface IAppLogger
    {
        void Log(string message);
        void LogWarning(string message);
        void LogError(string message);
    }
}