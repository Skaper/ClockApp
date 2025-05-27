using System;
using UnityEngine;
using UniRx;
using ClockApp.Presentation.Animations;

namespace ClockApp.Presentation.Views
{
    public abstract class BaseView : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] protected AnimationSettings animationSettings;
        
        protected CompositeDisposable disposables;
        protected AnimationController animationController;
        protected CanvasGroup mainPanel;
        
        private readonly Subject<Unit> _showSubject = new Subject<Unit>();
        private readonly Subject<Unit> _hideSubject = new Subject<Unit>();
        
        public IObservable<Unit> OnShowRequested => _showSubject.AsObservable();
        public IObservable<Unit> OnHideRequested => _hideSubject.AsObservable();
        
        protected virtual void Awake()
        {
            disposables = new CompositeDisposable();
            mainPanel = GetComponent<CanvasGroup>();
            if (mainPanel == null)
            {
                mainPanel = gameObject.AddComponent<CanvasGroup>();
            }
            animationController = new AnimationController(animationSettings);
            
            SetupVisibilityHandlers();
        }
        
        protected virtual void Start()
        {
            Initialize();
        }
        
        private void SetupVisibilityHandlers()
        {
            OnShowRequested
                .Subscribe(_ => HandleShow())
                .AddTo(disposables);
                
            OnHideRequested
                .Subscribe(_ => HandleHide())
                .AddTo(disposables);
        }
        
        protected virtual void Initialize()
        {
            SetupUI();
            BindViewModel();
        }
        
        public void Show()
        {
            _showSubject.OnNext(Unit.Default);
        }
        
        public void Hide()
        {
            _hideSubject.OnNext(Unit.Default);
        }
        
        protected virtual void HandleShow()
        {
            gameObject.SetActive(true);
            animationController.AnimatePanelIn(mainPanel, transform);
        }
        
        protected virtual void HandleHide()
        {
            gameObject.SetActive(false);
        }
        
        protected abstract void SetupUI();
        protected abstract void BindViewModel();
        
        protected virtual void OnDestroy()
        {
            _showSubject?.OnCompleted();
            _hideSubject?.OnCompleted();
            disposables?.Dispose();
            animationController?.KillTweens(transform);
            animationController?.KillTweens(mainPanel);
        }
    }
}