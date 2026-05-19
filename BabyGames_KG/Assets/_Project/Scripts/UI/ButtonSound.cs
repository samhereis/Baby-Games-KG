using DataClasses;
using Loggers;
using Services;
using SO.Lists;
using Sounds;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Effects
{
    [RequireComponent(typeof(Button))]
    [DisallowMultipleComponent]
    public class ButtonSound : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private Sound_SO _sound;
        [SerializeField] private SoundQueue _soundQueue;

        private static ISoundPlayer _soundPlayer_;
        private static ISoundPlayer _soundPlayer
        {
            get
            {
                if (_soundPlayer_ == null) _soundPlayer_ = DiService.Get<ISoundPlayer>();
                return _soundPlayer_;
            }
        }

        public void SetSound(Sound_SO sound) { _sound = sound; }

        private void Validate()
        {
            if (_button == null) { _button = GetComponent<Button>(); }
        }

        private void Awake()
        {
            Validate();
        }

        private void OnEnable()
        {
            _button.onClick.AddListener(OnClick);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(OnClick);
        }

        private void OnClick()
        {
            try
            {
                if (_sound != null)
                {
                    _soundPlayer?.TryPlay(_sound.sound);
                    return;
                }

                if (_soundQueue != null)
                {
                    _soundPlayer?.TryPlay(_soundQueue);
                    return;
                }
            }
            catch (Exception ex)
            {
                CustomLogger.instance?.LogException(ex);
            }
        }
    }
}