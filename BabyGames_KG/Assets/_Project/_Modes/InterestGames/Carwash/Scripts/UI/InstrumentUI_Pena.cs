using CustomAttributes;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Carwash
{
    public class InstrumentUI_Pena : InstrumentUI_Base
    {
        [SerializeField] private Pena_Identifier _penaPrefab;
        [SerializeField] private RectTransform _holder;
        [SerializeField] private float _spawnPositionDistance = 1;

        [Fg_De, SerializeField] private List<Transform> _spawnedPosition = new();

        private void OnEnable()
        {
            _spawnedPosition.Clear();
        }

        private void OnDisable()
        {
            _spawnedPosition.Clear();
        }

        public override void OnDrag(PointerEventData eventData)
        {
            _holder.position = eventData.position;

            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(eventData.position);
            mousePosition.z = 0;

            foreach (var x in _car.generatePointsInBox.pointObjects)
            {
                if (_spawnedPosition.Contains(x) == false && Vector3.Distance(mousePosition, x.position) < _spawnPositionDistance)
                {
                    Spawn(x);
                    break;
                }
            }

            foreach (var x in _car.generatePointsInBox.pointObjects_Secondary)
            {
                if (_spawnedPosition.Contains(x) == false && Vector3.Distance(mousePosition, new Vector3(x.position.x, x.position.y, 0)) < _spawnPositionDistance)
                {
                    Spawn(x);
                    break;
                }
            }

            base.OnDrag(eventData);
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            base.OnEndDrag(eventData);
            PutBack();

            if (_spawnedPosition.Count >= _car.generatePointsInBox.pointObjects.Count / 1.5f)
            {
                _car.generatePointsInBox.pointObjects.ForEach(x =>
                {
                    if (_spawnedPosition.Contains(x) == false)
                    {
                        Spawn(x);
                    }
                });

                _car.generatePointsInBox.pointObjects_Secondary.ForEach(x =>
                {
                    if (_spawnedPosition.Contains(x) == false)
                    {
                        Spawn(x);
                    }
                });

                onComplete?.Invoke(this);
            }
        }

        public override void PutBack()
        {
            base.PutBack();
            _holder.DOLocalMove(Vector3.zero, 0.25f).SetEase(Ease.OutBack);
        }

        private void Spawn(Transform x)
        {
            var position = Instantiate(_penaPrefab, x.position, Quaternion.identity);
            position.transform.SetParent(x, false);
            position.transform.localPosition = Vector3.zero;

            _spawnedPosition.Add(x);
        }
    }
}