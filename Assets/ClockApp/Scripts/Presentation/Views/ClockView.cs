using System;
using UnityEngine;
using TMPro;
using UniRx;
using VContainer;
using ClockApp.Presentation.ViewModels;

namespace ClockApp.Presentation.Views
{
    public class ClockView : BaseView
    {
        [Header("Time Displays")]
        [SerializeField] private TextMeshProUGUI localTimeText;
        [SerializeField] private TextMeshProUGUI localDateText;
        [SerializeField] private TextMeshProUGUI utcTimeText;
        [SerializeField] private TextMeshProUGUI jstTimeText;
        
        private ClockViewModel _viewModel;
        
        [Inject]
        public void Construct(ClockViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        protected override void Initialize()
        {
            if (_viewModel == null) return;
            base.Initialize();
        }
        
        protected override void SetupUI()
        {
            mainPanel.alpha = 0f;
            _viewModel.RefreshTime();
        }
        
        protected override void BindViewModel()
        {
            _viewModel.LocalTimeDisplay
                .SubscribeToText(localTimeText)
                .AddTo(disposables);
            
            _viewModel.LocalDateDisplay
                .SubscribeToText(localDateText)
                .AddTo(disposables);
            
            _viewModel.UtcTimeDisplay
                .SubscribeToText(utcTimeText)
                .AddTo(disposables);
            
            _viewModel.JstTimeDisplay
                .SubscribeToText(jstTimeText)
                .AddTo(disposables);
            
            _viewModel.LocalTimeDisplay
                .Skip(1)
                .Subscribe(_ => animationController.AnimateTextTick(localTimeText))
                .AddTo(disposables);
        }
        
        protected override void OnDestroy()
        {
            base.OnDestroy();
            animationController.KillTweens(localTimeText);
        }
    }
}