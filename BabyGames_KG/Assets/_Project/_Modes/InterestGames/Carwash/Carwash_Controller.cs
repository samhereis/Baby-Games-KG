using Assets._Project._Modes.Carwash.Scripts.State;
using DG.Tweening;
using FX;
using Helpers;
using Modes.Puzzle;
using Spine;
using System.Linq;
using UnityEngine;

namespace Carwash
{
    public class Carwash_Controller : StateMachineBase
    {
        public HintHand_Click hintHand_Click;

        private Carwash_GameState_Model _model;

        public void Construct(Carwash_GameState_Model model)
        {
            _model = model;
            foreach (var item in _model.activity_Identifier.selecButtons)
            {
                item.Construct(_model);
            }

            foreach (var item in _allStates)
            {
                (item as CarwashState_Base).Construct(_model);
            }
        }

        public void Initialize()
        {
            _model.isWaitingForCarSelection = true;

            _model.onCarSelected += OnCarSelected;
            _model.onCarFinished += OnFinish;

            hintHand_Click.targets.Clear();

            hintHand_Click.targets.AddRange(_model.activity_Identifier.selecButtons.Where(x => x.car.isFinished == false).Select(x => x.transform));

            hintHand_Click.SetIsActive(true);
        }

        public async void OnCarSelected(CarSelecButton carSelecButton)
        {
            _model.isWaitingForCarSelection = false;

            hintHand_Click.SetIsActive(false);

            _model.onCarSelected -= OnCarSelected;

            _model.activity_Identifier._background.loop = false;
            _model.activity_Identifier._background.AnimationName = carSelecButton.animationName;
            _model.activity_Identifier._background.state.Complete += OnCompleted;

            await CarwashCameraFollow.instance.Follow(_model.activity_Identifier._background, carSelecButton.followBoneName);
            var offset = carSelecButton.cameraFollowOffset;
            TweeningHelper.TweenFloat(offset.x, 0, 3, (x) =>
            {
                offset.x = x;
                CarwashCameraFollow.instance.offset = offset;
            });
        }

        private async void OnCompleted(TrackEntry trackEntry)
        {
            CarwashCameraFollow.instance.StopFollow();

            _model.activity_Identifier._background.state.Complete -= OnCompleted;
            _model.onCarEntered?.Invoke(_model.currentCarSelection);

            await AsyncHelper.DelayFloat(1f);

            var position = _model.activity_Identifier.carwashPosition.position;
            position.y = 0;
            position.z = Camera.main.transform.position.z;

            Camera.main.transform.DOMove(position, 0);
            _model.currentCarSelection.car?.gameObject.SetActive(true);

            ChangeState(_startState);
        }

        private async void OnFinish()
        {
            _model.onCarFinished -= OnFinish;

            if (_model.activity_Identifier.cars.TrueForAll(x => x.isFinished))
            {
                _model.onFinish.Invoke();
            }
            else
            {
                await _currentState.Disable();
                await _currentState.Exit();
                _currentState = null;
                _model.onStartOver.Invoke();

                await AsyncHelper.DelayFloat(1f);
                Initialize();

                Camera.main.transform.DOMove(CarwashCameraFollow.instance.cameraInitialPosition, 0);

                var car = _model.currentCarSelection.car;

                await AsyncHelper.DelayFloat(1f);
                car.transform.DOScale(0, 1);
            }
        }
    }
}