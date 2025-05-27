using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UniRx;
using VContainer;
using DG.Tweening;
using ClockApp.Presentation.ViewModels;

namespace ClockApp.Presentation.Views
{
    public class StopwatchView : BaseView
    {
        
        [Header("Time Display")]
        [SerializeField] private TextMeshProUGUI timeDisplay;
        [SerializeField] private TextMeshProUGUI millisecondsDisplay;
        
        [Header("Control Buttons")]
        [SerializeField] private Button startStopButton;
        [SerializeField] private Button lapResetButton;

        [Header("Button Icons")]
        [SerializeField] private Sprite playIcon;
        [SerializeField] private Sprite pauseIcon;
        [SerializeField] private Sprite resetIcon;
        [SerializeField] private Sprite lapIcon;
        
        [Header("Lap Time Display")]
        [SerializeField] private ScrollRect lapScrollRect;
        [SerializeField] private Transform lapContainer;
        [SerializeField] private GameObject lapItemPrefab;

        private Image _startStopButtonImage;
        private Image _lapResetButtonImage;
        private StopwatchViewModel _viewModel;
        
        [Inject]
        public void Construct(StopwatchViewModel viewModel)
        {
            _viewModel = viewModel;
        }
        
        protected override void Awake()
        {
            base.Awake();
            _startStopButtonImage = startStopButton.GetComponent<Image>();
            _lapResetButtonImage = lapResetButton.GetComponent<Image>();
        }
        
        protected override void Initialize()
        {
            if (_viewModel == null) return;
            base.Initialize();
        }
        
        protected override void SetupUI()
        {
            mainPanel.alpha = 0f;
            UpdateButtonIcons(false);
            
            foreach (Transform child in lapContainer)
            {
                Destroy(child.gameObject);
            }
        }
        
        protected override void BindViewModel()
        {
            if (timeDisplay != null && _viewModel.TimeDisplay != null)
            {
                _viewModel.TimeDisplay
                    .Subscribe(time =>
                    {
                        if (timeDisplay == null) return;
                        
                        var parts = time.Split('.');
                        if (parts.Length == 2)
                        {
                            timeDisplay.text = parts[0];
                            if (millisecondsDisplay != null)
                                millisecondsDisplay.text = $".{parts[1]}";
                        }
                        else
                        {
                            timeDisplay.text = time;
                        }
                    })
                    .AddTo(disposables);
            }

            _viewModel.IsRunning?.Subscribe(UpdateButtonIcons).AddTo(disposables);

            startStopButton.onClick.AsObservable()
                .Subscribe(_ =>
                {
                    _viewModel?.ToggleStartStop();
                    HandleStartStopClick();
                })
                .AddTo(disposables);
            
            lapResetButton.onClick.AsObservable()
                .Subscribe(_ =>
                {
                    _viewModel?.LapOrReset();
                    HandleLapResetClick();
                })
                .AddTo(disposables);
            
            _viewModel.LapDisplayList
                .ObserveAdd()
                .Subscribe(x => CreateLapItem(x.Value))
                .AddTo(disposables);
                
            _viewModel.LapDisplayList
                .ObserveReset()
                .Subscribe(_ => ClearLapItems())
                .AddTo(disposables);
        }
        
        private void UpdateButtonIcons(bool isRunning)
        {
            _startStopButtonImage.sprite = isRunning ? pauseIcon : playIcon;
            _lapResetButtonImage.sprite = isRunning ? lapIcon : resetIcon;
        }
        
        private void HandleStartStopClick()
        {
            animationController.AnimateButtonPress(startStopButton);
            animationController.AnimateButtonIcon(_startStopButtonImage);
            
            if (_viewModel?.IsRunning?.Value == true)
            {
                animationController.AnimatePositionPunch(timeDisplay.transform, Vector3.up);
            }
            else
            {
                animationController.AnimateShake(timeDisplay.transform);
            }
        }
        
        private void HandleLapResetClick()
        {
            animationController.AnimateButtonPress(lapResetButton);
            animationController.AnimateButtonIcon(_lapResetButtonImage);
        }
        
        private void CreateLapItem(string lapText)
        {
            if (lapContainer == null) return;
            
            var lapItem = Instantiate(lapItemPrefab, lapContainer);
            var text = lapItem.GetComponentInChildren<TextMeshProUGUI>();
            text.text = lapText;
            
            animationController.AnimateListItemAppear(lapItem);
            
            DOVirtual.DelayedCall(0.1f, () =>
            {
                if (lapScrollRect != null)
                    lapScrollRect.verticalNormalizedPosition = 0f;
            });
        }
        
        private void ClearLapItems()
        {
            if (lapContainer == null) return;
            
            foreach (Transform child in lapContainer)
            {
                var childTransform = child;
                animationController.AnimateListItemDisappear(child, () =>
                {
                    if (childTransform != null)
                        Destroy(childTransform.gameObject);
                });
            }
        }
        
        protected override void OnDestroy()
        {
            base.OnDestroy();
            animationController.KillTweens(timeDisplay);
            animationController.KillTweens(_startStopButtonImage);
            animationController.KillTweens(_lapResetButtonImage);
        }
    }
}