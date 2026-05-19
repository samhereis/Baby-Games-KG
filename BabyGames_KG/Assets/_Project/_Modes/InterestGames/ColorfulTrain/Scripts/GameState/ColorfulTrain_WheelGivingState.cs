using DG.Tweening;
using FX;
using Helpers;
using Services;
using Spine.Unity;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace ColorfulTrain
{
    public class ColorfulTrain_WheelGivingState : ColorfulTrain_StateBase
    {
        public List<ColorfulTrain_WheellessCar> wheellessCars = new();
        public List<Transform> places = new();
        public List<ColorfulTrain_Wheel> doneWheels = new();

        public override async Task Enter()
        {
            await base.Enter();

            var wheels = new List<ColorfulTrain_Wheel>();
            var cars_temp = wheellessCars.Shuffle_Copy();

            int index = 0;
            foreach (var place in places)
            {
                var item = cars_temp.First();
                cars_temp.Remove(item);

                item.transform.SetParent(place, false);
                item.transform.localPosition = Vector3.zero;

                wheels.Add(item.wheel);
                item.wheel.targetPlace = item.wheelPlace;
                item.wheel.isDropped = new("");

                item.missingWheel.gameObject.SetActive(false);

                item.wheel.onDropped += OnWheelDropped;

                foreach (var partRenderer in item.GetComponentsInChildren<SkeletonPartsRenderer>(true))
                {
                    partRenderer.MeshRenderer.sortingOrder += index;
                }

                index += 100;

                place.gameObject.SetActive(false);
            }

            places[doneWheels.Count].gameObject.SetActive(true);

            DiService.Get<StateEnd_FX>()?.DoFX();
            await AsyncHelper.DelayFloat(0.75f);
            await _model.train.Prepare();
            _model.controller.GoNext();
            await _model.train.Go();

            foreach (var item in wheels) { item.objectJuicer.StartJamming(); }

            SetHintData();
        }

        protected virtual async void OnWheelDropped(ColorfulTrain_Wheel wheel)
        {
            DiService.Get<StateEnd_FX>()?.DoFX();

            wheel.onDropped -= OnWheelDropped;
            doneWheels.SafeAdd(wheel);

            GetComponent<HintHand_Drag>().SetIsActive(false);

            foreach (var item in wheellessCars)
            {
                if (item.wheel.isDropped.value == true && item.isDone.value == false)
                {
                    await item.MarkDone();
                    item.transform.DOLocalMoveX(-7, 2).OnComplete(() =>
                    {
                        item.gameObject.SetActive(false);
                    });
                }
            }

            if (_model.train.seats.TrueForAll(x => x.isDropped.value))
            {
                await AsyncHelper.DelayFloat(1f);
                foreach (var place in places)
                {
                    place.gameObject.SetActive(false);
                }

                _nextState = _nextStateOnWin;
            }
            else
            {
                places[doneWheels.Count].gameObject.SetActive(true);

                await _model.train.Prepare();
                _model.controller.GoNext();
                await _model.train.Go();

                SetHintData();
            }
        }

        protected virtual void SetHintData()
        {
            GetComponent<HintHand_Drag>().objects = _model.train.seats.Where(x => x.targetPlace.gameObject.activeInHierarchy).Select(x => x.spriteRenderer.transform).ToList();
            GetComponent<HintHand_Drag>().targets = _model.train.seats.Where(x => x.targetPlace.gameObject.activeInHierarchy).Select(x => x.targetPlace).ToList();

            PlayerActions_DataHolder.ResetTime();
            GetComponent<HintHand_Drag>().SetIsActive(true);
        }

        public override void ForceWin()
        {
            _model.train.seats[doneWheels.Count].dropDistance = 50;
            _model.train.seats[doneWheels.Count].Place(false);
        }
    }
}