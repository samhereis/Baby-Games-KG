using _Project._Modes.Hiding.Scripts.State;
using FX;
using Helpers;
using Modes.Sorting;
using Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Hiding
{
    [Serializable]
    public class HidingWaveUnit
    {
        public bool isWaveUnitCompleted { get; private set; }
        public event Action<HidingWaveUnit> onCompleted;

        public HidingDraggable_Identifier draggable;
        public HidingDropZone_Identifier dropZone;

        public void OnCompleted()
        {
            isWaveUnitCompleted = true;
            onCompleted?.Invoke(this);
        }
    }


    [Serializable]
    public class HidingWave
    {
        public bool isWaveCompleted { get; private set; }
        public event Action<HidingWave> onCompleted;

        public List<HidingWaveUnit> hidingWaveDatas = new();

        public void OnCompleted()
        {
            isWaveCompleted = true;
            onCompleted?.Invoke(this);
        }
    }

    public class HidingController : MonoBehaviour
    {
        public Hiding_GameState_Model _model;
        public List<HidingWave> hidingWaves = new();
        public HintHand_Drag hintHand_Drag;

        public Task Initialize(Hiding_GameState_Model model)
        {
            _model = model;
            _model.currentWaveIndex = 0;

            foreach (var item in hidingWaves)
            {
                foreach (var item1 in item.hidingWaveDatas)
                {
                    item1.draggable.Construct(_model);
                }
            }

            SetNewWave();

            return Task.CompletedTask;
        }

        private void SetNewWave()
        {
            _model.currentWave = hidingWaves[_model.currentWaveIndex];

            foreach (var item in hidingWaves)
            {
                foreach (var item1 in item.hidingWaveDatas)
                {
                    item1.draggable.SetVisibility(item == _model.currentWave);
                }
            }

            foreach (var waveUnit in _model.currentWave.hidingWaveDatas)
            {
                waveUnit.onCompleted += OnDraggableCompleted;
            }
        }

        private void OnDraggableCompleted(HidingWaveUnit hidingWaveUnit)
        {
            hidingWaveUnit.dropZone.OnDropped();

            if (_model.currentWave.hidingWaveDatas.TrueForAll(x => x.isWaveUnitCompleted))
            {
                OnWaveCompleted(_model.currentWave);
            }

            hintHand_Drag?.objects.Remove(hidingWaveUnit.draggable.transform);
            hintHand_Drag?.targets.Remove(hidingWaveUnit.dropZone.transform);
        }

        private async void OnWaveCompleted(HidingWave hidingWave)
        {
            hintHand_Drag?.SetIsActive(false);

            _model.currentWaveIndex++;

            if (_model.currentWaveIndex >= hidingWaves.Count)
            {
                _model.currentWave = null;

                await AsyncHelper.DelayFloat(1f);
                _model.onFinish?.Invoke();
            }
            else
            {
                DiService.Get<StateEnd_FX>()?.DoFX();
                SetNewWave();
            }

            hidingWave.OnCompleted();
        }
    }
}