using DG.Tweening;
using FX;
using Helpers;
using Services;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Modes.Puzzle
{
    public class Puzzle_DrawState : StateMachine_StateBase, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private Puzzle _puzzle;

        [SerializeField] private Drawable _overlay;
        [SerializeField] private float _percentageToWin;

        private Slider _slider;

        public override async Task Enter()
        {
            await base.Enter();
            FindFirstObjectByType<Slider>()?.gameObject.SetActive(true);
            if (_puzzle == null) { _puzzle = FindFirstObjectByType<Puzzle>(); }
            _overlay.Initialize(_puzzle.mainImage[0].sprite);
        }

        public override async Task Exit()
        {
            await base.Exit();
            _slider?.gameObject.SetActive(false);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _puzzle._soundPlayer.Play();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _puzzle._soundPlayer.Stop();
        }

        private bool _isDone = false;
        public override void Tick()
        {
            base.Tick();

            if (_overlay.percentageOfColoring > _percentageToWin)
            {
                OpenPuzzleState();
            }

            if (_overlay.isDrawing == false)
            {
                _puzzle._soundPlayer.Stop();
            }

            if (_slider == null)
            {
                _slider = FindObjectOfType<Slider>(true);
            }
            else
            {
                if (_slider.maxValue != 100) { _slider.maxValue = 100; }
                _slider.value = _overlay.percentageOfColoring;
            }
        }

        private async void OpenPuzzleState()
        {
            if (_isDone) { return; }
            _isDone = true;
            DiService.Get<StateEnd_FX>()?.DoFX();

            _overlay._spriteRenderer.DOFade(0, 1);
            await AsyncHelper.DelayFloat(1);

            _puzzle._soundPlayer.Stop();
            _puzzle._soundPlayer.clip = null;

            _nextState = _nextStateOnWin;
        }
    }
}
