using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UniRx;
using VContainer;
using DG.Tweening;
using ClockApp.Presentation.ViewModels;
using ClockApp.Domain.Timer;

namespace ClockApp.Presentation.Views
{
    public class TimerView : BaseView
    {
        [Header("Time Display")]
        [SerializeField] private TextMeshProUGUI timeDisplay;
        
        [Header("Input Fields")]
        [SerializeField] private TMP_InputField hoursInput;
        [SerializeField] private TMP_InputField minutesInput;
        [SerializeField] private TMP_InputField secondsInput;
        [SerializeField] private CanvasGroup inputGroup;
        
        [Header("Control Buttons")]
        [SerializeField] private Button controlButton;
        [SerializeField] private Button resetButton;

        [Header("Icon Sprites")]
        [SerializeField] private Sprite playIcon;
        [SerializeField] private Sprite pauseIcon;
        [SerializeField] private Sprite resetIcon;
        [SerializeField] private Sprite stopIcon;

        private Image _controlButtonIcon;
        private Image _resetButtonIcon;
        private TimerViewModel _viewModel;
        private Sequence _pulseAnimation;
        
        [Inject]
        public void Construct(TimerViewModel viewModel)
        {
            _viewModel = viewModel;
        }
        
        protected override void Awake()
        {
            base.Awake();
            _controlButtonIcon = controlButton.GetComponent<Image>();
            _resetButtonIcon = resetButton.GetComponent<Image>();
        }
        
        protected override void Initialize()
        {
            if (_viewModel == null) return;
            base.Initialize();
        }
        
        protected override void SetupUI()
        {
            inputGroup.alpha = 1f;
            UpdateButtonIcons(TimerState.Idle);
        }
        
        protected override void BindViewModel()
        {
            _viewModel.TimeDisplay
                .Subscribe(text => timeDisplay.text = text)
                .AddTo(disposables);
            
            BindInputField(hoursInput, _viewModel.Hours, 99);
            BindInputField(minutesInput, _viewModel.Minutes, 59);
            BindInputField(secondsInput, _viewModel.Seconds, 59);
            
            var timerStateObservable = _viewModel.Hours.CombineLatest(_viewModel.Minutes,
                _viewModel.Seconds,
                _viewModel.State,
                (h, m, s, state) => new 
                { 
                    HasTime = h > 0 || m > 0 || s > 0,
                    State = state,
                    IsActive = state is TimerState.Running or TimerState.Paused
                });
            
            controlButton.onClick.AsObservable()
                .Subscribe(_ => HandleControlButtonClick())
                .AddTo(disposables);
            
            timerStateObservable
                .Subscribe(data => controlButton.interactable = data.HasTime || data.IsActive)
                .AddTo(disposables);
            
            resetButton.onClick.AsObservable()
                .Subscribe(_ => HandleResetButtonClick())
                .AddTo(disposables);
            
            timerStateObservable
                .Subscribe(data => resetButton.interactable = data.HasTime || data.IsActive)
                .AddTo(disposables);
            
            _viewModel.State
                .Subscribe(state =>
                {
                    UpdateUIForState(state);
                    UpdateButtonIcons(state);
                })
                .AddTo(disposables);
            
            _viewModel.State
                .Where(state => state == TimerState.Completed)
                .Subscribe(_ => 
                {
                    animationController.AnimateTextCompletion(timeDisplay);
                    animationController.AnimateShake(transform);
                    RefreshInputFields();
                })
                .AddTo(disposables);
        }
        
