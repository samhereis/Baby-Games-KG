using CustomAttributes;
using DG.Tweening;
using Helpers;
using Loggers;
using Spine.Unity;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FX
{
    public class HintHand_Drag : MonoBehaviour
    {
        [SerializeField] private bool _getScreenPosition_Objects = false;
        [SerializeField] private bool _getScreenPosition_Targets = false;

        public List<Transform> objects = new();
        public List<Transform> targets = new();

        public Transform visual;
        [SerializeField] private bool isActive = false;

        public float hintEvery_Seconds = 2;
        public bool isShowing;

        private int _currentObjectIndex = 0;

        private Vector3 GetObjectosition()
        {
            var target = objects.GetRandom();
            _currentObjectIndex = objects.IndexOf(target);

            if (target is RectTransform)
            {
                if (_getScreenPosition_Objects == false)
                {
                    return target.position;
                }
                else
                {
                    { return Camera.main.WorldToScreenPoint(target.position); }
                }
            }
            else { return Camera.main.WorldToScreenPoint(target.position); }
        }

        private Vector3 GetTargetPosition()
        {
            var target = targets.GetRandom();
            if (targets.HasEnoughElementsForIndex(_currentObjectIndex)) { target = targets[_currentObjectIndex]; }

            if (target is RectTransform)
            {
                if (_getScreenPosition_Targets == false)
                {
                    return target.position;
                }
                else
                {
                    { return Camera.main.WorldToScreenPoint(target.position); }
                }
            }
            else { return Camera.main.WorldToScreenPoint(target.position); }
        }

        public void SetIsActive(bool isActive)
        {
            enabled = isActive;

            this.isActive = isActive;
            PlayerActions_DataHolder.ResetTime();
        }

        private void Awake()
        {
            if (visual == null) { visual = PlayerActions_DataHolder.instance.hintVisual_Hand; }
        }

        private void OnEnable()
        {
            PlayerActions_DataHolder.ResetTime();
        }

        private void OnDisable()
        {
            visual?.gameObject.SetActive(false);
            visual?.DOKill();
        }

        public void Update()
        {
            if (Pointer.current?.press.isPressed == true)
            {
                foreach (var item in _meshRenderers)
                {
                    try
                    {
                        item.transform.localScale = Vector3.zero;
                        item.gameObject.SetActive(false);
                        Destroy(item.gameObject);
                    }
                    catch (Exception ex)
                    {
                        CustomLogger.instance?.LogException(ex);
                    }
                }

                foreach (var item in _skeletonPartsRenderers)
                {
                    try
                    {
                        item.transform.localScale = Vector3.zero;
                        item.gameObject.SetActive(false);
                        Destroy(item.gameObject);
                    }
                    catch (Exception ex)
                    {
                        CustomLogger.instance?.LogException(ex);
                    }
                }

                _materials.Clear();
                _skeletonPartsRenderers.Clear();
                _meshRenderers.Clear();
            }

            if (isActive && PlayerActions_DataHolder.instance.timeSinceLastAction > hintEvery_Seconds)
            {
                if (isShowing == true) { return; }
                if (objects.Count < 1) { return; }
                if (targets.Count < 1) { return; }

                Show();
            }
            else
            {

                if (visual.gameObject.activeSelf == false) { return; }
                visual.gameObject.SetActive(false);
                visual.DOKill();
                isShowing = false;
            }
        }

        public async void Show()
        {
            isShowing = true;

            try
            {
                var position = GetObjectosition();
                position.z = 0;
                visual.transform.position = position;
                visual.transform.localScale = Vector3.zero;

                visual.gameObject.SetActive(true);
                await visual.DOScale(1f, 0.25f).SetEase(Ease.OutBack).AsyncWaitForCompletion();

                await visual.DOMove(position, 0.5f).SetEase(Ease.OutBack).SetDelay(1f).AsyncWaitForCompletion();
                await visual.DOScale(0.5f, 0.25f).SetEase(Ease.OutBack).SetDelay(0.25f).AsyncWaitForCompletion();

                position = GetTargetPosition();
                FollowFinger(position);
                await visual.DOMove(position, 1).SetDelay(0.25f).AsyncWaitForCompletion();
                await visual.DOScale(1f, 0.25f).SetEase(Ease.OutBack).SetDelay(0.25f).AsyncWaitForCompletion();

                await visual.DOScale(0, 0.25f).SetEase(Ease.OutBack).SetDelay(0.25f).AsyncWaitForCompletion();
                visual.gameObject.SetActive(false);
            }
            catch (Exception ex)
            {
                CustomLogger.instance?.LogException(ex);
            }

            PlayerActions_DataHolder.ResetTime();
            isShowing = false;
        }

        [Fg_De, SerializeField] private List<Material> _materials = new();
        [Fg_De, SerializeField] private List<SkeletonPartsRenderer> _skeletonPartsRenderers = new();
        [Fg_De, SerializeField] private List<MeshRenderer> _meshRenderers = new();
        private async void FollowFinger(Vector3 target)
        {
            _materials.Clear();
            _skeletonPartsRenderers.Clear();
            _meshRenderers.Clear();

            var objectToDrag = objects[_currentObjectIndex];
            target = Camera.main.ScreenToWorldPoint(target);

            if (objectToDrag.TryGetComponent<HintHand_Attachment_SpineAnimation>(out var hha_sa))
            {
                try
                {
                    var copy = Instantiate(objectToDrag, objectToDrag.position, objectToDrag.rotation, objectToDrag.parent);
                    if (copy.TryGetComponent<MeshRenderer>(out var meshRenderer))
                    {
                        _meshRenderers.SafeAdd(meshRenderer);
                        meshRenderer.sortingLayerName = "AlwaysOnTop";
                        meshRenderer.sortingOrder += 100;
                    }

                    foreach (var item in copy.GetComponentsInChildren<SkeletonPartsRenderer>(true))
                    {
                        _skeletonPartsRenderers.SafeAdd(item);
                        item.MeshRenderer.sortingLayerName = "AlwaysOnTop";
                        item.MeshRenderer.sortingOrder += 100;
                    }

                    copy.GetComponent<SkeletonAnimation>()?.SetEnabled(false);
                    copy.GetComponent<SkeletonRenderSeparator>()?.SetEnabled(false);

                    foreach (var item in copy.GetComponent<MeshRenderer>().materials)
                    {
                        _materials.SafeAdd(item);
                    }

                    foreach (var skeletonPart in _skeletonPartsRenderers)
                    {
                        foreach (var item in skeletonPart.GetComponent<MeshRenderer>().materials)
                        {
                            _materials.SafeAdd(item);
                        }

                        skeletonPart.enabled = false;
                    }

                    foreach (var item in _materials)
                    {
                        item?.DOFade(0.75f, 0);
                    }

                    await copy.DOMove(target, 1).SetDelay(0.25f).AsyncWaitForCompletion();

                    foreach (var item in _materials) { item?.DOFade(0, 0.25f); }
                    await AsyncHelper.DelayFloat(0.25f);
                    Destroy(copy.gameObject);
                }
                catch (Exception ex)
                {
                    CustomLogger.instance?.LogException(ex);
                }
            }
        }
    }
}