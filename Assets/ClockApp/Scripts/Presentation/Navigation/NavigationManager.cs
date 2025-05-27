using ClockApp.Presentation.Animations;
using ClockApp.Presentation.Views;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;

namespace ClockApp.Presentation.Navigation
{
    public class NavigationManager : MonoBehaviour
    {
        [Header("View Containers")]
        [SerializeField] private Transform viewContainer;
        
        [Header("View Prefabs")]
        [SerializeField] private GameObject clockViewPrefab;
        [SerializeField] private GameObject timerViewPrefab;
        [SerializeField] private GameObject stopwatchViewPrefab;
        
        [Header("Navigation Buttons")]
        [SerializeField] private Button clockButton;
        [SerializeField] private Button timerButton;
        [SerializeField] private Button stopwatchButton;
        
        [Header("Visual Elements")]
        [SerializeField] private TextMeshProUGUI titleText;
        
        [Header("Animation Settings")]
        [SerializeField] private AnimationSettings animationSettings;
        
        private BaseView _clockView;
        private BaseView _timerView;
        private BaseView _stopwatchView;
        private BaseView _currentView;
        
        private CompositeDisposable _disposables;
        private AnimationController _animationController;
        
        [Inject]
        private IObjectResolver _container;
        
        private void Start()
        {
            _disposables = new CompositeDisposable();
            _animationController = new AnimationController(animationSettings);
            
            CreateAllViews();
            SetupNavigation();
            ShowClock();
        }
        
        private void CreateAllViews()
        {
            _clockView = CreateView(clockViewPrefab).GetComponent<BaseView>();
            _timerView = CreateView(timerViewPrefab).GetComponent<BaseView>();
            _stopwatchView = CreateView(stopwatchViewPrefab).GetComponent<BaseView>();
            
            _clockView.Hide();
            _timerView.Hide();
            _stopwatchView.Hide();
        }
        
        private GameObject CreateView(GameObject prefab)
        {
            return _container.Instantiate(prefab, viewContainer);
        }
        
        private void SetupNavigation()
        {
            if (clockButton != null)
            {
                clockButton.onClick.AsObservable()
                    .Where(_ => _currentView != _clockView)
                    .Subscribe(_ => ShowClock())
                    .AddTo(_disposables);
            }
            
            if (timerButton != null)
            {
                timerButton.onClick.AsObservable()
                    .Where(_ => _currentView != _timerView)
                    .Subscribe(_ => ShowTimer())
                    .AddTo(_disposables);
            }
            
            if (stopwatchButton != null)
            {
                stopwatchButton.onClick.AsObservable()
                    .Where(_ => _currentView != _stopwatchView)
                    .Subscribe(_ => ShowStopwatch())
                    .AddTo(_disposables);
            }
        }
        
        public void ShowClock()
        {
            SwitchToView(_clockView, clockButton, "World Clock");
        }
        
        public void ShowTimer()
        {
            SwitchToView(_timerView, timerButton, "Timer");
        }
        
        public void ShowStopwatch()
        {
            SwitchToView(_stopwatchView, stopwatchButton, "Stopwatch");
        }
        
        private void SwitchToView(BaseView targetView, Button selectedButton, string title)
        {
            if (_currentView == targetView)
                return;
                
            _currentView?.Hide();
            _currentView = targetView;
            _currentView.Show();
            
            UpdateButtonStates(selectedButton);
            
            if (titleText != null)
                titleText.text = title;
        }
        
        private void UpdateButtonStates(Button selectedButton)
        {
            var buttons = new[] { clockButton, timerButton, stopwatchButton };
            
            foreach (var button in buttons)
            {
                if (button == null) continue;
                
                var isSelected = button == selectedButton;
                _animationController.AnimateNavigationButton(button, isSelected);
            }
        }
        
        private void OnDestroy()
        {
            _disposables?.Dispose();
        }
    }
}