        private void HandleControlButtonClick()
        {
            var state = _viewModel.State.Value;
            
            switch (state)
            {
                case TimerState.Idle:
                case TimerState.Completed:
                    if (HasTime())
                    {
                        SyncInputValuesToViewModel();
                        _viewModel.StartTimer();
                        animationController.AnimateButtonPress(controlButton);
                    }
                    break;
                    
                case TimerState.Running:
                    _viewModel.PauseTimer();
                    animationController.AnimateButtonPress(controlButton);
                    StartPulseAnimation();
                    break;
                    
                case TimerState.Paused:
                    _viewModel.StartTimer();
                    animationController.AnimateButtonPress(controlButton);
                    StopPulseAnimation();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        private void HandleResetButtonClick()
        {
            var state = _viewModel.State.Value;
            
            ResetInputValues();
            
            if (state is TimerState.Running or TimerState.Paused)
            {
                _viewModel.StopTimer();
                _viewModel.ResetTimer();
            }
            
            animationController.AnimateButtonPress(resetButton, true);
            StopPulseAnimation();
            animationController.ResetTextAppearance(timeDisplay);
        }
        
        private void ResetInputValues()
        {
            _viewModel.Hours.Value = 0;
            _viewModel.Minutes.Value = 0;
            _viewModel.Seconds.Value = 0;
        }
        
        private void SyncInputValuesToViewModel()
        {
            var hours = int.TryParse(hoursInput.text, out var h) ? Mathf.Clamp(h, 0, 99) : 0;
            var minutes = int.TryParse(minutesInput.text, out var m) ? Mathf.Clamp(m, 0, 59) : 0;
            var seconds = int.TryParse(secondsInput.text, out var s) ? Mathf.Clamp(s, 0, 59) : 0;
            
            _viewModel.Hours.Value = hours;
            _viewModel.Minutes.Value = minutes;
            _viewModel.Seconds.Value = seconds;
        }
        
        private void RefreshInputFields()
        {
            hoursInput.text = _viewModel.Hours.Value.ToString("D2");
            minutesInput.text = _viewModel.Minutes.Value.ToString("D2");
            secondsInput.text = _viewModel.Seconds.Value.ToString("D2");
        }
        
        private void UpdateButtonIcons(TimerState state)
        {
            if (_controlButtonIcon)
                _controlButtonIcon.sprite = state == TimerState.Running ? pauseIcon : playIcon;
            
            if (_resetButtonIcon)
                _resetButtonIcon.sprite = state is TimerState.Running or TimerState.Paused ? stopIcon : resetIcon;
        }
        
        private void BindInputField(TMP_InputField input, IReactiveProperty<int> property, int maxValue)
        {
            input.onEndEdit.AsObservable()
                .Where(_ => _viewModel.State.Value is TimerState.Idle or TimerState.Completed)
                .Select(text => int.TryParse(text, out var value) ? Mathf.Clamp(value, 0, maxValue) : 0)
                .Subscribe(value => 
                {
                    if (property.Value != value)
                    {
                        property.Value = value;
                    }
                })
                .AddTo(disposables);
            
            property
                .Subscribe(value => 
                {
                    var formattedValue = value.ToString("D2");
                    if (input.text != formattedValue)
                    {
                        input.text = formattedValue;
                    }
                })
                .AddTo(disposables);
        }
        
        private void UpdateUIForState(TimerState state)
        {
            var showInput = state is TimerState.Idle or TimerState.Completed;
            animationController.AnimateInputGroupVisibility(inputGroup, showInput);
            
            if (state is TimerState.Completed or TimerState.Idle)
            {
                animationController.ResetTextAppearance(timeDisplay);
                DOVirtual.DelayedCall(0.1f, RefreshInputFields);
            }
        }
        
        private void StartPulseAnimation()
        {
            StopPulseAnimation();
            _pulseAnimation = animationController.CreatePulseAnimation(timeDisplay.transform);
        }
        
        private void StopPulseAnimation()
        {
            _pulseAnimation?.Kill();
            _pulseAnimation = null;
        }
        
        private bool HasTime()
        {
            return _viewModel.Hours.Value > 0 || 
                   _viewModel.Minutes.Value > 0 || 
                   _viewModel.Seconds.Value > 0;
        }
        
        protected override void OnDestroy()
        {
            base.OnDestroy();
            StopPulseAnimation();
            
            animationController.KillTweens(timeDisplay);
            animationController.KillTweens(inputGroup);
        }
    }
}