using DG.Tweening;
using FX;
using Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace CoockingSalade
{
    public class CoockingSalade_ChoiseState : CoockingSalade_StateBase
    {
        public List<Cooking_Cuttable> cuttables = new();
        public Cooking_Cuttable currentCuttable;
        public CoockingSalade_Placing placing;

        [SerializeField] private HintHand_Drag _hintHand_Drag;

        public override async Task Enable()
        {
            if (cuttables.Count < 1) { cuttables = FindObjectsByType<Cooking_Cuttable>(UnityEngine.FindObjectsInactive.Include, UnityEngine.FindObjectsSortMode.InstanceID).Reverse().ToList(); }

            await base.Enable();
        }

        public override async Task Enter()
        {
            _nextState = null;

            await base.Enter();
            foreach (var item in cuttables)
            {
                item.gameObject.SetActive(item.isCut.value ? true : false);
            }

            currentCuttable = cuttables.First(x => x.isCut.value == false);
            currentCuttable.Initialize();

            _hintHand_Drag = GetComponent<HintHand_Drag>();

            _hintHand_Drag.SetIsActive(true);

            _hintHand_Drag.targets.Clear();
            _hintHand_Drag.objects.Clear();
            _hintHand_Drag.objects.Add(currentCuttable.indicator);
            _hintHand_Drag.targets.Add(currentCuttable.indicator_end);

            foreach (var item in cuttables)
            {
                item.isCut.RemoveListener(OnDashCompleted);
                item.isCut.AddListener(OnDashCompleted);
            }
        }

        private void OnDashCompleted(bool value)
        {
            currentCuttable.transform.DOScale(currentCuttable.dropScale, 1);
            currentCuttable.transform.DOMoveX(25, 1f);

            if (cuttables.TrueForAll(x => x.isCut.value == true))
            {
                if (isDone) { return; }

                DiService.Get<StateEnd_FX>()?.DoFX();
                _nextState = placing;
                isDone = true;
                return;
            }
            else
            {
                _nextState = this;
            }
        }
    }
}