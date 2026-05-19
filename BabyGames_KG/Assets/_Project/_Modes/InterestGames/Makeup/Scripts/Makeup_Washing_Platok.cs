using _Project.Scripts.Sound;
using DG.Tweening;
using Sirenix.OdinInspector;
using Sounds;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace InterestGames
{
    public class Makeup_Washing_Platok : Makeup_Washing_ItemBase, ISelfValidator
    {
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private Sound _sound;

        public override void Initialize()
        {
            foreach (var item in items)
            {
                item.boxCollider.enabled = true;
                item.onMouseEnter = () =>
                {
                    item.spriteRenderer.transform.DOScale(0, 2f);
                    item.waterDrop.transform.DOScale(0, 2f);
                    item.boxCollider.enabled = false;
                    item.state = Makeup_Pena.State.Washed;
                };
            }
        }

        private void Update()
        {
            if (Pointer.current.press.isPressed == false)
            {
                if (_audioSource != null)
                {
                    _audioSource.Pause();
                }
            }
        }

        public override async void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);

            if (_audioSource != null)
            {
                _audioSource.clip = await _sound.GetSound();
                _audioSource.Play();
            }

            Sound_FX.Play_Static(Sound_Effect.StartDrag);
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);

            var readyItems = items.Where(x => x.state == Makeup_Pena.State.Washed).ToList();
            bool itemsDone = readyItems.Count > items.Count - 5;

            if (itemsDone)
            {
                foreach (var item in items)
                {
                    item.boxCollider.enabled = true;
                    item.spriteRenderer.transform.DOScale(0, 2f);
                    item.waterDrop.transform.DOScale(0, 2f);
                }

                onFinish?.Invoke();
                enabled = false;
            }
        }
    }
}
