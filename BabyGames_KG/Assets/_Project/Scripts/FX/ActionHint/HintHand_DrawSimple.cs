using Agents;
using DG.Tweening;
using Helpers;
using System.Collections.Generic;
using UnityEngine;

namespace FX
{
    public class HintHand_DrawSimple : MonoBehaviour
    {
        public AnimationAgent animationAgent;
        public List<Transform> sources = new();
        public List<Transform> targets = new();
        [SerializeField] private bool _isActive = false;

        public float hintEvery_Seconds = 2;
        public bool isShowing;

        private Vector3 GetSourcePosition()
        {
            var target = sources.GetRandom();

            if (target is not RectTransform) { return Camera.main.WorldToScreenPoint(target.position); }
            else { return target.position; }
        }

        private Vector3 GetTargetPosition()
        {
            var target = targets.GetRandom();

            if (target is not RectTransform) { return Camera.main.WorldToScreenPoint(target.position); }
            else { return target.position; }
        }

        public void SetIsActive(bool isActive)
        {
            this._isActive = isActive;
            PlayerActions_DataHolder.ResetTime();
        }

        private void Awake()
        {
            if (animationAgent == null) { animationAgent = PlayerActions_DataHolder.instance.hintVisual_HandWithAnimation; }
        }

        private void OnDisable()
        {
            animationAgent.gameObject.SetActive(false);
            animationAgent.transform.DOKill();

            isShowing = false;
        }

        public void Update()
        {
            if (_isActive)
            {
                if (PlayerActions_DataHolder.instance.timeSinceLastAction < hintEvery_Seconds) { Hide(); return; }
                if (isShowing == true) { return; }

                Show();
            }
            else
            {
                Hide();
            }
        }

        public async void Show()
        {
            isShowing = true;

            var position = GetSourcePosition();
            position.z = 0;
            animationAgent.transform.transform.position = position;
            animationAgent.transform.transform.localScale = Vector3.zero;

            animationAgent.transform.gameObject.SetActive(true);
            await animationAgent.transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack).AsyncWaitForCompletion();
            if(isShowing == false) { return; }

            await animationAgent.transform.DOMove(position, 0.5f).SetEase(Ease.OutBack).SetDelay(1f).AsyncWaitForCompletion();
            if (isShowing == false) { return; }
            await animationAgent.transform.DOScale(0.5f, 0.25f).SetEase(Ease.OutBack).SetDelay(0.25f).AsyncWaitForCompletion();
            if (isShowing == false) { return; }
            await animationAgent.transform.DOMove(GetTargetPosition(), 1).SetDelay(0.25f).AsyncWaitForCompletion();
            if (isShowing == false) { return; }
            await animationAgent.transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack).SetDelay(0.25f).AsyncWaitForCompletion();
            if (isShowing == false) { return; }

            animationAgent?.PlayAnimation("Draw");
            await AsyncHelper.DelayFloat(PlayerActions_DataHolder.instance?.drawAnimationDuration ?? 0);
            if (isShowing == false) { return; }
            if (enabled == false) { return; }

            await animationAgent.transform.DOScale(0, 0.25f).SetEase(Ease.OutBack).SetDelay(0.25f).AsyncWaitForCompletion();
            if (isShowing == false) { return; }
            animationAgent.gameObject.SetActive(false);

            PlayerActions_DataHolder.ResetTime();
            isShowing = false;
        }

        private void Hide()
        {
            if (animationAgent.gameObject.activeSelf == false) { return; }
            animationAgent.gameObject.SetActive(false);
            animationAgent.transform.DOKill();
            isShowing = false;
        }
    }
}