using Assets._Project._Modes.Carwash.Scripts.State;
using CustomAttributes;
using DG.Tweening;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace CarTuning
{
    public class CarTuning_PartUI_Base : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
    {
        public CarPart_Base _carPart;
        public CarPart_Base _carPart_Copy;

        [SerializeField] protected bool _isDragging;

        protected CarTuning_GameState_Model _model;

        [Fg_De, SerializeField] private CarPart_Base _carPart_Temp;

        public virtual void Construct(CarTuning_GameState_Model model)
        {
            _model = model;
        }

        public void Initialize()
        {
            _carPart_Copy = _carPart.second;
        }

        protected virtual void Update()
        {
            if (_isDragging == false) { return; }
            if (_carPart_Temp == null) { return; }

            var position = Camera.main.ScreenToWorldPoint(Pointer.current.position.ReadValue());
            position.z = 0;

            _carPart_Temp.Move(position);
            _carPart_Temp.SetVisible(true);
        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            Drop();
        }

        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            if (_carPart == null) { return; }
            _carPart_Temp = Instantiate(_carPart, _carPart.transform.parent);

            _isDragging = true;

            CompleteButton.instance?.actionsOnComplete.Add(Hide);
        }

        public virtual void OnDrag(PointerEventData eventData)
        {
            _isDragging = true;
        }

        public virtual void OnEndDrag(PointerEventData eventData)
        {
            CompleteButton.instance?.actionsOnComplete.Remove(Hide);

            if (_carPart_Temp == null) { return; }
            if (_isDragging == false) { return; }

            _isDragging = false;
            Drop();
        }

        private void Drop()
        {
            if (_carPart_Temp != null)
            {
                var position = Camera.main.ScreenToWorldPoint(Pointer.current.position.ReadValue());
                position.z = 0;

                if (_carPart.second != null && Vector3.Distance(position, _carPart.transform.position) > Vector3.Distance(position, _carPart.second.transform.position))
                {
                    _carPart.second.Move(position);
                    _carPart.second.SetVisible(true);
                    _carPart.second.Drop();
                    _model.currentCarPart.ChangeValue(_carPart.second);
                }
                else
                {
                    _carPart.Move(position);
                    _carPart.SetVisible(true);
                    _carPart.Drop();
                    _model.currentCarPart.ChangeValue(_carPart);
                }

                Destroy(_carPart_Temp.gameObject);
            }
            else
            {
                _carPart?.Drop();
                _model.currentCarPart.ChangeValue(_carPart);
            }
        }

        private async Task Hide()
        {
            await _carPart_Temp.transform.DOScale(0, 0.25f).AsyncWaitForCompletion();
            Destroy(_carPart_Temp.gameObject);
            CompleteButton.instance?.actionsOnComplete.Remove(Hide);
        }
    }
}