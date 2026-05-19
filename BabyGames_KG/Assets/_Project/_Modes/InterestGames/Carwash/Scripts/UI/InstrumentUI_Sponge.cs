using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Carwash
{
    public class InstrumentUI_Sponge : InstrumentUI_Base
    {
        [SerializeField] private RectTransform _holder;
        [SerializeField] private float _spawnPositionDistance = 1;

        private Vector3 _initialPosition;

        [SerializeField] private List<Pena_Identifier> _penas = new();

        private void OnEnable()
        {
            _initialPosition = _holder.position;
        }

        public override void OnBeginDrag(PointerEventData eventData)
        {
            base.OnBeginDrag(eventData);

            _penas.Clear();
            _penas.AddRange(_car.generatePointsInBox.pointObjects.Select(x => x.GetComponentInChildren<Pena_Identifier>()).Where(x => x != null).ToList());
            _penas.AddRange(_car.generatePointsInBox.pointObjects_Secondary.Select(x => x.GetComponentInChildren<Pena_Identifier>()).Where(x => x != null).ToList());
        }

        public override void OnDrag(PointerEventData eventData)
        {
            _holder.position = eventData.position;

            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(eventData.position);
            mousePosition.z = 0;

            _penas.Find(x =>
            {
                if (Vector3.Distance(mousePosition, x.transform.position) < _spawnPositionDistance)
                {
                    x?.MakeDirty();
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

            if (_penas.Where(x => { return x.IsDirty(); }).Count() >= _penas.Count / 1.5f)
            {
                _penas.ForEach(x =>
                {
                    x?.MakeDirty();
                });

                onComplete?.Invoke(this);
            }
        }
    }
}