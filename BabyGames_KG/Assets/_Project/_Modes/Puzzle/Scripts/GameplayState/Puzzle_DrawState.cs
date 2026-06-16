using DG.Tweening;
using FX;
using Helpers;
using Services;
using System.Threading.Tasks;
using _Project._Modes.Puzzle.Scripts;
using CustomAttributes;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Modes.Puzzle
{
    public class Puzzle_DrawState : StateMachine_StateBase, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private Puzzle _puzzle;

        [Re_Fg_Co] public Drawable drawable;
        [Re_Fg_Co] public DrawableP2D drawableP2D;
        [SerializeField] private float _percentageToWin;

        private Slider _slider;

        public override async Task Enter()
        {
            await base.Enter();
            FindFirstObjectByType<Slider>()?.gameObject.SetActive(true);
            if (_puzzle == null) { _puzzle = FindFirstObjectByType<Puzzle>(); }
            drawableP2D = drawable.GetComponent<DrawableP2D>();
            drawableP2D.Initialize(_puzzle.mainImage[0].sprite);
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
            if (drawableP2D == null) { return; }

            base.Tick();

            if (drawableP2D.percentageOfColoring > _percentageToWin)
            {
                OpenPuzzleState();
            }

            if (drawableP2D.isDrawing == false)
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
                _slider.value = drawableP2D.percentageOfColoring;
            }
        }

        private async void OpenPuzzleState()
        {
            if (_isDone) { return; }
            _isDone = true;
            DiService.Get<StateEnd_FX>()?.DoFX();

            drawableP2D.Get<SpriteRenderer>().DOFade(0, 1);
            await AsyncHelper.DelayFloat(1);

            _puzzle._soundPlayer.Stop();
            _puzzle._soundPlayer.clip = null;

            _nextState = _nextStateOnWin;
        }
    }
}