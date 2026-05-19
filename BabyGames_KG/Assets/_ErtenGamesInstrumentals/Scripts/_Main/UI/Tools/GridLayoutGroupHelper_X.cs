using Helpers;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Helpers
{
    [RequireComponent(typeof(GridLayoutGroup))]
    public class GridLayoutGroupHelper_X : GridLayoutGroupHelper
    {
        protected override void SetGridLayoutSizes()
        {
            if (_parent == null) return;

            float horrizontal = NumberHelper.GetNumberFromPercentage(_parent.rect.size.x, _horrizontalOffset);
            float vertical = NumberHelper.GetNumberFromPercentage(_parent.rect.size.x, _verticalOffset);

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