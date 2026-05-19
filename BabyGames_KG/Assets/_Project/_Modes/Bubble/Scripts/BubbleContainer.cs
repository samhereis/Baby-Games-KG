using CustomAttributes;
using DG.Tweening;
using FX;
using Helpers;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Bubble
{
    public class BubbleContainer : MonoBehaviour
    {
        [SerializeField] private BubbleContainerData _bubbleConntainerData;
        [SerializeField] private Image[] _iconImages;
        [SerializeField] private Slider _slider;
        [SerializeField] private List<RectTransform> _positions;

        [Fg_De, SerializeField] private HintHand_Click hintHandClick;

        public void Construct(BubbleContainerData data, HintHand_Click hintHandClick)
        {
            _bubbleConntainerData = data;
            _bubbleConntainerData.positions = _positions;

            this.hintHandClick = hintHandClick;

            foreach (var item in _iconImages)
            {
                item.sprite = _bubbleConntainerData.bubbleUnits[0].icon;
            }

#if UNITY_EDITOR
            //_bubbleConntainerData.capacity = 1;
#endif

            _slider.minValue = -_bubbleConntainerData.capacity;
            _slider.maxValue = 0;
            _slider.value = -_bubbleConntainerData.capacity;
        }

        public void Spawn(BubbleUnit bubbleUnitToSpawn = null)
        {
            if (bubbleUnitToSpawn == null) { bubbleUnitToSpawn = _bubbleConntainerData.bubbleUnits.GetRandom(); }
            else { bubbleUnitToSpawn.isEmpty = true; }

            Vector3 position = new Vector3(new Vector2(300, Screen.width - 300).GetRandom(), -250, 0);
            var bubbleUnit = Instantiate(bubbleUnitToSpawn, position, Quaternion.identity, bubbleUnitToSpawn.isEmpty ? transform.root : transform.parent.parent);
            bubbleUnit.Construct(_bubbleConntainerData, hintHandClick);

            _bubbleConntainerData.currentBubbleUnits.Add(bubbleUnit);

            bubbleUnit.onAutoDestroy += OnAutoDestroy;
            bubbleUnit.onBlew += OnBlew;
        }

        private async void OnBlew(BubbleUnit unit)
        {
            _bubbleConntainerData.currentBubbleUnits.Remove(unit);
            _bubbleConntainerData.OnCatch(unit);

            await unit.Complete();

            _bubbleConntainerData.capacity -= 1;
            _slider.DOValue(0 - _bubbleConntainerData.capacity, 2);

            if (_bubbleConntainerData.capacity < 1)
            {
                _bubbleConntainerData.Complete();
            }
        }

        private void OnAutoDestroy(BubbleUnit unit)
        {
            _bubbleConntainerData.currentBubbleUnits.Remove(unit);
        }
    }
}