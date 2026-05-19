using Bubble;
using DG.Tweening;
using Helpers;
using Loggers;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FX
{
    public class HintHand_Click : MonoBehaviour
    {
        public List<Transform> targets = new();

        public Transform visual;

        public float hintEvery_Seconds = 2;
        public bool isShowing;

        [SerializeField] private bool _isActive = false;
        private int _currentObjectIndex = 0;

        private BubbleUnit _currentBubbleUnit;

        private Vector3 GetTargetPosition()
        {
            targets.RemoveNulls();
            targets.RemoveAll(x => x == null);

            var obj = targets.GetRandom();
            _currentObjectIndex = targets.IndexOf(obj);

            if (obj is not RectTransform) { return Camera.main.WorldToScreenPoint(obj.position); }
            else { return obj.position; }
        }

        private void Awake()
        {
            if (visual == null) { visual = PlayerActions_DataHolder.instance.hintVisual_Hand; }
        }

        private void OnDisable()
        {
            visual.gameObject.SetActive(false);
            visual.DOKill();
        }

        public void Update()
        {
            if (_isActive && PlayerActions_DataHolder.instance.timeSinceLastAction > hintEvery_Seconds)
            {
                visual.gameObject.SetActive(true);
                if (isShowing == true) { return; }
                if (targets.Count < 1) { return; }

                Show();
            }
            else
            {
                if (visual.gameObject.activeSelf == false) { return; }
                visual.gameObject.SetActive(false);
                visual.DOKill();
                isShowing = false;

                if (_currentBubbleUnit != null)
                {
                    _currentBubbleUnit.forceStop = false;
                    Destroy(_currentBubbleUnit.gameObject);
                    _currentBubbleUnit = null;
                }
            }
        }

        public void SetIsActive(bool isActive)
        {
            _isActive = isActive;
            PlayerActions_DataHolder.ResetTime();
        }

        public async void Show()
        {
            isShowing = true;

            try
            {
                var position = GetTargetPosition();
                var target = targets[_currentObjectIndex];
                if (target == null)
                {
                    targets.Clear();
                    return;
                }

                if (target.TryGetComponent<BubbleUnit>(out var bubbleUnit))
                {
                    _currentBubbleUnit = Instantiate(bubbleUnit, bubbleUnit.transform.parent);
                    _currentBubbleUnit.MakeCopy();
                    _currentBubbleUnit.transform.position = bubbleUnit.transform.position;
                    _currentBubbleUnit.forceStop = true;
                }

                position.z = 0;
                visual.transform.position = position;
                visual.transform.localScale = Vector3.zero;

                visual.gameObject.SetActive(true);
                await visual.DOScale(1f, 0.25f).SetEase(Ease.OutBack).AsyncWaitForCompletion();

                await visual.DOMove(position, 0.5f).SetEase(Ease.OutBack).SetDelay(1f).AsyncWaitForCompletion();
                await visual.DOScale(0.5f, 0.25f).SetEase(Ease.OutBack).SetDelay(0.25f).AsyncWaitForCompletion();
                _currentBubbleUnit?.DoAnimation();
                await AsyncHelper.DelayFloat(0.25f);
                await visual.DOScale(1f, 0.25f).SetEase(Ease.OutBack).SetDelay(0.25f).AsyncWaitForCompletion();

                await visual.DOScale(0, 0.25f).SetEase(Ease.OutBack).SetDelay(0.25f).AsyncWaitForCompletion();
                visual.gameObject.SetActive(false);

                if (_currentBubbleUnit != null)
                {
                    _currentBubbleUnit.forceStop = false;
                    Destroy(_currentBubbleUnit.gameObject);
                }

                PlayerActions_DataHolder.ResetTime();
            }
            catch (Exception ex)
            {
                CustomLogger.instance?.LogException(ex);
            }

            isShowing = false;
        }
    }
}