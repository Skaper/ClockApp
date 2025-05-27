using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace ClockApp.Presentation.Animations
{
    public class AnimationController
    {
        private readonly AnimationSettings _settings;
        
        public AnimationController(AnimationSettings settings)
        {
            _settings = settings;
        }
        
        public void AnimatePanelIn(CanvasGroup panel, Transform transform)
        {
            panel.alpha = 0f;
            transform.localScale = Vector3.zero;
            
            panel.DOFade(1f, _settings.fadeInDuration);
            transform.DOScale(1f, _settings.scaleInDuration).SetEase(_settings.scaleInEase);
        }
        
        public void AnimateButtonPress(Button button, bool rotate = false)
        {
            if (rotate)
            {
                button.transform.DORotate(new Vector3(0, 0, -360), _settings.buttonRotationDuration, RotateMode.FastBeyond360);
            }
            else
            {
                button.transform.DOPunchScale(Vector3.one * _settings.buttonPunchScale, _settings.buttonPunchDuration);
            }
        }
        
        public void AnimateButtonIcon(Image buttonImage)
        {
            var sequence = DOTween.Sequence();
            sequence.Append(buttonImage.transform.DOScale(_settings.buttonScaleAmount, _settings.buttonScaleDuration));
            sequence.Append(buttonImage.transform.DOScale(1f, _settings.buttonScaleDuration));
        }
        
        public void AnimateNavigationButton(Button button, bool isSelected)
        {
            var buttonImage = button.GetComponent<Image>();
            var buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
            
            var targetScale = isSelected ? _settings.navigationButtonScale : 1f;
            var targetButtonColor = isSelected ? _settings.selectedButtonColor : _settings.unselectedButtonColor;
            var targetTextColor = isSelected ? _settings.selectedTextColor : _settings.unselectedTextColor;
            
            button.transform.DOScale(targetScale, _settings.navigationButtonDuration);
            
            if (buttonImage != null)
            {
                buttonImage.DOColor(targetButtonColor, _settings.navigationButtonDuration);
            }
            
            if (buttonText != null)
            {
                buttonText.DOColor(targetTextColor, _settings.navigationButtonDuration);
            }
        }
        
        public void AnimateTextTick(TextMeshProUGUI text)
        {
            text.transform.DOPunchScale(Vector3.one * _settings.timePulseScale, _settings.timePulseDuration, 0, 0);
        }
        
        public void AnimateTextCompletion(TextMeshProUGUI text)
        {
            var sequence = DOTween.Sequence();
            sequence.Append(text.transform.DOScale(_settings.textScaleCompletionMax, _settings.textScaleCompletionDuration));
            sequence.Append(text.transform.DOScale(1f, _settings.textScaleCompletionDuration));
            sequence.Join(text.DOColor(_settings.completionColor, _settings.textScaleCompletionDuration));
            sequence.Append(text.DOColor(_settings.normalColor, _settings.textScaleCompletionDuration));
        }
        
        public Sequence CreatePulseAnimation(Transform target)
        {
            return DOTween.Sequence()
                .Append(target.DOScale(_settings.pulseScale, _settings.pulseDuration))
                .Append(target.DOScale(1f, _settings.pulseDuration))
                .SetLoops(-1);
        }
        
        public Sequence CreateFinitePulseAnimation(Transform target, int loops = 2)
        {
            return DOTween.Sequence()
                .Append(target.DOScale(_settings.pulseScale, _settings.pulseDuration))
                .Append(target.DOScale(1f, _settings.pulseDuration))
                .SetLoops(loops);
        }
        
        public void AnimateShake(Transform target)
        {
            target.DOShakePosition(_settings.shakeDuration, _settings.shakeStrength, _settings.shakeVibrato);
        }
        
        public void AnimatePositionPunch(Transform target, Vector3 direction)
        {
            target.DOPunchPosition(direction * _settings.punchPositionStrength, _settings.punchPositionDuration);
        }
        
        public void AnimateInputGroupVisibility(CanvasGroup inputGroup, bool visible)
        {
            inputGroup.DOFade(visible ? 1f : 0f, _settings.inputFadeDuration);
            inputGroup.interactable = visible;
            inputGroup.blocksRaycasts = visible;
        }
        
        public void AnimateListItemAppear(GameObject item)
        {
            item.transform.localScale = Vector3.zero;
            item.transform.DOScale(1f, _settings.itemAppearDuration).SetEase(_settings.itemAppearEase);
            
            var canvasGroup = item.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.DOFade(1f, _settings.itemAppearDuration);
            }
        }
        
        public void AnimateListItemDisappear(Transform item, System.Action onComplete = null)
        {
            item.DOScale(0f, _settings.itemDisappearDuration)
                .OnComplete(() => onComplete?.Invoke());
        }
        
        public void AnimateColorChange(Image image, Color targetColor)
        {
            image.DOColor(targetColor, _settings.colorChangeDuration);
        }
        
        public void AnimateColorChange(TextMeshProUGUI text, Color targetColor)
        {
            text.DOColor(targetColor, _settings.colorChangeDuration);
        }
        
        public void ResetTransform(Transform target)
        {
            target.localScale = Vector3.one;
            target.localPosition = Vector3.zero;
            target.localRotation = Quaternion.identity;
        }
        
        public void ResetTextAppearance(TextMeshProUGUI text)
        {
            text.color = _settings.normalColor;
            text.transform.localScale = Vector3.one;
        }
        
        public void KillTweens(Transform target)
        {
            target.DOKill();
        }
        
        public void KillTweens(Image image)
        {
            image.DOKill();
            image.transform.DOKill();
        }
        
        public void KillTweens(TextMeshProUGUI text)
        {
            text.DOKill();
            text.transform.DOKill();
        }
        
        public void KillTweens(CanvasGroup canvasGroup)
        {
            canvasGroup.DOKill();
        }
    }
}