﻿using UnityEngine;

namespace ClockApp.Scripts.Infrastructure.Logging
{
    public class UnityAppLogger : IAppLogger
    {
        public void Log(string message) => Debug.Log(message);

        public void LogWarning(string message) => Debug.LogWarning(message);
        
        public void LogError(string message) => Debug.LogError(message);
    }
}