using DataClasses;
using DG.Tweening;
using FX;
using Gameplay;
using Helpers;
using Services;
using Sirenix.OdinInspector;
using Sounds;
using System.Threading.Tasks;
using _Project.Scripts.Sound;
using UnityEngine;
using UnityEngine.UI;

namespace Coocking
{
    public class CoockingBurger_Frying : CoockingBurger_StateBase
    {
        public DroppableGeneral_SimpleController droppable;
        public Dropable_General item;

        public Transform plate;
        public Transform fryingIndicatorParent;
        public Slider fryringIndicator;
        public BoxCollider boxCollider;
        public Transform grillerParent;
        public Transform cutletteParent;
        public SpriteRenderer cutletteUnfried_1;
        public SpriteRenderer cutletteUnfried_2;

        [FoldoutGroup("Sound")] public Sound frySound;
        [FoldoutGroup("Sound")] public SoundQueue_Advanced endSound;

        [Space]
        public HintHand_Drag _hintHand_Drag;

        public float fryingDuration = 5;

        private bool _hasFlipped;
        private ISoundPlayer _audioSource;

        public override async Task Enter()
        {
            await base.Enter();

            boxCollider.enabled = false;
            fryingIndicatorParent.DOScale(0, 0f);
            plate.DOMoveX(cutletteParent.transform.position.x, 1);
            await transform.DOMoveX(0, 0.5f).AsyncWaitForCompletion();

            item.hasDropped.AddListener(StartFrying);
            item.onDropStart += OnDrop;
            item.objectJuicer.StartJamming();

            _hintHand_Drag.SetIsActive(true);

            _audioSource = DiService.Get<ISoundPlayer>();
        }

        public override async Task Exit()
        {
            _audioSource.Stop(frySound);

            await base.Exit();

            item.hasDropped.RemoveListener(StartFrying);
            item.onDropStart -= OnDrop;
            plate.DOMoveX(-25, 1);

            _hintHand_Drag.SetIsActive(false);
        }

        private void OnDrop(Dropable_General controller)
        {
            item.objectJuicer.StopJamming();
            item.GetComponent<BoxCollider>().enabled = false;
        }

        private async void StartFrying(bool isDropped)
        {
            _hintHand_Drag.SetIsActive(false);

            item.GetComponent<BoxCollider>().enabled = false;
            plate.DOMoveX(-25, 1);

            cutletteParent.SetParent(grillerParent, true);
            await grillerParent.DOMoveX(0, 1).AsyncWaitForCompletion();

            fryringIndicator.value = 0;
            fryingIndicatorParent.DOScale(1, 0.5f);

            await AsyncHelper.DelayFloat(1f);

#if UNITY_EDITOR
            fryingDuration = 1;
#endif

            if (_hasFlipped == false)
            {
                cutletteUnfried_1.DOFade(0, fryingDuration);
            }
            else
            {
                cutletteUnfried_2.DOFade(0, fryingDuration);
            }

            _audioSource.TryPlay(frySound);
            await fryringIndicator.DOValue(1, fryingDuration).AsyncWaitForCompletion();
            _audioSource.Stop(frySound);

            if (_hasFlipped == false)
            {
                cutletteParent.transform.DORotate(new Vector3(180, 0, 0), 1);
                cutletteUnfried_2.DOFade(0, 0);
                cutletteUnfried_2.sortingOrder = 4;
                cutletteUnfried_2.DOFade(1, 0.5f);
                _hasFlipped = true;

                StartFrying(false);
            }
            else
            {
                await transform.DOMoveX(-25, 0.5f).AsyncWaitForCompletion();

                Sound_FX.PlayAsync_Static(endSound);
                _nextState = _nextStateOnWin;
            }
        }
    }
}