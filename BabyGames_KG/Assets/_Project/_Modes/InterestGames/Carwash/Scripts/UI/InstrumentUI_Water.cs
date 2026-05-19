using DG.Tweening;
using Spine.Unity;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Carwash
{
    public class InstrumentUI_Water : InstrumentUI_Base
    {
        [SerializeField] private RectTransform _holder;
        [SerializeField] private float _spawnPositionDistance = 1;

        [SerializeField] private List<Pena_Identifier> _penas = new();

        [SerializeField] private SkeletonGraphic _skeletonGraphic;
        [SerializeField] private string _startAnimation;
        [SerializeField] private string _actionAnimation;
        [SerializeField] private string _endAnimation;

        [SerializeField] private Vector3 _offset;

        private Vector3 _initialPosition;

        private void OnEnable()
        {
            _initialPosition = _holder.position;

            _skeletonGraphic.AnimationState.ClearTracks();
            _skeletonGraphic.AnimationState.SetAnimation(0, _endAnimation, false);
        }

        public override void OnBeginDrag(PointerEventData eventData)
        {
            _penas.Clear();

            _penas.AddRange(_car.generatePointsInBox.pointObjects.Select(x => x.GetComponentInChildren<Pena_Identifier>()).Where(x => x != null && x.isVisible).ToList());
            _penas.AddRange(_car.generatePointsInBox.pointObjects_Secondary.Select(x => x.GetComponentInChildren<Pena_Identifier>()).Where(x => x != null && x.isVisible).ToList());

            _skeletonGraphic.AnimationState.ClearTracks();
            _skeletonGraphic.AnimationState.AddAnimation(0, _startAnimation, false, 0);
            _skeletonGraphic.AnimationState.AddAnimation(1, _actionAnimation, true, 0);

            base.OnBeginDrag(eventData);
        }

        public override void OnDrag(PointerEventData eventData)
        {
            _holder.position = eventData.position;

            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Pointer.current.position.ReadValue());
            mousePosition.z = 0;
            _penas.Find(x =>
            {
                if (x == null) { return false; }
                if (x.isVisible == false) { return false; }

                if (Vector3.Distance(mousePosition + _offset, x.transform.position) < _spawnPositionDistance)
                {
                    x.GetComponentInChildren<Pena_Identifier>()?.Fade();
                    return true;
                }

                return false;
            });

            base.OnDrag(eventData);
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            base.OnEndDrag(eventData);

            _holder.DOLocalMove(Vector3.zero, 0.25f).SetEase(Ease.OutBack);

            _skeletonGraphic.AnimationState.ClearTracks();
            _skeletonGraphic.AnimationState.SetAnimation(0, _endAnimation, false);

            if (_penas.Where(x => { return x.isVisible; }).Count() < 10)
            {
                _penas.Clear();
                onComplete?.Invoke(this);
            }
        }
    }
}