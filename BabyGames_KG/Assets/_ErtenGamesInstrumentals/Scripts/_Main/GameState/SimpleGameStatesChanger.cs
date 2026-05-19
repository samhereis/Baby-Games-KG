using System;
using UnityEngine;

namespace GameState
{
    [Serializable]
    public class SimpleGameStatesChanger : IGameStateService
    {
        [field: SerializeField] public IGameState currentGameState { get; private set; }

        public virtual void ChangeState(IGameState gameState)
        {
            currentGameState?.Exit();

            currentGameState = gameState;
            currentGameState.Enter();
        }
    }
}