using DG.Tweening;
using UnityEngine;

namespace ClockApp.Presentation.Animations
{
    [CreateAssetMenu(fileName = "AnimationSettings", menuName = "ClockApp/Animation Settings")]
    public class AnimationSettings : ScriptableObject
    {
        [Header("Panel Animations")]
        public float fadeInDuration = 0.5f;
        public float scaleInDuration = 0.5f;
        public Ease scaleInEase = Ease.OutBack;
        
        [Header("Button Animations")]
        public float buttonPunchScale = 0.2f;
        public float buttonPunchDuration = 0.3f;
        public float buttonRotationDuration = 0.5f;
        public float buttonScaleDuration = 0.1f;
        public float buttonScaleAmount = 0.8f;
        
        [Header("Text Animations")]
        public float textScaleCompletionMax = 1.5f;
        public float textScaleCompletionDuration = 0.3f;
        
        [Header("Pulse Animations")]
        public float pulseScale = 1.1f;
        public float pulseDuration = 0.5f;
        public float timePulseScale = 0.05f;
        public float timePulseDuration = 0.2f;
        
        [Header("Movement Animations")]
        public float shakeStrength = 10f;
        public float shakeDuration = 0.5f;
        public int shakeVibrato = 20;
        public float punchPositionStrength = 10f;
        public float punchPositionDuration = 0.3f;
        
        [Header("List Item Animations")]
        public float itemAppearDuration = 0.3f;
        public float itemDisappearDuration = 0.2f;
        public Ease itemAppearEase = Ease.OutBack;
        
        [Header("Input Group Animations")]
        public float inputFadeDuration = 0.3f;
        
        [Header("Navigation Animations")]
        public float navigationButtonScale = 1.1f;
        public float navigationButtonDuration = 0.2f;
        public Color selectedButtonColor = Color.white;
        public Color unselectedButtonColor = Color.gray;
        public Color selectedTextColor = Color.black;
        public Color unselectedTextColor = Color.gray;
        
        [Header("Color Animations")]
        public float colorChangeDuration = 0.3f;
        public Color completionColor = Color.green;
        public Color normalColor = Color.white;
        public Color syncSuccessColor = Color.green;
        public Color syncFailColor = Color.red;
    }
}