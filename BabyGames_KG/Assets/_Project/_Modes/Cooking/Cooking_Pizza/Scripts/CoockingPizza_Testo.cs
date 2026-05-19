using DG.Tweening;
using Observables;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Coocking
{
    public class CoockingPizza_Testo : MonoBehaviour
    {
        public ObservableValue<bool> isCompleted = new("isCompleted");

        [SerializeField] private Transform _skalka;
        [SerializeField] private List<Testo_Data> _data = new();
        [SerializeField] private Testo_Data _currentTestoData;
        [SerializeField] private bool _isVertocal;
        [SerializeField] private BoxCollider _boxCollider;

        private Vector2 _previousTouchPosition;
        private bool _isTouching = false;
        private Vector3 _initialSkalkaPosition;

        [SerializeField] private float _smoothness = 2f;

        public void Initialize()
        {
            foreach (var item in _data)
            {
                item.QuickFade();
            }

            _initialSkalkaPosition = _skalka.localPosition;
            _boxCollider = GetComponent<BoxCollider>();
            _boxCollider.enabled = true;

            _currentTestoData = null;
            NextTestoState();
        }

        private void Update()
        {
            if (!_boxCollider.enabled) return;

            var pointer = Pointer.current;
            if (pointer == null) return;

            Vector2 screenPos = pointer.position.ReadValue();
            Ray ray = Camera.main.ScreenPointToRay(screenPos);

            if (pointer.press.wasPressedThisFrame)
            {
                if (_boxCollider.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
                {
                    _previousTouchPosition = Camera.main.ScreenToWorldPoint(screenPos);
                    _isTouching = true;
                }
            }
            else if (pointer.press.isPressed && _isTouching)
            {
                Vector2 currentWorldPos = Camera.main.ScreenToWorldPoint(screenPos);
                Vector2 direction = currentWorldPos - _previousTouchPosition;

                if (direction.sqrMagnitude > 0.0001f)
                {
                    if (_isVertocal)
                    {
                        currentWorldPos.x = _initialSkalkaPosition.x;
                        _skalka.DOLocalMove(currentWorldPos, _smoothness);
                        _skalka.DORotate(new Vector3(0f, 0f, 90f), _smoothness);
                    }
                    else
                    {
                        currentWorldPos.y = _initialSkalkaPosition.y;
                        _skalka.DOLocalMove(currentWorldPos, _smoothness);
                        _skalka.DORotate(new Vector3(0f, 0f, 0f), _smoothness);
                    }
                }

                _previousTouchPosition = currentWorldPos;

                if (_currentTestoData.isReady)
                    NextTestoState();
                else
                    _currentTestoData.Grow();
            }
            else if (pointer.press.wasReleasedThisFrame)
            {
                _isTouching = false;
            }
        }


        private void NextTestoState()
        {
            if (_data.TrueForAll(x => x.isReady == true))
            {
                isCompleted.ChangeValue(true);
                GetComponent<BoxCollider>().enabled = false;

                _skalka.DOLocalMoveY(25, 0.5f).SetEase(Ease.InBack);
                return;
            }

            bool wasFirst = _currentTestoData == null;

            _currentTestoData?.Disable();
            _currentTestoData = _data.First(x => x.isReady == false);
            _isVertocal = _currentTestoData.toolDirectionIsVertical;

            if (wasFirst)
            {
                _currentTestoData.Pop();
            }
            else
            {
                _currentTestoData.Enable();
            }
        }
    }

    [Serializable]
    public class Testo_Data
    {
        public string name = "1";

        public SpriteRenderer testo;
        public bool toolDirectionIsVertical;
        public bool canEnable;
        public bool canDisnable;
        public float initialScale;
        public float maxScale;

        public float growSpeed;
        public float growAnimationSpeed;

        public bool isReady = false;

        public void QuickFade()
        {
            testo.transform.localScale = Vector3.one * initialScale;
            testo.DOFade(0, 0);
        }

        public void Pop()
        {
            testo.transform.DOScale(initialScale, 1).SetEase(Ease.OutBack);
            testo.DOFade(1, 0);
        }

        public void Enable()
        {
            if (isReady) { return; }
            if (canEnable == false) { return; }

            testo.transform.localScale = Vector3.one * initialScale;
            testo.DOFade(1, 0);
        }

        public void Grow()
        {
            if (isReady) { return; }

            testo.transform.DOScale(testo.transform.localScale.x + growSpeed, growAnimationSpeed);

            isReady = testo.transform.localScale.x >= maxScale;
        }

        public void Disable()
        {
            if (canDisnable == false) { return; }

            testo.DOFade(0, 1);
        }
    }
}