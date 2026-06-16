using CoockingSalade;
using DG.Tweening;
using FX;
using Modes.Puzzle;
using System.Threading.Tasks;
using _Project._Modes.Puzzle.Scripts;
using CustomAttributes;
using UnityEngine;

namespace Coocking
{
    public class CookingSalade_Pizza_Sauce : CoockingSalade_StateBase
    {
        [Re_Fg_Co] public Transform skalka;
        [Re_Fg_Co] public Transform saucesPennel;
        [Re_Fg_Co] public Drawable drawable;
        [Re_Fg_Co] public DrawableP2D drawableP2D;

        [Space]
        [SerializeField] private CoockingPizza_Sauce _currentSauce;
        [SerializeField] private HintHand_Click _hintHand_Drag;
        [SerializeField] private HintHand_DrawDots _hintHand_DDrawDots;

        private bool _doCheckDrawable;

        public override async Task Enter()
        {
            await base.Enter();
            drawableP2D = drawable.GetComponent<DrawableP2D>();

            CoockingPizza_Sauce.onSelected += EnableDrawable;

            skalka.DOMoveY(10, 0).OnComplete(() =>
            {
                skalka.gameObject.SetActive(false);
            });
            saucesPennel.DOMoveY(0, 1);

            _hintHand_Drag = GetComponent<HintHand_Click>();
            _hintHand_DDrawDots = GetComponent<HintHand_DrawDots>();
            _hintHand_Drag?.SetIsActive(true);
            _hintHand_DDrawDots?.SetIsActive(false);
        }

        public override async Task Exit()
        {
            saucesPennel.DOMoveY(25f, 1);
            await base.Exit();
        }

        private void Update()
        {
            if (_doCheckDrawable == false) { return; }
            if (_currentSauce == null) { return; }
            if (drawableP2D.percentageOfColoring < 1) { return; }

            if (drawableP2D.percentageOfColoring < 10)
            {
                CompleteSauce();
            }
        }

        private async void EnableDrawable(CoockingPizza_Sauce selected)
        {
            _hintHand_Drag?.SetIsActive(false);
            _hintHand_DDrawDots?.SetIsActive(true);

            _currentSauce = selected;

            drawable.gameObject.SetActive(true);

            var sprite = selected.sauceSprite;

            foreach (var item in drawable.GetComponentsInChildren<SpriteRenderer>(true))
            {
                item.sprite = sprite;
            }

            foreach (var item in drawable.GetComponentsInChildren<SpriteMask>(true))
            {
                item.sprite = sprite;
            }

            drawableP2D.drawMode = DrawableP2D.DrawMode.Reveal;
            drawableP2D.Initialize(sprite);

            _doCheckDrawable = true;
        }

        public async override void ForceWin()
        {
            base.ForceWin();

            _doCheckDrawable = false;
            await drawableP2D.Complete();
            _nextState = _nextStateOnWin;
        }

        private async void CompleteSauce()
        {
            _hintHand_Drag?.SetIsActive(false);
            _hintHand_DDrawDots?.SetIsActive(false);

            _doCheckDrawable = false;
            await drawableP2D.Complete();

            _nextState = _nextStateOnWin;
        }
    }
}