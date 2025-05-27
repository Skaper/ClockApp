using ClockApp.Presentation.Navigation;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace ClockApp.Bootstrap
{
    public class MainSceneLifetimeScope : LifetimeScope
    {
        [Header("Scene Components")]
        [SerializeField] private NavigationManager navigationManager;
        
        protected override void Configure(IContainerBuilder builder)
        {
            // Register NavigationManager
            builder.RegisterComponent(navigationManager);
        }
    }
}