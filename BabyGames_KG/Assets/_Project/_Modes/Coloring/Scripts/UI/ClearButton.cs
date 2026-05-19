using DG.Tweening;
using Modes.Coloring;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityUtils;

namespace Modes.Coloring
{
    public class ClearButton : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        private Gameplay_GameState_Coloring_Model _model;

        [Space]
        [SerializeField] private RectTransform _clearRectTransform;
        [SerializeField] private RectTransform _clearTraceRectTransform;

        [Space]
        [SerializeField] private List<RectTransform> _arrowsRectTransform;

        private Vector2 _deltaPosition;
        private RectTransform _touchedObjectRect;
        private Vector2 _initClearButtonPosition;

        private float TargetLongY => ScreenUtils.ScreenHeightUI(0.280f);

        private void Start()
        {
            _clearTraceRectTransform.GetComponent<CanvasGroup>().alpha = 0;
            _initClearButtonPosition = _clearRectTransform.anchoredPosition;

            foreach (var arrow in _arrowsRectTransform) { arrow.gameObject.SetActive(false); }

            _clearRectTransform.GetComponent<Button>().interactable = true;
            _clearRectTransform.GetComponent<Button>().enabled = false;
        }

        public void SetModel(Gameplay_GameState_Coloring_Model model)
        {
            _model = model;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.pointerCurrentRaycast.gameObject.name != _clearRectTransform.name) { return; }

            foreach (var arrow in _arrowsRectTransform) { arrow.gameObject.SetActive(true); }

            _touchedObjectRect = _clearRectTransform;
            _deltaPosition = eventData.position;
            _clearTraceRectTransform.GetComponent<CanvasGroup>().alpha = 1;

            ObjectTap();
        }

        private void ObjectTap()
        {
            _touchedObjectRect.DOKill();
            _touchedObjectRect.DOScale(Vector3.one * 1.05f, 0.1f).OnComplete(() =>
            {
                _touchedObjectRect.DOScale(Vector3.one, 0.1f);
            });
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_touchedObjectRect == null) return;

            var positonMoved = eventData.position;

            float moveY = (positonMoved.y - _deltaPosition.y) / (Screen.height / ScreenUtils.ScreenHeightUI());
            if (_initClearButtonPosition.y > moveY || TargetLongY < moveY) return;

            _touchedObjectRect.anchoredPosition = new Vector2(0, moveY);

            Vector3 tapPos = _touchedObjectRect.transform.position;
            CheckPosition();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (_touchedObjectRect == null) return;

            var positonMoved = eventData.position;
            Transform transformPos = _touchedObjectRect.transform;

            float targetPos = 270;

            if (_clearRectTransform.anchoredPosition.y > targetPos)
            {
                _clearRectTransform.GetComponent<Image>().raycastTarget = false;

                _touchedObjectRect = null;

                _clearRectTransform.DOAnchorPos3DY(TargetLongY, 0.2f).OnComplete(() =>
                {
                    _model?.onClearRequested?.Invoke();
                });
                return;
            }

            float animTime = GetAnimationTime(_clearRectTransform.anchoredPosition, Vector2.zero);

            StartCoroutine(ReturnArrow(animTime));
            _clearRectTransform.GetComponent<Image>().raycastTarget = false;
            _clearRectTransform.DOAnchorPos(Vector2.zero, animTime)
                .OnUpdate(() => ShowArrowsBack(transformPos))
                .OnComplete(() =>
                {
                    _clearRectTransform.GetComponent<Image>().raycastTarget = true;
                    _clearTraceRectTransform.GetComponent<CanvasGroup>().alpha = 0;
                    foreach (var arrow in _arrowsRectTransform)
                        arrow.gameObject.SetActive(false);
                });

            _touchedObjectRect = null;
        }

        private IEnumerator ReturnArrow(float time)
        {
            time -= 0.1f;
            while (time > 0)
            {
                yield return new WaitForEndOfFrame();
                time -= Time.deltaTime;
                CheckPosition();
            }
        }

        private void CheckPosition()
        {
            foreach (var arrow in _arrowsRectTransform)
            {
                if (_clearRectTransform.anchoredPosition.y > arrow.anchoredPosition.y)
                    arrow.gameObject.SetActive(false);
                else
                    arrow.gameObject.SetActive(true);
            }
        }

        private void ShowArrowsBack(Transform transformPos)
        {
            foreach (var arrow in _arrowsRectTransform)
            {
                if (arrow.transform.position.y > transformPos.position.y)
                    arrow.GetComponent<Image>().enabled = true;
            }
        }

        private float GetAnimationTime(Vector3 startPoint, Vector3 endPoint, float speed = 500)
        {
            float animTime = Vector2.Distance(startPoint, endPoint) / speed;
            return animTime;
        }
    }
}