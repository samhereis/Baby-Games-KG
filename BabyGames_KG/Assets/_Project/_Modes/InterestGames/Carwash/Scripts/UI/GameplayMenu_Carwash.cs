using Assets._Project._Modes.Carwash.Scripts.State;
using DG.Tweening;
using Helpers;
using UI.Menus;
using UnityEngine;
using UnityEngine.UI;

namespace Carwash
{
    public class GameplayMenu_Carwash : MenuBase
    {
        public Button backButton;
        public RectTransform panelHolder;

        public Image fade;


        private Carwash_GameState_Model _model;

        public void Construct(Carwash_GameState_Model model)
        {
            _model = model;
        }

        public override void Enable(float? duration = null)
        {
            base.Enable(duration);

            _model.onCarEntered += OnCarEntered;
            _model.onStartOver += OnStartOver;
            _model.onCarFinished += OnCarFinished;

            panelHolder?.DOAnchorPos3DY(-1000, 0);
        }

        public override void Disable(float? duration = null)
        {
            base.Disable(duration);

            _model.onCarEntered -= OnCarEntered;
            _model.onStartOver -= OnStartOver;
            _model.onCarFinished -= OnCarFinished;
        }

        private async void OnCarEntered(CarSelecButton button)
        {
            await fade.DOColor(Color.black, 1).AsyncWaitForCompletion();
            fade.DOFade(0, 1);
            await AsyncHelper.DelayFloat(1f);
            panelHolder?.DOAnchorPos3DY(200, 0.25f).SetEase(Ease.OutBack).AsyncWaitForCompletion();
        }

        private async void OnStartOver()
        {
            await fade.DOColor(Color.black, 1).AsyncWaitForCompletion();
            fade.DOFade(0, 1);
        }

        private void OnCarFinished()
        {
            panelHolder?.DOAnchorPos3DY(-1000, 0.25f).SetEase(Ease.OutBack);
        }
    }
}