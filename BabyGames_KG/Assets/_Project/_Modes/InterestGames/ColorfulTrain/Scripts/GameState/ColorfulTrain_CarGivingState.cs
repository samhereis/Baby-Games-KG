using DG.Tweening;
using FX;
using Helpers;
using Loggers;
using Services;
using Spine.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace ColorfulTrain
{
    public class ColorfulTrain_CarGivingState : ColorfulTrain_StateBase
    {
        public List<ColorfulTrain_WheellessCar> wheellessCars = new();
        public List<Transform> places = new();
        public List<ColorfulTrain_Wheel> doneWheels = new();

        public override async Task Enter()
        {
            GetComponent<HintHand_Drag>().SetIsActive(false);

            await base.Enter();

            places.Shuffle_Original();

            int index = 0;
            foreach (var item in wheellessCars)
            {
                var place = places.First();

                item.transform.SetParent(place, false);
                item.transform.localPosition = Vector3.zero;

                item.wheel.targetPlace = item.wheelPlace;
                Debug.Log($"{item.gameObject.name} + {item.wheelPlace}");

                item.wheel.isDropped = new("");

                item.missingWheel.gameObject.SetActive(false);

                item.wheel.onDropped -= OnWheelDropped;
                item.wheel.onDropped += OnWheelDropped;

                foreach (var partRenderer in item.GetComponentsInChildren<SkeletonPartsRenderer>(true))
                {
                    partRenderer.MeshRenderer.sortingOrder += index;
                }

                index += 100;

                item.gameObject.SetActive(false);
                item.transform.localScale = Vector3.zero;
            }

            wheellessCars[doneWheels.Count].gameObject.SetActive(true);
            wheellessCars[doneWheels.Count].transform.DOScale(1, 1);

            DiService.Get<StateEnd_FX>()?.DoFX();
            await AsyncHelper.DelayFloat(1f);
            await _model.train.Prepare();
            _model.controller.GoNext();
            await _model.train.Go();

            SetHintData();
        }

        public override async Task Exit()
        {
            await base.Exit();

            foreach (var item in wheellessCars)
            {
                item.wheel.onDropped -= OnWheelDropped;
            }
        }

        protected virtual async void OnWheelDropped(ColorfulTrain_Wheel wheel)
        {
            wheel.onDropped -= OnWheelDropped;
            doneWheels.SafeAdd(wheel);

            GetComponent<HintHand_Drag>().SetIsActive(false);

            foreach (var item in wheellessCars)
            {
                if (item.wheel.isDropped.value == true && item.isDone.value == false)
                {
                    await item.MarkDone();
                    item.transform.DOLocalMoveX(-7, 2);
                }
            }

            if (_model.train.seats.TrueForAll(x => x.isDropped.value))
            {
                Win();
            }
            else
            {
                try
                {
                    wheellessCars[doneWheels.Count].gameObject.SetActive(false);
                    wheellessCars[doneWheels.Count].gameObject.SetActive(true);
                    await wheellessCars[doneWheels.Count].transform.DOScale(1, 1).AsyncWaitForCompletion();
                }
                catch (Exception ex)
                {
                    CustomLogger.instance?.LogException(ex);

                    Win();
                    return;
                }
            }

            SetHintData();
        }

        private async void Win()
        {
            _nextState = _nextStateOnWin;

            await AsyncHelper.DelayFloat(2f);
            foreach (var place in places)
            {
                place.gameObject.SetActive(false);
            }
        }

        private void SetHintData()
        {
            try
            {
                GetComponent<HintHand_Drag>().objects = _model.train.seats.Where(x => x.targetPlace.gameObject.activeInHierarchy).Select(x => x.spriteRenderer.transform).ToList();
                GetComponent<HintHand_Drag>().targets = _model.train.seats.Where(x => x.targetPlace.gameObject.activeInHierarchy).Select(x => x.targetPlace).ToList();

                PlayerActions_DataHolder.ResetTime();
                GetComponent<HintHand_Drag>().SetIsActive(true);
            }
            catch (Exception ex)
            {

            }
        }
    }
}