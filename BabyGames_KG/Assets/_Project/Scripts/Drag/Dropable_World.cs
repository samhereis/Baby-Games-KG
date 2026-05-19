using _Project.Scripts.Sound;
using DataClasses;
using DG.Tweening;
using Gameplay;
using Helpers;
using Loggers;
using Observables;
using Services;
using Sirenix.OdinInspector;
using Sounds;
using Spine.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using _Project._Modes.Coloring;
using _Project.Scripts.GameFeel;
using Modes.Coloring;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace Coocking
{
    //TODO: too loaded, need seperation
    public class Dropable_World : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public ObservableValue<bool> isDropped = new("");
        public Action<Dropable_World> onFinish;
        public Action<Dropable_World> onPostAction;

        public enum PostActionMode { Enable, Scale, Fade, OnlyNotify }
        public PostActionMode postActionMode;

        [FoldoutGroup("animation")] public string actionName = "action";
        [FoldoutGroup("animation")] public Sound actionSound = new();

        public GameObject enableOnDrop;

        [Header("Scale")]
        public float startScaling;
        public float animationDurationDivider = 2;
        public float goToTargetDuration = 0.25f;
        public float scaleDuration = 1;
        public float scaleTo = 1;

        [Space]
        public SkeletonAnimation skeletonAnimation;
        public BoxCollider boxCollider;

        [Space]
        public Transform targetPosition;
        public List<Transform> wrongAnswers = new();

        [Space]
        public Vector3 initialPosition;
        public float dropDistance = 2;
        public bool doFadeAfter = true;
        public bool goToInitialPositionOnDrop = true;
        public float fadeAmount = 0f;
        public bool autoSearchForWrongPosition = false;

        [FoldoutGroup("FX")] public bool doPunchAnimation = true;
        [FoldoutGroup("FX")] public float dropParticleScale = 1;
        [FoldoutGroup("FX")] public float dragParticleScale = 1;

        [Space]
        [FoldoutGroup("FX")] public Sound_FX sound_FX;
        [FoldoutGroup("FX")] public Move_FX move_FX;
        [FoldoutGroup("FX")] public TrailParticle trailParticle;
        [FoldoutGroup("FX")] public ObjectJuicer objectJuicer;

        private bool _hasInteracted = false;

        [Inject] private ISoundPlayer _soundPlayer;
        [Inject] private Content _content;

        private void Awake()
        {
            skeletonAnimation = GetComponentInChildren<SkeletonAnimation>();
            boxCollider = GetComponent<BoxCollider>();

            if (doFadeAfter)
            {
                skeletonAnimation.GetComponent<MeshRenderer>().sharedMaterials.First(x => x != null).DOFade(1, 0.1f);
            }

            if (enableOnDrop != null) { enableOnDrop?.SetActive(false); }

            if (sound_FX == null) { sound_FX = GetComponent<Sound_FX>(); }
            if (move_FX == null) { move_FX = GetComponent<Move_FX>(); }
            if (objectJuicer == false) { objectJuicer = GetComponent<ObjectJuicer>(); }
            if (objectJuicer == false) { objectJuicer = gameObject.AddComponent<ObjectJuicer>(); }
            if (trailParticle == null)
            {
                trailParticle = gameObject.AddComponent<TrailParticle>();
                trailParticle.dropParticleScale = dragParticleScale;
                trailParticle.enabled = false;
            }

            DiService.Inject(this);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_hasInteracted == false) { initialPosition = transform.position; }

            if (autoSearchForWrongPosition)
            {
                var list = FindObjectsByType<Dropable_General>(FindObjectsSortMode.None);

                foreach (var item in list)
                {
                    if (item == null || item.placesHolder == null) { continue; }
                    foreach (var item1 in item.placesHolder.secondary) { wrongAnswers.SafeAdd(item1); }
                }
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (boxCollider.enabled == false) { return; }

            if (sound_FX != null) { sound_FX.Play(Sound_Effect.StartDrag); }
            if (trailParticle != null) { trailParticle.enabled = true; }
        }

        public void OnDrag(PointerEventData eventData)
        {
            var sp = new Vector3(eventData.position.x, eventData.position.y, 0f);
            var world = Camera.main != null ? Camera.main.ScreenToWorldPoint(sp) : transform.position;
            world.z = 0f;
            transform.position = world;
        }

        public async void OnEndDrag(PointerEventData eventData)
        {
            if (trailParticle != null) { trailParticle.enabled = false; }

            if (Vector3.Distance(transform.position, targetPosition.position) < dropDistance)
            {
                boxCollider.enabled = false;
                isDropped.ChangeValue(true);

                Move_FX.MakeDoneParticle(targetPosition.position);

                await transform.DOMove(targetPosition.position, goToTargetDuration).AsyncWaitForCompletion();
                skeletonAnimation.timeScale = 1;
                skeletonAnimation.loop = false;
                skeletonAnimation.AnimationName = actionName;
                _soundPlayer.TryPlay(actionSound);

                switch (postActionMode)
                {
                    case PostActionMode.OnlyNotify:
                    {
                        onPostAction?.Invoke(this);
                        break;
                    }
                    case PostActionMode.Enable:
                    {
                        await AsyncHelper.DelayFloat(skeletonAnimation.AnimationState.GetCurrent(0).Animation.Duration);
                        if (enableOnDrop != null) { enableOnDrop?.SetActive(true); }
                        onPostAction?.Invoke(this);
                        break;
                    }
                    case PostActionMode.Scale:
                    {
                        await AsyncHelper.DelayFloat(startScaling);
                        onPostAction?.Invoke(this);
                        if (enableOnDrop != null)
                        {
                            enableOnDrop.transform.localScale = Vector3.zero;
                            enableOnDrop.SetActive(true);
                            enableOnDrop.transform.DOScale(scaleTo, scaleDuration);
                        }
                        break;
                    }
                    case PostActionMode.Fade:
                    {
                        onPostAction?.Invoke(this);
                        break;
                    }
                }

                await AsyncHelper.DelayFloat(skeletonAnimation.AnimationState.GetCurrent(0).Animation.Duration / animationDurationDivider);
                skeletonAnimation.AnimationName = "start";

                if (doFadeAfter)
                {
                    skeletonAnimation.GetComponent<MeshRenderer>().sharedMaterials.First(x => x != null).DOFade(fadeAmount, 1);
                }

                if (goToInitialPositionOnDrop) { await transform.DOMove(initialPosition, 1).AsyncWaitForCompletion(); }

                onFinish?.Invoke(this);
            }
            else
            {
                boxCollider.enabled = false;
                await TryPlayFailureSounds();

                await transform.DOMove(initialPosition, 1).AsyncWaitForCompletion();
                boxCollider.enabled = true;
            }
        }

        private async Task TryPlayFailureSounds()
        {
            try
            {
                foreach (var item in wrongAnswers)
                {
                    if (item == this) { continue; }

                    if (Vector3.Distance(transform.position, item.position) < dropDistance)
                    {
                        var clip = await sound_FX?.PlayAsync(Sound_Effect.Failure);
                        move_FX.Fail(transform, clip.length);
                        await AsyncHelper.DelayFloat(clip.length);

                        break;
                    }
                }
            } catch (Exception ex) { CustomLogger.instance?.LogException(ex); }
        }
    }
}