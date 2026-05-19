using FX;
using Gameplay;
using Helpers;
using Modes.Puzzle;
using Services;
using Spine.Unity;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace WhoLivesWhere
{
    public class WLW_CollectingState : StateMachine_StateBase
    {
        [SerializeField] private SkeletonAnimation _background;
        [SerializeField] private WLW_TraktorIdentifier _traktorIdentifier;

        [SerializeField] private List<WLW_Character> _characters = new();

        [SerializeField] private HintHand_Drag _hintHand_Drag;

        public override async Task PreInittialize()
        {
            _background.AnimationName = "idle_start";

            _hintHand_Drag.objects.Clear();
            _hintHand_Drag.targets.Clear();

            foreach (var character in _characters)
            {
                await character.GetHintSprite();
            }

            foreach (var character in _characters)
            {
                var transformSeat = _traktorIdentifier.seats.Where(x => x.hasCharacter == false).GetRandom();
                var characterCopy = Instantiate(character.transform, transformSeat.holder.transform);
                await character.SetCopy(characterCopy);

                character.onSet += OnSetACharacter;

                _hintHand_Drag.objects.Add(character.transform);
                _hintHand_Drag.targets.Add(characterCopy);
            }

            foreach (var item in _background.GetComponentsInChildren<SkeletonPartsRenderer>())
            {
                if (item.name.Contains("cloud") == false) { continue; }

                var levitator = item.gameObject.AddComponent<Levitator>();

                levitator.rotationData._mode = Levitator.Vector3Data.Mode.Random;
                levitator.positionData._mode = Levitator.Vector3Data.Mode.Random;
                levitator.scaleData._mode = Levitator.Vector3Data.Mode.Random;

                levitator.rotationData.duration = new Vector2(20f, 25f);
                levitator.positionData.duration = new Vector2(20f, 25f);
                levitator.scaleData.duration = new Vector2(15f, 25f);

                levitator.positionData.valueRandomX = new Vector2(-5f, 5f);
                levitator.positionData.valueRandomY = new Vector2(-0.25f, 0.25f);
            }

            await base.PreInittialize();
        }

        public override async Task Enter()
        {
            await PreInittialize();
            await base.Enter();
            _hintHand_Drag.SetIsActive(true);
        }

        private void OnSetACharacter(WLW_Character character)
        {
            character.onSet -= OnSetACharacter;
            _characters.Remove(character);
            if (_characters.Count < 1)
            {
                _hintHand_Drag?.SetIsActive(false);

                _nextState = WLW_HomeSetState.allInstances.First();
                if (_nextState == null)
                {
                    _nextState = _nextStateOnWin;
                }
            }

            _hintHand_Drag.objects.Remove(character.transform);
            _hintHand_Drag.targets.Remove(character.newCopy.transform);
            _hintHand_Drag.objects.RemoveNulls();
            _hintHand_Drag.targets.RemoveNulls();
        }
    }
}