using Sirenix.OdinInspector;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class HorizontalCarousel : MonoBehaviour
    {
        [SerializeField] private RectTransform[] _items;
        [SerializeField] private float _spacing = 650f;
        [SerializeField] private float _speed = 150f;

        [SerializeField] private Vector3 _initial;
        [SerializeField] private Vector3 _end;

        [Button]
        private void Find()
        {
            _items = GetComponentsInChildren<RectTransform>(true)
                .Where(x => x != GetComponent<RectTransform>() && x.GetComponent<Button>() != null)
                .ToArray();
        }

        private void Start()
        {
            ArrangeItems();
        }

        private void Update()
        {
            MoveItems();
        }

        [Button]
        private void ArrangeItems()
        {
            Find();
            if (_items == null || _items.Length == 0) return;

            float totalWidth = (_items.Length - 1) * _spacing;
            float startX = -totalWidth / 2f;

            for (int i = 0; i < _items.Length; i++)
            {
                var p = _items[i].anchoredPosition;
                _items[i].anchoredPosition = new Vector2(startX + i * _spacing, p.y);
            }

            _initial = _items[0].anchoredPosition;
            _end = _items.Last().anchoredPosition;

            MoveItems();
        }

        private float CycleWidth()
        {
            // One full loop distance between identical neighbors after wrapping
            return _spacing * _items.Length;
        }

        [Button]
        private void MoveItems()
        {
            if (_items == null || _items.Length == 0) return;

            float delta = _speed * Time.deltaTime;
            float cycle = CycleWidth();

            // Move all first
            foreach (var item in _items)
            {
                item.anchoredPosition += Vector2.right * delta;
            }

            // Then wrap without overlapping by shifting a full cycle
            if (_speed >= 0f)
            {
                float wrapRight = _end.x + _spacing * 0.5f;      // a small buffer
                foreach (var item in _items)
                {
                    if (item.anchoredPosition.x > wrapRight)
                    {
                        var p = item.anchoredPosition;
                        p.x -= cycle;                            // shift left by one full cycle
                        item.anchoredPosition = p;
                    }
                }
            }
            else
            {
                float wrapLeft = _initial.x - _spacing * 0.5f;   // a small buffer
                foreach (var item in _items)
                {
                    if (item.anchoredPosition.x < wrapLeft)
                    {
                        var p = item.anchoredPosition;
                        p.x += cycle;                            // shift right by one full cycle
                        item.anchoredPosition = p;
                    }
                }
            }
        }
    }
}
