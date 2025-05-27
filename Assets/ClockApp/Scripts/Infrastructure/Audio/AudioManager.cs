using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UniRx;

namespace ClockApp.Infrastructure.Audio
{
    public class AudioManager : MonoBehaviour
    {
        [Header("Audio Sources")]
        [SerializeField] private AudioSource effectsSource;
        [SerializeField] private AudioSource notificationSource;
        
        [Header("Audio Clips")]
        [SerializeField] private AudioClip timerCompleteSound;
        [SerializeField] private AudioClip buttonClickSound;
        [SerializeField] private AudioClip lapRecordSound;
        [SerializeField] private AudioClip tickSound;
        
        [Header("Audio Mixer")]
        [SerializeField] private AudioMixer audioMixer;
        
        private readonly Dictionary<string, AudioClip> _audioClips = new Dictionary<string, AudioClip>();
        private readonly CompositeDisposable _disposables = new CompositeDisposable();
        
        // Volume settings (saved in PlayerPrefs)
        private readonly ReactiveProperty<float> _masterVolume = new ReactiveProperty<float>(1f);
        private readonly ReactiveProperty<float> _effectsVolume = new ReactiveProperty<float>(1f);
        private readonly ReactiveProperty<bool> _isMuted = new ReactiveProperty<bool>(false);
        
        public IReadOnlyReactiveProperty<float> MasterVolume => _masterVolume;
        public IReadOnlyReactiveProperty<float> EffectsVolume => _effectsVolume;
        public IReadOnlyReactiveProperty<bool> IsMuted => _isMuted;
        
        private void Awake()
        {
            LoadAudioSettings();
            RegisterAudioClips();
            SetupVolumeSubscriptions();
        }
        
        private void LoadAudioSettings()
        {
            _masterVolume.Value = PlayerPrefs.GetFloat("MasterVolume", 1f);
            _effectsVolume.Value = PlayerPrefs.GetFloat("EffectsVolume", 1f);
            _isMuted.Value = PlayerPrefs.GetInt("IsMuted", 0) == 1;
        }
        
        private void RegisterAudioClips()
        {
            _audioClips["timer_complete"] = timerCompleteSound;
            _audioClips["button_click"] = buttonClickSound;
            _audioClips["lap_record"] = lapRecordSound;
            _audioClips["tick"] = tickSound;
        }
        
        private void SetupVolumeSubscriptions()
        {
            _masterVolume
                .Subscribe(volume =>
                {
                    if (audioMixer != null)
                        audioMixer.SetFloat("MasterVolume", Mathf.Log10(volume) * 20);
                    PlayerPrefs.SetFloat("MasterVolume", volume);
                })
                .AddTo(_disposables);
            
            _effectsVolume
                .Subscribe(volume =>
                {
                    if (audioMixer != null)
                        audioMixer.SetFloat("EffectsVolume", Mathf.Log10(volume) * 20);
                    PlayerPrefs.SetFloat("EffectsVolume", volume);
                })
                .AddTo(_disposables);
            
            _isMuted
                .Subscribe(muted =>
                {
                    AudioListener.volume = muted ? 0f : 1f;
                    PlayerPrefs.SetInt("IsMuted", muted ? 1 : 0);
                })
                .AddTo(_disposables);
        }
        
        public void PlayTimerComplete()
        {
            PlayNotification("timer_complete");
            
            #if UNITY_IOS || UNITY_ANDROID
            Handheld.Vibrate();
            #endif
        }
        
        public void PlayButtonClick()
        {
            PlayEffect("button_click", 0.5f);
        }
        
        public void PlayLapRecord()
        {
            PlayEffect("lap_record", 0.7f);
        }
        
        public void PlayTick()
        {
            PlayEffect("tick", 0.3f);
        }
        
        public void PlayEffect(string clipName, float volume = 1f)
        {
            if (_isMuted.Value)
                return;
            
            if (_audioClips.TryGetValue(clipName, out var clip) && clip != null)
            {
                effectsSource.PlayOneShot(clip, volume);
            }
            else
            {
                Debug.LogWarning($"Audio clip '{clipName}' not found");
            }
        }
        
        public void PlayNotification(string clipName, float volume = 1f)
        {
            if (_audioClips.TryGetValue(clipName, out var clip) && clip != null)
            {
                notificationSource.PlayOneShot(clip, volume);
            }
            else
            {
                Debug.LogWarning($"Notification clip '{clipName}' not found");
            }
        }
        
        public void SetMasterVolume(float volume)
        {
            _masterVolume.Value = Mathf.Clamp01(volume);
        }
        
        public void SetEffectsVolume(float volume)
        {
            _effectsVolume.Value = Mathf.Clamp01(volume);
        }
        
        public void ToggleMute()
        {
            _isMuted.Value = !_isMuted.Value;
        }
        
        private void OnDestroy()
        {
            _disposables?.Dispose();
            PlayerPrefs.Save();
        }
    }
}