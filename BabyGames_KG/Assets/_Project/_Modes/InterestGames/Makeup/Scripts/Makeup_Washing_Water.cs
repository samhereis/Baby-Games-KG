using _Project.Scripts.Sound;
using DG.Tweening;
using Sirenix.OdinInspector;
using Sounds;
using Spine.Unity;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace InterestGames
{
    public class Makeup_Washing_Water : Makeup_Washing_ItemBase, ISelfValidator, IPointerDownHandler, IPointerUpHandler
    {
        public SkeletonAnimation water;

        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private Sound _sound;

        public override void Initialize()
        {
            foreach (var item in items)
            {
                item.boxCollider.enabled = true;
                item.boxCollider.center = new Vector3(1, -1.75f, 0);
                item.onMouseEnter = () =>
                {
                    item.spriteRenderer.transform.DOScale(0, 2f);
                    item.state = Makeup_Pena.State.Watered;
                    item.waterDrop?.transform.DOScale(1, 2f);
                };
            }
        }

        protected override void Awake()
        {
            base.Awake();
            water.transform.DOScale(0, 0.25f);
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

        public async override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            water.transform.DOScale(1, 0.25f);
            water.AnimationState.ClearTracks();
            water.AnimationState.SetAnimation(0, "action", true);

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

            if (items.Where(x => x.state == Makeup_Pena.State.Watered).Count() >= items.Count / 2)
            {
                foreach (var item in items)
                {
                    item.spriteRenderer.transform.DOScale(0, 2f);
                    item.boxCollider.center = new Vector3(0, 0, 0);
                    item.boxCollider.enabled = false;
                    item.state = Makeup_Pena.State.Watered;
                    item.waterDrop?.transform.DOScale(1, 2f);
                }

                onFinish?.Invoke();
                enabled = false;
            }

            water.transform.DOScale(0, 0.25f);
        }
    }
}
