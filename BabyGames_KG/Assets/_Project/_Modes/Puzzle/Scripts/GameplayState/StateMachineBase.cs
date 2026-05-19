using CustomAttributes;
using Loggers;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Modes.Puzzle
{
    public class StateMachineBase : MonoBehaviour
    {
        [SerializeField, Fg_Se] protected StateMachine_StateBase _startState;

        [SerializeField, Fg_De] protected StateMachine_StateBase _currentState;
        [SerializeField, Fg_De] protected List<StateMachine_StateBase> _allStates = new();
        [SerializeField, Fg_De] protected bool _isBusy = false;

        [Button]
        protected virtual void FindStates()
        {
            _allStates = GetComponentsInChildren<StateMachine_StateBase>(true).ToList();
        }

        protected virtual void Update()
        {
            if (_currentState == null) { return; }
            if (_isBusy == true) { return; }

            _currentState?.Tick();

            StateMachine_StateBase nextState = _currentState.GetNextState();
            if (nextState != null) { ChangeState(nextState); }
        }

        protected virtual async void ChangeState(StateMachine_StateBase newState)
        {
            _isBusy = true;
            try
            {
                foreach (var item in _allStates) { item.Disable(); }

                if (_currentState != null) { await _currentState.Exit(); }

                _currentState = newState;
                _currentState.gameObject.SetActive(true);
                await _currentState.Enter();
            } catch (Exception ex)
            {
                CustomLogger.instance?.LogException(ex);
            } finally
            {
                _isBusy = false;
            }
        }
    }
}