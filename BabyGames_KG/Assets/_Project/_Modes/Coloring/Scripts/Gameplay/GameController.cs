using _Project.Scripts.Data;
using _Project.Scripts.Services;
using CustomAttributes;
using DataClasses.AssetReferences;
using Helpers;
using Identifiers;
using Loggers;
using PaintCore;
using Spine;
using Spine.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Modes.Coloring
{
    public class GameController : MonoBehaviour
    {
        [Space]
        [SerializeField] private ExternalAssetReference_HasComponent<ScreenShotItem> _screenShotObjectReference;

        [SerializeField] private ExternalAssetReference_HasComponent<ParticleSystem> _releaseParticleReference;

        [SerializeField] public List<ToolBase> tools = new();

        public Gameplay_GameState_Coloring_Model model { get; private set; }

        private Paintable_Identifier_Basic _currentPaintable;
        private ScreenShotItem _screenShotObject;
        private ParticleSystem _releaseParticle;

        private BackgroundMusicService _backgroundMusicService;

        [Fg_De, SerializeField] public bool isAnimationPlaying = false;

        public async Task Initialize(Gameplay_GameState_Coloring_Model newModel)
        {
            if (tools.Count < 1)
            {
                tools = FindObjectsByType<ToolBase>(FindObjectsInactive.Include, FindObjectsSortMode.None).ToList();
            }

            model = newModel;

            _ActivityBase_Identifier activityBase_Identifier = await model.activity_Identifier_Provider.GetActivity(model.activity);
            Spine_Identifier spine_Identifier_prefab = activityBase_Identifier?.Get<Spine_Identifier>();
            if (spine_Identifier_prefab == null)
            {
                return;
            }

            Spine_Identifier spine_Identifier = Instantiate(spine_Identifier_prefab);
            if (spine_Identifier == null)
            {
                return;
            }

            model.SetSpine(spine_Identifier);
            await spine_Identifier.AddGlitter(this);

            spine_Identifier.Get<SkeletonAnimation>().loop = false;
            spine_Identifier.Get<SkeletonAnimation>().AnimationName = "idle";

            await spine_Identifier.Init_SlotSeparation();
            SetDrawings();
            SetBackgroundDrawing();

            model.engineEyes.Initialize(model);
            model.isInitialized?.ChangeValue(true);

            _screenShotObject = await _screenShotObjectReference.InstantiateAsync();
            _releaseParticle = await _releaseParticleReference.InstantiateAsync();

            _backgroundMusicService = FindAnyObjectByType<BackgroundMusicService>(FindObjectsInactive.Include);
        }

        private void Update()
        {
            if (model?.currentSpine?.Get<SkeletonAnimation>()?.AnimationName != "idle")
                return;

            var pointer = Pointer.current;
            if (pointer == null)
                return;

            Vector2 screenPos = pointer.position.ReadValue();

            if (pointer.press.wasPressedThisFrame)
            {
                Ray ray = Camera.main.ScreenPointToRay(screenPos);
                if (Physics.Raycast(ray, out RaycastHit hit, 50f, model.gameSettings.layerMask_allDrawables)
                    && hit.collider != null
                    && hit.collider.gameObject.TryGetComponent<Paintable_Identifier_Basic>(out var paintable_Identifier))
                {
                    _currentPaintable = paintable_Identifier;
                    model.tapObject.ChangeValue(_currentPaintable);
                }
            }

            if (pointer.press.wasReleasedThisFrame)
            {
                model.tapObject.ChangeValue(null);
                Confetti();
            }
        }

        public Task Dispose()
        {
            model.soundPlayer.Stop(model.currentSpine.animationAudioClip);
            return Task.CompletedTask;
        }

        public async void PlayAnimation(Action onAnimationEnded)
        {
            _backgroundMusicService.ChangeVolume_External(0.25f);

            if (model.onAnimationStarting != null)
            {
                await model.onAnimationStarting?.Invoke();
            }

            model.onAnimationEndedAction = onAnimationEnded;

            model.onAnimationEnded -= OnAnimationEnded;
            model.onAnimationEnded += OnAnimationEnded;

            model.tooldIdentifier?.Deactivate();

            PlaySpineAnimation();

            SaveDrawings();
        }

        public async Task SaveScreenshot()
        {
            try
            {
                if (_screenShotObject == null)
                {
                    return;
                }

                Texture2D screenshot = model.screenshotService.GetSnapshot(model.activity.GetName(), model.gameSettings.layerMask_allDrawables);

                if (Application.isEditor)
                {
                    await model.gameSaveService.SaveScreenshot(screenshot, model.activity.acitvityCategory, model.activity.GetName());
                }
                else
                {
                    string name = string.Format("{0}_Capture{1}_{2}.jpeg", Application.productName, "{0}", System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
                    NativeGallery.SaveImageToGallery(screenshot, Application.productName + " Captures", name);
                }

                _screenShotObject.AnimationScreenShot(screenshot);
            }
            catch (Exception ex)
            {
                CustomLogger.instance?.LogException(ex, "SaveScreenshot(): Could not SaveScreenshot");
            }
        }

        public void WriteResult(bool isAutoSave = false)
        {
            if (model.currentSpine?.Get<SkeletonAnimation>().AnimationName == "idle")
            {
                SaveIcon();
            }

            SaveBackgroundDrawing();
            SaveDrawings();
        }

        public async Task ClearDrawings()
        {
            foreach (var texturesWithTheirPaintable in model.currentSpine.materialsWithTheirPaintables)
            {
                Texture2D destinationTexture = texturesWithTheirPaintable.Key.mainTexture.MakeTexture2D(ClearablesHolder.instance.clearableTextures);
                await destinationTexture.SetColor(Color.white);

                foreach (var paintable in texturesWithTheirPaintable.Value)
                {
                    if (paintable.paintableTexture.Texture == null) { continue; }
                    Destroy(paintable.paintableTexture.Texture);
                }
            }
        }

        public List<Texture> textures = new();

        private void SaveDrawings()
        {
            try
            {
                int index = 0;
                foreach (var texturesWithTheirPaintable in model.currentSpine.materialsWithTheirPaintables)
                {
                    Texture2D destinationTexture = texturesWithTheirPaintable.Key.mainTexture.MakeTexture2D(ClearablesHolder.instance.clearableTextures);

                    foreach (var paintable in texturesWithTheirPaintable.Value)
                    {
                        Texture2D sourceTexture = paintable.GetOutputTexture();
                        Graphics.CopyTexture(sourceTexture, 0, 0, 0, 0, paintable.atlasRegion.width, paintable.atlasRegion.height, destinationTexture, 0, 0,
                            paintable.atlasRegion.x, destinationTexture.height - paintable.atlasRegion.y - paintable.atlasRegion.height);

                        Destroy(sourceTexture);
                    }

                    model.gameSaveService.SetDrawings(destinationTexture, model.activity.acitvityCategory, model.activity.GetName(), index);
                    index++;
                }
            }
            catch (Exception ex)
            {
                CustomLogger.instance?.LogException(ex, "Error during SaveDrawings() GameController_SlotSeparation");
            }
        }

        public void SaveBackgroundDrawing()
        {
            model.gameSaveService.SetBackgroundDrawing(model.backgroundPage.GetTexture(), model.activity.acitvityCategory, model.activity.GetName());
        }

        public void SaveIcon()
        {
            try
            {
                Texture2D icon = model.screenshotService.GetIcon(model.activity.GetName(), model.gameSettings.layerMask_Spine);
                model.gameSaveService.SetIcon(icon, model.activity.acitvityCategory, model.activity.GetName());
            }
            catch (Exception ex)
            {
                CustomLogger.instance?.LogException(ex, "Error during SaveIcon()");
            }
        }

        private void SetDrawings()
        {
            if (model.hasLocalSave == false)
            {
                int index = 0;
                foreach (var materialWithTheirPaintable in model.currentSpine.materialsWithTheirPaintables)
                {
                    materialWithTheirPaintable.Value.ForEach(paintable => { paintable.Get<CwPaintableTexture>().SetTexture(materialWithTheirPaintable.Key.mainTexture); });
                    index++;
                }
            }
            else
            {
                int index = 0;
                foreach (var materialWithTheirPaintable in model.currentSpine.materialsWithTheirPaintables)
                {
                    Texture2D texture = model.localSaves[index];
                    materialWithTheirPaintable.Value.ForEach(paintable => { paintable.Get<CwPaintableTexture>().SetTexture(texture); });

                    index++;
                }
            }
        }

        private void SetBackgroundDrawing()
        {
            Texture2D backgroundDrawing = model.gameSaveService.GetBackgroundDrawing(model.activity.acitvityCategory, model.activity.GetName());
            model.backgroundPage.InitBackgroundPage(backgroundDrawing);
        }

        private async void PlaySpineAnimation()
        {
            SaveIcon();

            model.isAnimationPlaying.value = true;
            isAnimationPlaying = true;

            CancellationToken cancellationToken = destroyCancellationToken;

            TrackEntry currentTrackEntry;

            model.currentSpine.Get<SkeletonAnimation>().Skeleton.A = 1;

            currentTrackEntry = model.currentSpine.Get<SkeletonAnimation>().AnimationState.SetAnimation(0, "action", false);
            model.soundPlayer.TryPlay(model.currentSpine.animationAudioClip);
            while (currentTrackEntry.IsComplete == false)
            {
                if (cancellationToken.IsCancellationRequested == true)
                {
                    isAnimationPlaying = false;
                    model.isAnimationPlaying.value = false;
                    return;
                }

                await AsyncHelper.NextFrame();
            }

            currentTrackEntry = model.currentSpine.Get<SkeletonAnimation>().AnimationState.SetAnimation(0, "idle", false);

            isAnimationPlaying = false;
            model.isAnimationPlaying.value = false;
            model.onAnimationEnded?.Invoke();
        }

        private void OnAnimationEnded()
        {
            model.onAnimationEndedAction?.Invoke();
            model.isAnimationPlayed?.ChangeValue(true);

            model.tooldIdentifier?.Activate();

            _backgroundMusicService.ChangeVolume_External(1);
        }

        public void Confetti()
        {
            if (_releaseParticle == null)
            {
                return;
            }

            var touchPosition = Pointer.current.position.ReadValue();
            var particlePos = Camera.main.ScreenToWorldPoint(touchPosition);
            var particleTransform = _releaseParticle.GetComponent<Transform>();
            float scale = Camera.main.orthographicSize / 5.4f;
            particleTransform.localScale = new Vector3(scale, scale, 0);
            _releaseParticle.GetComponent<Transform>().position = new Vector3(particlePos.x, particlePos.y);
            _releaseParticle.Play();
        }
    }
}