using DataClasses;
using DG.Tweening;
using Helpers;
using Loggers;
using Sirenix.OdinInspector;
using Spine.Unity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Identifiers
{
    public class HidingActivity_Identifier : _ActivityBase_Identifier
    {
        [SerializeField] private SkeletonAnimation _background;
        [SerializeField] private List<KeyedObject<string, Vector3>> _position = new();
        [SerializeField] private List<KeyedObject<string, Vector3>> _scales = new();

        public override void UpdateData(Activity activity)
        {
            base.UpdateData(activity);

            type = DataClasses.ActivityType.Hiding;
        }

        [Button]
        public async Task Initialize()
        {
            await _background.Separate();
            foreach (var partsRenderer in _background.GetComponentsInChildren<SkeletonPartsRenderer>())
            {
                partsRenderer.MeshRenderer.sortingLayerName = "Background";
            }

            Get<Canvas>().renderMode = RenderMode.ScreenSpaceCamera;
            Get<Canvas>().worldCamera = Camera.main;
            Get<Canvas>().planeDistance = 5;

            RectTransform rectTransform = GetComponent<RectTransform>();
            _background.transform.localScale = rectTransform.localScale * 10000;

            DoSizing();
        }

        [Button]
        private void DoSizing()
        {
            try
            {
                foreach (var partsRenderer in _background.GetComponentsInChildren<SkeletonPartsRenderer>())
                {
                    if (_position.Find(x => x.key == partsRenderer.name) is KeyedObject<string, Vector3> position)
                    {
                        partsRenderer.transform.DOMove(position.value, 0.25f);
                    }

                    if (_scales.Find(x => x.key == partsRenderer.name) is KeyedObject<string, Vector3> scale)
                    {
                        partsRenderer.transform.DOScale(scale.value, 0.25f);
                    }
                }
            }
            catch (Exception ex)
            {
                CustomLogger.instance?.LogException(ex);
            }
        }
    }
}