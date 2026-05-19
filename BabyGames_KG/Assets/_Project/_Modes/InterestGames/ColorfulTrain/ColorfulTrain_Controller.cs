using Helpers;
using Modes.Puzzle;
using Sirenix.OdinInspector;
using Spine.Unity;
using System.Threading.Tasks;

namespace ColorfulTrain
{
    public class ColorfulTrain_Controller : StateMachineBase
    {
        public SkeletonAnimation background;

        private ColorfulTrain_GameState_Model _model;

        private int _goindex = 0;

        [Button]
        public async Task Setup()
        {
            await background.Separate();

            foreach (var item in background.GetComponentsInChildren<SkeletonPartsRenderer>())
            {
                item.MeshRenderer.sortingLayerName = "Spine";
            }
        }

        public void Construct(ColorfulTrain_GameState_Model model)
        {
            _model = model;

            foreach (var item in _allStates)
            {
                (item as ColorfulTrain_StateBase).Construct(model);
            }
        }

        public void Initialize()
        {
            if (_startState == null) { _startState = _allStates[0]; }

            ChangeState(_startState);

            background.AnimationName = $"{1}_to_{2}";
            background.loop = false;
            background.timeScale = 0;
        }

        public void GoNext()
        {
            _goindex++;

            background.AnimationName = $"{_goindex}_to_{_goindex + 1}";
            background.timeScale = 1;
        }
    }
}