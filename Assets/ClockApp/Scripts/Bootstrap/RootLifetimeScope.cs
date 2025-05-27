using ClockApp.Application.UseCases;
using ClockApp.Domain.Clock;
using ClockApp.Domain.Stopwatch;
using ClockApp.Domain.Timer;
using ClockApp.Infrastructure.Audio;
using ClockApp.Infrastructure.Background;
using ClockApp.Presentation.ViewModels;
using ClockApp.Scripts.Domain.Common;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace ClockApp.Bootstrap 
{
    public class RootLifetimeScope : LifetimeScope
    {
        [Header("Initialization Settings")] 
        [SerializeField] private AudioManager audioManager;
        [SerializeField] private BackgroundTaskManager backgroundTaskManager;

        protected override void Configure(IContainerBuilder builder)
        {
            RegisterInfrastructure(builder);
            RegisterDomainServices(builder);
            RegisterUseCases(builder);
            RegisterManagers(builder);
            RegisterViewModels(builder);
        }
        
        private void RegisterInfrastructure(IContainerBuilder builder)
        {
            builder.Register<ITimeProvider, SystemTimeProvider>(Lifetime.Singleton);
        }
        
        private void RegisterDomainServices(IContainerBuilder builder)
        {
            builder.Register<IClockService, ClockService>(Lifetime.Singleton);
            builder.Register<ITimerService, TimerService>(Lifetime.Singleton);
            builder.Register<IStopwatchService, StopwatchService>(Lifetime.Singleton);
        }
        
        private void RegisterUseCases(IContainerBuilder builder)
        {
            builder.Register<ClockUseCase>(Lifetime.Singleton);
            builder.Register<TimerUseCase>(Lifetime.Singleton);
            builder.Register<StopwatchUseCase>(Lifetime.Singleton);
        }
        
        private void RegisterManagers(IContainerBuilder builder)
        {
            builder.Register(_ => audioManager, Lifetime.Singleton);
            builder.Register(_ => backgroundTaskManager, Lifetime.Singleton);
        }
        
        private void RegisterViewModels(IContainerBuilder builder)
        {
            builder.Register<ClockViewModel>(Lifetime.Transient);
            builder.Register<TimerViewModel>(Lifetime.Transient);
            builder.Register<StopwatchViewModel>(Lifetime.Transient);
        }
    }
}