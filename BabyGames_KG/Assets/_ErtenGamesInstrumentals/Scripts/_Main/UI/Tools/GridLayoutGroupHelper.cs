using Helpers;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Helpers
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(GridLayoutGroup))]
    public class GridLayoutGroupHelper : MonoBehaviour, ISelfValidator
    {
        [Required]
        [SerializeField] protected RectTransform _parent;

        [Header("Componenets")]
        [Required]
        [SerializeField] protected GridLayoutGroup _gridLayout;

        [Header("Settings")]
        [SerializeField] protected float _horrizontalOffset = 0;
        [SerializeField] protected float _verticalOffset = 0;

        public virtual void Validate(SelfValidationResult result)
        {
            if (_parent == null && transform.parent != null) { _parent = transform.parent.GetComponentInParent<RectTransform>(); }
            if (_gridLayout == null) { _gridLayout = GetComponent<GridLayoutGroup>(); }
            SetGridLayoutSizes();
        }

        private void Update()
        {
            SetGridLayoutSizes();
        }

        [Button]
        protected virtual void SetGridLayoutSizes()
        {
            if (_parent == null) return;

            float horrizontal = NumberHelper.GetNumberFromPercentage(_parent.rect.size.x, _horrizontalOffset);
            float vertical = NumberHelper.GetNumberFromPercentage(_parent.rect.size.y, _verticalOffset);

            if (horrizontal == 0)
            {
                horrizontal = _gridLayout.cellSize.x;
            }

            if (vertical == 0)
            {
                vertical = _gridLayout.cellSize.y;
            }

            _gridLayout.cellSize = new Vector2(horrizontal, vertical);
        }
    }
}