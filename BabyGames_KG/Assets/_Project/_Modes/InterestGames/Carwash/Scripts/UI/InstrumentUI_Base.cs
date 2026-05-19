using _Project.Scripts.Sound;
using Assets._Project._Modes.Carwash.Scripts.State;
using DG.Tweening;
using Sounds;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Carwash
{
    public class InstrumentUI_Base : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        protected CarwashCar_Identifier _car => _model.currentCarSelection.car;

        public Action<InstrumentUI_Base> onComplete;

        [SerializeField] protected GameplayMenu_Carwash _gameplayMenu_Carwash;
        [SerializeField] protected AudioSource _audioSource;
        [SerializeField] protected SoundAdvanced _soundReference;

        private Carwash_GameState_Model _model;

        private AudioClip _audioClip;

        public virtual async void Construct(Carwash_GameState_Model model)
        {
            _model = model;

            _gameplayMenu_Carwash = FindAnyObjectByType<GameplayMenu_Carwash>();
            _audioClip = await _soundReference?.GetSound();
        }

        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            _gameplayMenu_Carwash?.panelHolder?.DOAnchorPos3DY(-300, 0.25f).SetEase(Ease.OutBack);

            if (_audioSource != null && _audioClip != null)
            {
                _audioSource.clip = _audioClip;
                _audioSource.loop = true;
                _audioSource.Play();
            }

            Sound_FX.Play_Static(Sound_Effect.StartDrag);
        }

        public virtual void OnDrag(PointerEventData eventData)
        {

        }

        public virtual void OnEndDrag(PointerEventData eventData)
        {
            _gameplayMenu_Carwash?.panelHolder?.DOAnchorPos3DY(150, 0.25f).SetEase(Ease.OutBack);

            _audioSource.clip = null;
            _audioSource.Stop();
        }

        public virtual void PutBack()
        {

        }
    }
}