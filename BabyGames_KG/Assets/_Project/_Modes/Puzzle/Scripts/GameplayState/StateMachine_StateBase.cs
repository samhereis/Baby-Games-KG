using Sirenix.OdinInspector;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Modes.Puzzle
{
    public abstract class StateMachine_StateBase : MonoBehaviour
    {
        protected CancellationToken _dct;

        [FoldoutGroup("Base/Events")] public UnityEvent onEnter;
        [FoldoutGroup("Base/Events")] public UnityEvent onExit;

        [SerializeField, FoldoutGroup("Base")] protected StateMachine_StateBase _nextState;

        [SerializeField, FoldoutGroup("Base")] protected StateMachine_StateBase _nextStateOnWin;

        protected virtual void Awake()
        {
            _dct = destroyCancellationToken;
        }

        public async virtual Task PreInittialize()
        {
            await Exit();
        }

        public virtual StateMachine_StateBase GetNextState()
        {
            return _nextState;
        }

        public virtual Task Enable()
        {
            gameObject.SetActive(true);

            return Task.CompletedTask;
        }

        public virtual async Task Enter()
        {
            _nextState = null;

            await Enable();
            onEnter?.Invoke();
        }

        public virtual Task Exit()
        {
            _nextState = null;

            onExit?.Invoke();

            return Task.CompletedTask;
        }

        public virtual Task Disable()
        {
            gameObject.SetActive(false);

            return Task.CompletedTask;
        }

        public virtual void Tick()
        {
            if (Keyboard.current.spaceKey.wasReleasedThisFrame)
            {
                ForceWin();
            }
        }

        [Button]
        public virtual void ForceWin()
        {

        }
    }
}