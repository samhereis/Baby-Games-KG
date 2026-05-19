using _Project._Modes.Hiding.Scripts.State;
using Helpers;
using Hiding;
using Identifiers;
using Sirenix.OdinInspector;
using Spine.Unity;
using System.Threading.Tasks;
using UnityEngine;

namespace Modes.Sorting
{
    public class HidingDropZone_Identifier : IdentifierBase
    {
        [SerializeField] private RectTransform _holder;

        public SkeletonGraphic skeletonGraphic;
        public RectTransform skeletonGraphicRect;

        private HidingWaveUnit _waveUnit;
        private Hiding_GameState_Model _model;

        public async Task Construct(SkeletonGraphic skeletonGraphic, HidingWaveUnit waveUnit, Hiding_GameState_Model model)
        {
            this.skeletonGraphic = skeletonGraphic;
            _waveUnit = waveUnit;
            _model = model;

            skeletonGraphicRect = this.skeletonGraphic.GetComponent<RectTransform>();

            skeletonGraphic.MatchRectTransformWithBounds();
            await AsyncHelper.NextFrame();

            Setup(); 
            Grayout(true);
        }

        [Button]
        private void Setup()
        {
            skeletonGraphic.transform.localScale = _waveUnit.draggable.pannelScale;
            skeletonGraphicRect.pivot = Vector3.zero;
            skeletonGraphicRect.anchorMin = Vector2.one * 0.5f;
            skeletonGraphicRect.anchorMax = Vector2.one * 0.5f;
            skeletonGraphicRect.anchoredPosition3D = _waveUnit.draggable.panelAnchoredPosition;
            skeletonGraphicRect.anchoredPosition3D = _waveUnit.draggable.panelAnchoredPosition;

            if (string.IsNullOrEmpty(_waveUnit.draggable.inPannelAnimation) == false)
            {
                skeletonGraphic.AnimationState.ClearTracks();
                skeletonGraphic.AnimationState.SetAnimation(0, _waveUnit.draggable.inPannelAnimation, false);
            }
        }

        public async void Grayout(bool active)
        {
            skeletonGraphic.material = active
                ? await _model.hiding_Data._panelItemMaterial_Gray.GetAssetAsync()
                : await _model.hiding_Data._panelItemMaterial_Normal.GetAssetAsync();
        }

        public void OnDropped()
        {
            Grayout(false);
        }
    }
}