using Coocking;
using DG.Tweening;
using Helpers;
using Identifiers;
using Loggers;
using Modes.Puzzle;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Gameplay;
using UnityEngine;

namespace CoockingSalade
{
    public class Coocking_Controller : StateMachineBase
    {
        public static Coocking_Controller instance { get; private set; }

        [FoldoutGroup("Settings")] public float fingetYOffset = 0.25f;

        [FoldoutGroup("Backgrounds")] public SpriteRenderer curtain;
        [FoldoutGroup("Backgrounds")] public List<SpriteRenderer> backgrounds = new();
        [FoldoutGroup("Backgrounds")] public int currentBackground = 0;

        public Panel_World panel_World;

        private Coocking_GameState_Model _model;

        private async void Awake()
        {
            instance = this;
        }

        public void Construct(Coocking_GameState_Model model)
        {
            _model = model;
        }

        [Button]
        public async Task Initialize()
        {
            _currentState = null;

            foreach (var item in _allStates)
            {
                if (item is CoockingSalade_StateBase) { (item as CoockingSalade_StateBase).Construct(_model); }
                if (item is CoockingBurger_StateBase) { (item as CoockingBurger_StateBase).Construct(_model, this); }
                if (item is CoockingIceCream_StateBase) { (item as CoockingIceCream_StateBase).Construct(_model); }
                if (item is CoockingFeeding_StateBase) { (item as CoockingFeeding_StateBase).Construct(_model); }
            }

            ChangeState(_startState);
            await ShowCurtain(false);
        }

        [Button]
        public async Task ChangeBackground(int index)
        {
            try
            {
                currentBackground = index;
                for (int i = 0; i < backgrounds.Count; i++)
                {
                    var toShow = currentBackground == i;
                    backgrounds[i].FadeWithActiveStatus(toShow);
                }
            } catch (Exception ex)
            {
                CustomLogger.instance?.LogException(ex);
                ;
            }

            await AsyncHelper.DelayFloat(0.5f);
        }

        [Button]
        public async Task ShowCurtain(bool isVisible, float duration = 0.5f)
        {
            try
            {
                if (curtain == null) { return; }

                await curtain?.FadeWithActiveStatus(isVisible, duration).AsyncWaitForCompletion();
                curtain?.gameObject.SetActive(isVisible);
            } catch (Exception ex)
            {
                CustomLogger.instance?.LogException(ex);
            }
        }

        [Button]
        private void Do()
        {
            foreach (var item in GetComponentsInChildren<Dropable_Basic>())
            {
                item.dropParticleScale = 3;
                item.dragParticleScale = 1.5f;
            }

            foreach (var item in GetComponentsInChildren<Dropable_General>())
            {
                item.dropParticleScale = 3;
                item.dragParticleScale = 1.5f;
            }
        }
    }
}