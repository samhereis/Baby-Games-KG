using _Project.Scripts.Sound;
using Modes.Puzzle;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace CarTuning
{
    public class Princess_WearubgUI_Base : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
    {
        public Princess_DressBase _wearing;

        [SerializeField] protected bool _isDragging;

        protected Gameplay_GameState_InterestGameGirl_Model _model;

        public virtual void Construct(Gameplay_GameState_InterestGameGirl_Model model)
        {
            _model = model;
        }

        protected virtual void Update()
        {
            if (_isDragging == false) { return; }
            if (_wearing == null) { return; }

            var position = Camera.main.ScreenToWorldPoint(Pointer.current.position.ReadValue());
            position.z = 0;

            _wearing.Move(position);
        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            Drop();
        }

        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            if (_wearing == null) { return; }

            _wearing.SetVisible(true);

            _isDragging = true;

            Sound_FX.Play_Static(Sound_Effect.StartDrag);
        }

        public virtual void OnDrag(PointerEventData eventData)
        {
            _isDragging = true;
        }

        public virtual void OnEndDrag(PointerEventData eventData)
        {
            if (_wearing == null) { return; }
            if (_isDragging == false) { return; }

            _isDragging = false;
            Drop();
        }

        private void Drop()
        {
            if (_wearing == null) { return; }

            _isDragging = false;

            _wearing.Drop();
            _model.currentDress.ChangeValue(_wearing);
        }
    }
}