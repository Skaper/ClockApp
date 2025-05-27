using System;
using ClockApp.Bootstrap;
using DG.Tweening;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ClockApp.Scripts.Bootstrap
{
    public class InitSceneBootstrapper : MonoBehaviour
    {
        [Header("Scene Loading")]
        [SerializeField] private string mainSceneName = "MainScene";
        [SerializeField] private bool useAsyncLoading = true;
        
        private RootLifetimeScope _rootLifetimeScope;
        private CompositeDisposable _disposables;
        
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            _disposables = new CompositeDisposable();
            
            InitializeApplicationSettings();
            
            _rootLifetimeScope = FindObjectOfType<RootLifetimeScope>();
            if (_rootLifetimeScope == null)
            {
                Debug.LogError("[Init] RootLifetimeScope not found in Init scene!");
            }
        }
        
        private void Start()
        {
            InitializeApplicationReactive()
                .Subscribe(
                    _ => { },
                    error => Debug.LogError($"[Init] Error: {error}"),
                    () => DestroySelf()
                )
                .AddTo(_disposables);
        }
        
        private void InitializeApplicationSettings()
        {
            DOTween.Init(true, true, LogBehaviour.ErrorsOnly);
            DOTween.defaultEaseType = Ease.OutQuad;
            DOTween.defaultAutoPlay = AutoPlay.All;
            
            UnityEngine.Application.targetFrameRate = 60;
            UnityEngine.Application.runInBackground = true;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }
        
        private IObservable<Unit> InitializeApplicationReactive()
        {
            return Observable.Concat(
                LogStep("Initializing..."),
                WaitForDiContainer().ContinueWith(_ => LogStep("DI Container ready")),
                Observable.NextFrame().ContinueWith(_ => LogStep("Managers initialized")),
                LoadMainSceneReactive().ContinueWith(_ => LogStep("Scene loaded"))
            );
        }
        
        private IObservable<Unit> LogStep(string message)
        {
            Debug.Log($"[Init] {message}");
            return Observable.Return(Unit.Default);
        }
        
        private IObservable<Unit> WaitForDiContainer()
        {
            if (_rootLifetimeScope != null) return Observable.Return(Unit.Default);
            
            return Observable.EveryUpdate()
                .Where(_ => _rootLifetimeScope != null)
                .Take(1)
                .AsUnitObservable();
        }
        
        private IObservable<Unit> LoadMainSceneReactive()
        {
            if (!useAsyncLoading)
            {
                SceneManager.LoadScene(mainSceneName);
                return Observable.Return(Unit.Default);
            }
            
            return Observable.Create<Unit>(observer =>
            {
                var asyncOperation = SceneManager.LoadSceneAsync(mainSceneName, LoadSceneMode.Single);
                asyncOperation.allowSceneActivation = false;
                
                var progressSubscription = Observable.EveryUpdate()
                    .TakeWhile(_ => asyncOperation.progress < 0.9f)
                    .Subscribe();
                
                var completionSubscription = Observable.EveryUpdate()
                    .SkipWhile(_ => asyncOperation.progress < 0.9f)
                    .Take(1)
                    .Subscribe(_ =>
                    {
                        asyncOperation.allowSceneActivation = true;
                        
                        Observable.EveryUpdate()
                            .SkipWhile(_ => !asyncOperation.isDone)
                            .Take(1)
                            .Subscribe(__ =>
                            {
                                observer.OnNext(Unit.Default);
                                observer.OnCompleted();
                            });
                    });
                
                return new CompositeDisposable(progressSubscription, completionSubscription);
            });
        }
        
        private void DestroySelf()
        {
            Debug.Log("[Init] Initialization completed, destroying bootstrapper");
            _disposables?.Dispose();
            Destroy(this);
        }
        
        private void OnDestroy()
        {
            _disposables?.Dispose();
        }
    }
}