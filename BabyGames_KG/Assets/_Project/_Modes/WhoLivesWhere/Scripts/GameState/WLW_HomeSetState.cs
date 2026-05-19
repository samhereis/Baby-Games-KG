using FX;
using Helpers;
using Modes.Puzzle;
using Services;
using Sirenix.OdinInspector;
using Spine;
using Spine.Unity;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace WhoLivesWhere
{
    public class WLW_HomeSetState : StateMachine_StateBase
    {
        public bool completed;

        [SerializeField] private SkeletonAnimation _background;
        [SerializeField] private WLW_TraktorIdentifier _traktorIdentifier;
        [SerializeField] private BoneFollower _houseBoneFollower;
        [SerializeField] private Transform _targetPosition;
        [SerializeField] private WLW_Character _correntAnswer;

        [SerializeField] private string _onSetAnimation = "door";

        [ShowInInspector] public static List<WLW_HomeSetState> allInstances = new List<WLW_HomeSetState>();
        [SerializeField] private static BoneFollower _lastBoneFollower;

        [SerializeField] private HintHand_Drag _hintHand_Drag;

        [ShowInInspector] private int _toIndex => allInstances.Count > 0 ? allInstances.IndexOf(this) + 1 : 0;

        protected override void Awake()
        {
            base.Awake();

            if (_houseBoneFollower == null) { _houseBoneFollower = GetComponentInChildren<BoneFollower>(); }
            _houseBoneFollower.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            _correntAnswer.onSet -= OnSet;
        }

        public override async Task Enable()
        {
            SetDragStatus(false);

            await AsyncHelper.DelayFloat(0.5f);
            await _traktorIdentifier.Prepare();
            _traktorIdentifier.Go();

            _houseBoneFollower.gameObject.SetActive(true);
            _houseBoneFollower.boneName = $"house_end";

            if (_lastBoneFollower != null) { _lastBoneFollower.SetBone($"house_start"); }
            _background.timeScale = 1;

            _background.AnimationState.Data.DefaultMix = 0;
            _background.ClearState();
            _background.AnimationState.SetAnimation(0, $"to_{_toIndex}", false);
            _background.AnimationState.Complete += OnAnimationEnded;

            await _correntAnswer.SetTargetPosition(_targetPosition, _targetPosition);

            _correntAnswer.onSet -= OnSet;
            _correntAnswer.onSet += OnSet;

            _correntAnswer.onDrop -= OnDrop;
            _correntAnswer.onDrop += OnDrop;

            await AsyncHelper.NextFrame();
        }

        public override Task Enter()
        {
            DiService.Get<StateEnd_FX>()?.DoFX();
            return base.Enter();
        }

        public override async Task Disable()
        {
            if (completed)
            {
                await AsyncHelper.DelayFloat(8f);
                await base.Disable();
            }
            else
            {
                await base.Disable();
            }
        }

        private void OnDrop(WLW_Character character)
        {
            _correntAnswer.onDrop -= OnDrop;
            SetDragStatus(false);
        }

        private async void OnSet(WLW_Character character)
        {
            SetDragStatus(false);

            _hintHand_Drag.SetIsActive(false);
            character.onSet -= OnSet;
            _correntAnswer.onSet -= OnSet;

            completed = true;

            _lastBoneFollower = _houseBoneFollower;

            if (string.IsNullOrEmpty(_onSetAnimation) == false &&
                _lastBoneFollower.GetComponentInChildren<SkeletonAnimation>(true) is SkeletonAnimation skeletonAnimation)
            {
                skeletonAnimation.AnimationState.ClearTracks();
                skeletonAnimation.AnimationState.SetAnimation(0, _onSetAnimation, false);
            }

            if (allInstances.Exists(x => x.completed == false))
            {
                await AsyncHelper.DelayFloat(2f);
                DiService.Get<StateEnd_FX>()?.DoFX();
                _nextState = allInstances.Where(x => x.completed == false).GetRandom();
            }
            else
            {
                FindFirstObjectByType<WLW_Controller>().Win();
            }
        }

        private void OnAnimationEnded(TrackEntry trackEntry)
        {
            SetDragStatus(true);

            _background.AnimationState.Complete -= OnAnimationEnded;

            _background.AnimationState.Data.DefaultMix = 0;
            _background.AnimationName = $"idle_{_toIndex}";

            _hintHand_Drag.SetIsActive(true);
            _hintHand_Drag.objects.Add(_correntAnswer.transform);
            _hintHand_Drag.targets.Add(_targetPosition);
        }

        private void SetDragStatus(bool drag)
        {
            foreach (var item in _traktorIdentifier.seats)
            {
                foreach (var asd in item?.TryGetAll_List<WLW_Character>())
                {
                    if (asd.gameObject == null) { continue; }
                    if (asd.gameObject.name.Contains("Clone")) { continue; }
                    asd?.SetDraggableStatus(drag);
                }
            }
        }
    }
}