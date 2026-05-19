using _Project._Modes.Hiding.Scripts.State;
using DataClasses.AssetReferences;
using DG.Tweening;
using FX;
using Helpers;
using Hiding;
using Modes.Sorting;
using Spine.Unity;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Menus
{
    public class GameplayMenu_Hiding : MenuBase
    {
        public Button backButton;

        [SerializeField] private RectTransform _contentHolder;
        [SerializeField] private RectTransform _content;
        [SerializeField] private RectTransform _content_Temp;
        [SerializeField] private SkeletonGraphic _click;
        [SerializeField] private ExternalAssetReference_HasComponent<HidingDropZone_Identifier> _hidingZoneReference;

        private Hiding_GameState_Model _model;
        private HintHand_Drag hintHand_Drag => _model.hint_drag;

        protected override void Awake()
        {
            base.Awake();

            Get<Canvas>().renderMode = RenderMode.ScreenSpaceCamera;
            Get<Canvas>().worldCamera = Camera.main;
            Get<Canvas>().planeDistance = 5;

            _content_Temp.DOAnchorPos3DY(Screen.height, 0);
            _content.DOAnchorPos3DY(0, 0);

            List<HidingDropZone_Identifier> trash = _content.GetComponentsInChildren<HidingDropZone_Identifier>().ToList();
            foreach (var item in trash)
            {
                Destroy(item.gameObject);
            }

            List<HidingDropZone_Identifier> current = _content_Temp.GetComponentsInChildren<HidingDropZone_Identifier>().ToList();
            foreach (var item in current)
            {
                Destroy(item.gameObject);
            }
        }

        public async Task Initialize(HidingWave hidingWave, Hiding_GameState_Model model)
        {
            _model = model;
            _model.click = _click;

            List<HidingDropZone_Identifier> trash = _content.GetComponentsInChildren<HidingDropZone_Identifier>().ToList();
            List<HidingDropZone_Identifier> current = new();

            await _contentHolder.DOAnchorPosX(-250, 1).AsyncWaitForCompletion();
            foreach (var item in hidingWave.hidingWaveDatas)
            {
                var dropZoneComponent = await _hidingZoneReference.InstantiateAsync(_content_Temp);
                var copy = Instantiate(item.draggable.skeletonGraphic, dropZoneComponent.transform);
                current.Add(dropZoneComponent);

                await dropZoneComponent.Construct(copy, item, _model);

                item.dropZone = dropZoneComponent;
                item.draggable.Construct(item, _model);
            }

            foreach (var item in trash)
            {
                item.transform.SetParent(_content_Temp, true);
            }

            foreach (var item in current)
            {
                item.transform.SetParent(_content, true);
            }

            _content.DOAnchorPos3DY(Screen.height, 0);
            _content_Temp.DOAnchorPos3DY(0, 0);

            _content_Temp.DOAnchorPos3DY(-Screen.height / 1.5f, 0.5f).SetEase(Ease.OutBack);
            await _content.DOAnchorPos3DY(0, 0.5f).SetEase(Ease.OutBack).AsyncWaitForCompletion();
            _content_Temp.DOAnchorPos3DY(Screen.height / 1.5f, 0);

            foreach (var item in trash)
            {
                Destroy(item.gameObject);
            }

            SetHintHand();
        }

        private void SetHintHand()
        {
            if (hintHand_Drag != null)
            {
                hintHand_Drag.objects.Clear();
                hintHand_Drag.targets.Clear();

                foreach (var item in _model.currentWave.hidingWaveDatas)
                {
                    hintHand_Drag.objects.SafeAdd(item.draggable.transform);
                    hintHand_Drag.targets.SafeAdd(item.dropZone.transform);
                }

                hintHand_Drag?.SetIsActive(true);
            }
        }

        public override async void Disable(float? duration = null)
        {
            await _contentHolder.DOAnchorPos3DX(Screen.width * 1.5f, 2f).AsyncWaitForCompletion();
            base.Disable(duration);
        }
    }
}