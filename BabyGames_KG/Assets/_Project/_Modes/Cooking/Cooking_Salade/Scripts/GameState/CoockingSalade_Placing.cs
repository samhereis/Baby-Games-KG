using Coocking;
using DG.Tweening;
using FX;
using Helpers;
using Services;
using Spine.Unity;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace CoockingSalade
{
    public class CoockingSalade_Placing : CoockingSalade_StateBase
    {
        public List<Cooking_Cuttable> cuttables = new();
        public Cooking_Cuttable currentCuttable;
        public CoockingSalade_Mixing mixingState;

        public Transform plate;
        public Transform plate_target;

        public Transform dock;

        public SkeletonAnimation miskaSkeletonAnimation;
        public Transform miska;
        public Transform miska_target;

        public Dropable_World _salt;
        public Transform salt_target;

        [SerializeField] private HintHand_Drag _hintHand_Drag;

        public override async Task Enable()
        {
            if (cuttables.Count < 1) { cuttables = FindObjectsByType<Cooking_Cuttable>(UnityEngine.FindObjectsInactive.Include, UnityEngine.FindObjectsSortMode.InstanceID).Reverse().ToList(); }

            miskaSkeletonAnimation.gameObject.SetActive(false);

            dock.transform.DOScale(0, 0.25f).SetEase(Ease.InOutBack);
            miska.transform.DOMove(miska_target.position, 0.25f).SetEase(Ease.InOutBack);
            miskaSkeletonAnimation.transform.DOMove(miska_target.position, 0.25f).SetEase(Ease.InOutBack);

            _hintHand_Drag = GetComponent<HintHand_Drag>();

            await base.Enable();
        }

        public override async Task Enter()
        {
            _nextState = null;

            await base.Enter();

            currentCuttable = cuttables.First(x => x.isPlaced.value == false);
            currentCuttable.GetComponent<BoxCollider>().enabled = true;

            foreach (var item in currentCuttable.pieces) { item.gameObject.SetActive(false); }
            foreach (var item in currentCuttable.forceAltPieces) { item.gameObject.SetActive(true); }

            plate.transform.DOMoveX(plate_target.position.x, 1);
            currentCuttable.transform.DOMoveX(plate_target.position.x, 1);

            _hintHand_Drag.SetIsActive(true);


            if (currentCuttable.isCut.value)
            {
                _hintHand_Drag.targets.Clear();
                _hintHand_Drag.objects.Clear();
                _hintHand_Drag.objects.Add(currentCuttable.forceAltPieces[0].transform);
                _hintHand_Drag.targets.Add(currentCuttable.targetPlace.transform);
            }
            else
            {
                _hintHand_Drag.targets.Clear();
                _hintHand_Drag.objects.Clear();
                _hintHand_Drag.objects.Add(currentCuttable.indicator);
                _hintHand_Drag.targets.Add(currentCuttable.indicator_end);
            }

            foreach (var item in cuttables)
            {
                item.isCut.RemoveListener(OnPlaced);
                item.isPlaced.AddListener(OnPlaced);
                item.onStartedPlacing += OnItemStartedPlacing;
            }
        }

        private void OnItemStartedPlacing(Cooking_Cuttable cuttable)
        {
            cuttable.onStartedPlacing -= OnItemStartedPlacing;
            cuttable.GetComponent<BoxCollider>().enabled = false;
        }

        private void OnPlaced(bool value)
        {
            currentCuttable.transform.DOScale(currentCuttable.dropScale, 1);
            currentCuttable.GetComponent<BoxCollider>().enabled = false;

            int index = 0;
            foreach (var item in currentCuttable.forceAltPieces)
            {
                var piece = currentCuttable.targetPlace.secondary.GetRandom();
                item.transform.DOMove(piece.position + Vector3.up * 5, 0.5f).OnComplete(() =>
                {
                    item.transform.DOMove(piece.position, 1);
                });

                index++;
            }

            if (cuttables.TrueForAll(x => x.isPlaced.value == true))
            {
                plate.DOMoveX(25, 1);
                _salt.transform.DOMove(salt_target.position, 0.25f).SetEase(Ease.InOutBack);
                _salt.onFinish += Win;

                _hintHand_Drag.targets.Clear();
                _hintHand_Drag.objects.Clear();
                _hintHand_Drag.objects.Add(_salt.transform);
                _hintHand_Drag.targets.Add(_salt.targetPosition);

                return;
            }
            else
            {
                _nextState = this;
            }
        }

        private void Win(Dropable_World dropable_World)
        {
            if (isDone) { return; }

            _hintHand_Drag.SetIsActive(false);

            _salt.onFinish -= Win;

            _salt.transform.DOMoveX(25, 0.25f).SetEase(Ease.InOutBack);
            plate.transform.DOMoveX(25, 0.25f).SetEase(Ease.InOutBack);

            DiService.Get<StateEnd_FX>()?.DoFX();
            _nextState = mixingState;
            isDone = true;
        }
    }
}