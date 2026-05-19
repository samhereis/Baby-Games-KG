using CoockingSalade;
using DG.Tweening;
using FX;
using System.Threading.Tasks;
using UnityEngine;

namespace Coocking
{
    public class CoockingFeeding_Cutting : CoockingFeeding_StateBase
    {
        public Cooking_Cuttable carrot;
        public Cooking_Cuttable_Horrizontal meat_Whole;
        public Cooking_Cuttable meat_Peace;

        public Transform meatWholeTransform;
        public Transform holder;

        public HintHand_Drag _hintHand_Drag;

        public override async Task Enter()
        {
            await base.Enter();

            holder.DOMoveX(0f, 1);
            transform.DOMoveY(0, 1);

            carrot.GetComponent<BoxCollider>().enabled = false;
            meat_Whole.GetComponent<BoxCollider>().enabled = false;
            meat_Peace.GetComponent<BoxCollider>().enabled = false;

            carrot.isCut.RemoveListener(OnCarrotCut);
            carrot.isCut.AddListener(OnCarrotCut);

            meat_Whole.isCut.RemoveListener(OnWholeMeatCut);
            meat_Whole.isCut.AddListener(OnWholeMeatCut);

            meat_Peace.isCut.RemoveListener(OnPeaceMeatCut);
            meat_Peace.isCut.AddListener(OnPeaceMeatCut);

            meatWholeTransform.DOMoveX(0, 1);
            await meat_Whole.Initialize(false);

            _hintHand_Drag = GetComponent<HintHand_Drag>();
            if (_hintHand_Drag == null) { _hintHand_Drag = gameObject.AddComponent<HintHand_Drag>(); }
            _hintHand_Drag.SetIsActive(true);
            SetHintHandData(meat_Whole.indicator, meat_Whole.indicator_End);
        }

        public override async Task Exit()
        {
            if (_isDone)
            {
                holder.DOMoveX(-25, 0.5f).SetEase(Ease.InOutBack).AsyncWaitForCompletion();
            }

            carrot.isCut.RemoveListener(OnCarrotCut);
            meat_Whole.isCut.RemoveListener(OnWholeMeatCut);
            meat_Peace.isCut.RemoveListener(OnPeaceMeatCut);

            await base.Exit();
        }

        private void OnWholeMeatCut(bool obj)
        {
            meat_Peace.Initialize(false);
            SetHintHandData(meat_Peace.indicator, meat_Peace.indicator_end);
        }

        private void OnPeaceMeatCut(bool obj)
        {
            meatWholeTransform.transform.DOMoveX(-25, 1);
            carrot.transform.DOMoveX(0, 1);
            carrot.Initialize();

            SetHintHandData(carrot.indicator, carrot.indicator_end);
        }

        private void OnCarrotCut(bool obj)
        {
            if (_isDone == true) { return; }

            _isDone = true;
            _nextState = _nextStateOnWin;
        }

        public override void ForceWin()
        {
            base.ForceWin();
            _nextState = _nextStateOnWin;
        }

        private void SetHintHandData(Transform indicator, Transform indicator_end)
        {
            _hintHand_Drag.targets.Clear();
            _hintHand_Drag.objects.Clear();
            _hintHand_Drag.objects.Add(indicator);
            _hintHand_Drag.targets.Add(indicator_end);
        }
    }
}