using Modes.Coloring;
using UnityEngine;

namespace SO
{
    [CreateAssetMenu(menuName = "Scriptables/Lists/" + nameof(GameConfigs_Coloring_SO), fileName = nameof(GameConfigs_Coloring_SO))]
    public class GameConfigs_Coloring_SO : ScriptableObject
    {
        public EyesSettings eyeSettings = new();
        public DrawSettings drawSettings = new();
        public PalleteData palleteData = new();
        public GameplaySettings_UI_Eraser ui_eraser = new();
        public GameLinks_Data linkSettings = new();

        [Space]
        [field: SerializeField] public int screenTimeout = 1;
        [field: SerializeField] public float backgroundColorX = 1;
        [field: SerializeField] public float backgroundColorY = 1;
        [field: SerializeField] public float backgroundColorZ = 1;
        [field: SerializeField] public float backgroundColorA = 1;

        [Space]
        [field: SerializeField] public float onAnimationMusicFadeValue = 0.25f;
        [field: SerializeField] public float onAnimationMusicFadeDuration = 0.5f;

        [Space]
        [field: SerializeField] public float hint_delayDraw = 10f;
        [field: SerializeField] public float hint_delayPlayAnimation = 10f;

        [Space]
        [field: SerializeField] public float nonGlitterAlpha = 0.98f;

        [Space]
        [field: SerializeField] public int spineDefaultSortingOrder = 0;

        [Space]
        [field: SerializeField] public LayerMask layerMask_Spine;
        [field: SerializeField] public LayerMask layerMask_allDrawables;

        [Space]
        public float decalsHitSpacing = 1;
        public float decalsRadius = 0.6f;

        [Space]
        public int snapshot_BorderWith = 45;
        public Color snapshot_Color = Color.yellow;
        public int snapshot_CornerRadius = 45;
    }
}