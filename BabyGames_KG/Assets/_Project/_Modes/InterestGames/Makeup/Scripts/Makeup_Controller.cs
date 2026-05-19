using Modes.Puzzle;
using Sirenix.OdinInspector;
using Spine;
using Spine.Unity;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InterestGames
{
    public class Makeup_Controller : StateMachineBase
    {
        protected GameplayGameState_Makeup_Model _model;

        public Skin combinedSkin;
        public SkeletonAnimation girl;

        [FoldoutGroup("Skins"), SpineSkin] public List<string> hairSkin = new();
        [FoldoutGroup("Skins"), SpineSkin] public List<string> rumyana = new();
        [FoldoutGroup("Skins"), SpineSkin] public List<string> pomada = new();
        [FoldoutGroup("Skins"), SpineSkin] public List<string> sergi = new();
        [FoldoutGroup("Skins"), SpineSkin] public List<string> ojereliya = new();
        [FoldoutGroup("Skins"), SpineSkin] public List<string> zakolki = new();
        [FoldoutGroup("Skins"), SpineSkin] public List<string> obodok = new();
        [FoldoutGroup("Skins"), SpineSkin] public List<string> diadema = new();
        [FoldoutGroup("Skins"), SpineSkin] public List<string> banti = new();

        public int hairIndex = 0;

        public bool autoInitialize;
        private async void Awake()
        {
#if UNITY_EDITOR
            if (autoInitialize)
            {
                await Initialize(null);
            }
#endif
        }

        public virtual async Task Initialize(GameplayGameState_Makeup_Model model)
        {
            _model = model;
            _model?._skinCombiner?.Build();

            foreach (var item in _allStates)
            {
                await item.PreInittialize();

                if (item is Makeup_StateBase princess_StateBase)
                {
                    princess_StateBase.Construct(_model);
                }
            }

            ChangeState(_startState);
        }

        [Button]
        public void Build()
        {
            if (girl == null) { girl = GetComponentInChildren<SkeletonAnimation>(); }

            if (girl != null)
            {
                combinedSkin = new Skin("combinedSkin");

                if (string.IsNullOrEmpty(hairSkin.Last()) == false) { AddSkin_World(hairSkin.Last()); }
                if (string.IsNullOrEmpty(rumyana.Last()) == false) { AddSkin_World(rumyana.Last()); }
                if (string.IsNullOrEmpty(pomada.Last()) == false) { AddSkin_World(pomada.Last()); }
                if (string.IsNullOrEmpty(sergi.Last()) == false) { AddSkin_World(sergi.Last()); }
                if (string.IsNullOrEmpty(ojereliya.Last()) == false) { AddSkin_World(ojereliya.Last()); }
                if (string.IsNullOrEmpty(zakolki.Last()) == false) { AddSkin_World(zakolki.Last()); }
                if (string.IsNullOrEmpty(obodok.Last()) == false) { AddSkin_World(obodok.Last()); }
                if (string.IsNullOrEmpty(banti.Last()) == false) { AddSkin_World(banti.Last()); }

                girl.skeleton.SetSkin(combinedSkin);
                girl.skeleton.SetSlotsToSetupPose();
            }
        }

        private void AddSkin_World(string skinName)
        {
            if (string.IsNullOrEmpty(skinName) == false)
            {
                var skin = girl.skeleton.Data.FindSkin(skinName);
                combinedSkin.AddSkin(skin);
            }
        }
    }
}