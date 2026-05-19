using DataClasses;
using DG.Tweening;
using Helpers;
using Observables;
using Services;
using Sirenix.OdinInspector;
using Sounds;
using Spine.Unity;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;

namespace ColorfulTrain
{
    public class ColorfulTrain_WheellessCar : MonoBehaviour
    {
        public ObservableValue<bool> isDone = new("");

        public SpriteRenderer spriteRenderer;

        public ColorfulTrain_Wheel wheel;
        public SkeletonPartsRenderer missingWheel;
        public Transform wheelPlace;

        [Space]
        public float delayBeforeSoundPlay;
        public Sound audioClip;

        [Inject] private ISoundPlayer _soundPlayer;

        public string goAnimationName = "go";

        [Button]
        public void Setup()
        {
            foreach (var item in GetComponentsInChildren<SkeletonPartsRenderer>())
            {
                item.MeshRenderer.sortingLayerName = "Background";
            }
        }

        private void OnEnable()
        {
            DiService.Inject(this);
        }

        private void Update()
        {
            transform.DOLocalMoveY(0, 0.25f);
        }

        public async Task MarkDone()
        {
            if (isDone.value) { return; }
            isDone.ChangeValue(true);

            if (spriteRenderer != null)
            {
                spriteRenderer?.gameObject?.SetActive(false);
            }
            missingWheel.gameObject.SetActive(true);

            await AsyncHelper.DelayFloat(0.5f);
            GetComponent<SkeletonAnimation>().loop = false;
            GetComponent<SkeletonAnimation>().AnimationName = goAnimationName;

            PlayAudio();

            foreach (var partRenderer in GetComponentsInChildren<SkeletonPartsRenderer>(true))
            {
                partRenderer.MeshRenderer.sortingOrder *= 2;
            }

            await AsyncHelper.DelayFloat(GetComponent<SkeletonAnimation>().AnimationState.GetCurrent(0).Animation.Duration);
        }

        private async void PlayAudio()
        {
            await AsyncHelper.DelayFloat(delayBeforeSoundPlay);
            _soundPlayer?.TryPlay(audioClip);
        }
    }
}