using DG.Tweening;
using Spine.Unity;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Carwash
{
    public class InstrumentUI_Polishing : InstrumentUI_Base
    {
        public bool isActive = true;

        [SerializeField] private RectTransform _holder;
        [SerializeField] private float _spawnPositionDistance = 1;

        private Vector3 _initialPosition;

        [SerializeField] private List<Pena_Identifier> _penas = new();

        [Space]
        [SerializeField] private SkeletonAnimation _blikPrefab;

        private List<Transform> _penasToIgnore = new();

        private void OnEnable()
        {
            _initialPosition = _holder.position;
            _penasToIgnore.Clear();
        }

        public override void OnBeginDrag(PointerEventData eventData)
        {
            if (isActive == false) { return; }

            base.OnBeginDrag(eventData);

            _penas.Clear();
            _penas.AddRange(_car.generatePointsInBox.pointObjects.Select(x => x.GetComponentInChildren<Pena_Identifier>()).Where(x => x != null).ToList());
            _penas.AddRange(_car.generatePointsInBox.pointObjects_Secondary.Select(x => x.GetComponentInChildren<Pena_Identifier>()).Where(x => x != null).ToList());
        }

        public override void OnDrag(PointerEventData eventData)
        {
            if (isActive == false) { return; }

            _holder.position = eventData.position;

            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(eventData.position);
            mousePosition.z = 0;

            _penas.ForEach(x =>
            {
                if (Vector3.Distance(mousePosition, x.transform.position) < _spawnPositionDistance)
                {
                    if (_penasToIgnore.Contains(x.transform) == false)
                    {
                        x?.Fade();

                        var copy = Instantiate(_blikPrefab, x.transform.position, Quaternion.identity);
                        copy.loop = false;
                        copy.AnimationName = "action";
                        Destroy(copy.gameObject, 5);

                        _penasToIgnore.Add(x.transform);

                        x.isPolished = true;
                    }
                }
            });

            base.OnDrag(eventData);
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            if (isActive == false) { return; }

            base.OnEndDrag(eventData);

            _holder.DOLocalMove(Vector3.zero, 0.25f).SetEase(Ease.OutBack);
            var invisibleOnes = _penas.Where(x => x.isPolished == true).ToList();

            if (invisibleOnes.Count > _penas.Count / 1.5f)
            {
                isActive = false;

                _penas.ForEach(x =>
                {
                    x?.Fade();
                });

                if (_car.blik != null)
                {
                    _car.blik.loop = false;
                    _car.blik.AnimationName = "action";
                }

                onComplete?.Invoke(this);
            }
        }
    }
}