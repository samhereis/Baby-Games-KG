using _Project._Modes.Hiding.Scripts.State;
using CustomAttributes;
using DG.Tweening;
using Helpers;
using Modes.Puzzle;
using Spine.Unity;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WhoLivesWhere
{
    public class WLW_Controller : StateMachineBase
    {
        public WhoLivedWhere_GameState_Model _model;

        [Fg_Se] public SkeletonAnimation _background;
        [Fg_Se] public List<string> _makeOnTopAll = new();

        [SerializeField] private Transform _traktorParent;
        [SerializeField] private Transform _traktorMesh;
        [SerializeField] private float _traktorParentScale = 1;
        [SerializeField] private float _traktorMeshScale = 0.75f;

        public void Construct(WhoLivedWhere_GameState_Model model)
        {
            _model = model;

            WLW_HomeSetState.allInstances.Clear();
            foreach (var item in _allStates)
            {
                if (item is WLW_HomeSetState homeSetState)
                {
                    WLW_HomeSetState.allInstances.Add(homeSetState);
                }
            }
            WLW_HomeSetState.allInstances.Shuffle_Original();
        }

        private async void OnEnable()
        {
            if (_traktorMesh != null) { _traktorMesh?.DOScale(_traktorMeshScale, 0).AsyncWaitForCompletion(); }
            if (_traktorParent != null) { _traktorParent?.DOScale(_traktorParentScale, 0).AsyncWaitForCompletion(); }

            if (_background != null)
            {
                await AsyncHelper.NextFrame();
                await _background.Separate();

                foreach (var item in GetComponentsInChildren<SkeletonPartsRenderer>().ToList())
                {
                    if (_makeOnTopAll.Contains(item.name))
                    {
                        item.MeshRenderer.sortingLayerName = "Front";
                        item.MeshRenderer.sortingOrder += 100;
                    }
                }
            }

            ChangeState(_startState);
        }

        public void Win()
        {
            _model.onFinish?.Invoke();
        }
    }
}