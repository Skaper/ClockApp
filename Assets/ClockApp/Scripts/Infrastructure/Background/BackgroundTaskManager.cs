using System;
using System.Collections;
using ClockApp.Scripts.Domain.Events;
using UnityEngine;
using UniRx;

namespace ClockApp.Infrastructure.Background
{
    public class BackgroundTaskManager : MonoBehaviour
    {
        private bool _isApplicationPaused;
        private float _pauseStartTime;
        
        private void Awake()
        {
            UnityEngine.Application.runInBackground = true;
        }
        
        private void OnApplicationPause(bool pauseStatus)
        {
            _isApplicationPaused = pauseStatus;
            if (pauseStatus)
            {
                _pauseStartTime = Time.realtimeSinceStartup;
            }
            else
            {
                var pauseDuration = Time.realtimeSinceStartup - _pauseStartTime;
                MessageBroker.Default.Publish(new BackgroundResumeEvent { PauseDuration = pauseDuration });
            }
        }
        
        private void OnApplicationFocus(bool hasFocus)
        {
            MessageBroker.Default.Publish(new ApplicationFocusEvent { HasFocus = hasFocus });
        }
    }
}