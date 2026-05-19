using Coocking;
using DG.Tweening;
using FX;
using Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using _Project.Scripts.Sound;
using Sirenix.OdinInspector;
using Sounds;
using UnityEngine;

namespace CoockingSalade
{
    public class CoockingBurger_Cutting : CoockingBurger_StateBase
    {
        public List<Cooking_Cuttable> cuttables = new();
        public Cooking_Cuttable currentCuttable;

        public Transform miska;
        public Transform miska_target;

        [FoldoutGroup("Sound")] public SoundQueue_Advanced endSound;

        [Space]
        public HintHand_Drag _hintHand_Drag;

        public override async Task Enter()
        {
            if (cuttables.Count < 1)
            {
                cuttables = FindObjectsByType<Cooking_Cuttable>(UnityEngine.FindObjectsInactive.Include, UnityEngine.FindObjectsSortMode.InstanceID).Reverse().ToList();
            }

            await base.Enter();

            transform.DOMoveY(0, 1);

            miska.transform.DOMove(miska_target.position, 0.25f).SetEase(Ease.InOutBack);
            miska.transform.DOScale(miska_target.localScale, 0.25f).SetEase(Ease.InOutBack);

            foreach (var item in cuttables)
            {
                item.gameObject.SetActive(item.isCut.value ? true : false);
            }

            currentCuttable = cuttables.First(x => x.isCut.value == false);
            currentCuttable.Initialize();

            foreach (var item in cuttables)
            {
                item.isCut.RemoveListener(OnCut);
                item.isCut.AddListener(OnCut);
            }

            _hintHand_Drag.SetIsActive(true);
            _hintHand_Drag.targets.Clear();
            _hintHand_Drag.objects.Clear();

            _hintHand_Drag.objects.Add(currentCuttable.indicator);
            _hintHand_Drag.targets.Add(currentCuttable.indicator_end);
        }

        public override async Task Disable()
        {
            if (_isDone) { miska.transform.DOMoveY(25, 0.25f).SetEase(Ease.InOutBack); }
            await base.Disable();
        }

        private void OnCut(bool value)
        {
            _hintHand_Drag.SetIsActive(false);

            currentCuttable.transform.DOScale(currentCuttable.dropScale, 1);
            currentCuttable.transform.DOMoveX(25, 1f);

            if (cuttables.TrueForAll(x => x.isCut.value == true))
            {
                _isDone = true;

                Sound_FX.PlayAsync_Static(endSound);
                _nextState = _nextStateOnWin;
            }
            else
            {
                _nextState = this;
            }
        }

        public override void ForceWin()
        {
            _nextState = _nextStateOnWin;
            base.ForceWin();
        }
    }
}