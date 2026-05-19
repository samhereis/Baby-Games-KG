using _Project._Modes.Orchestra.Scripts.SO;
using _Project.Scripts._Modes.Sorting;
using AllIn1SpriteShader;
using CustomAttributes;
using DataClasses;
using DG.Tweening;
using FX;
using GameState;
using Helpers;
using Modes.Sorting;
using Services;
using Spine.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace _Project._Modes.Orchestra.Scripts
{
    public class OrchestraController : MonoBehaviour
    {
        [SerializeField] [Fg_Se] private int _waveCount = 4;

        [SerializeField] [Fg_Se] private List<OrchestraCharacter_Identifier> _orchestraCharacters = new();
        [SerializeField] [Fg_Se] private List<WaveSettings_Orchestra> _waveHolders = new();

        [Space]
        [SerializeField] [Fg_De] public OrchestraWaveData currentOrchestraWaveData;

        [SerializeField] [Fg_De] private int _currentWaveIndex;

        public Action<DropZone_Identifier> onDropZoneCompleted;
        public Action<Transform> doConfetti;

        public Gameplay_GameState_Orchestra_Model model;
        public Gameplay_GameState_Orchestra_View view;

        [Inject] private Orchestra_Data _orchestra_Data;

        public async Task Initialize(Gameplay_GameState_Orchestra_Model model, Gameplay_GameState_Orchestra_View view)
        {
            DiService.Inject(this);

            this.model = model;
            this.view = view;

            _orchestraCharacters = transform.root.GetComponentsInChildren<OrchestraCharacter_Identifier>(true).ToList();
            foreach (var orchestraCharacter in _orchestraCharacters)
            {
                await orchestraCharacter.Initialize();
            }

            await SetNextWave();
        }

        private void Update()
        {
            foreach (var item in currentOrchestraWaveData?.waveUnits)
            {
                if (item.isComplete)
                {
                    item.draggable.transform.DOScale(0, 0.25f);
                }
            }
        }

        public async Task SetNextWave()
        {
            if (_currentWaveIndex > 0)
            {
                doConfetti?.Invoke(transform);
            }

            if (_currentWaveIndex < _waveCount)
            {
                currentOrchestraWaveData = new OrchestraWaveData();
                var waveHolder = _waveHolders[_currentWaveIndex];
                _waveHolders[_currentWaveIndex]?.waveHolder?.gameObject?.SetActive(true);

                foreach (var orchestraCharacter in _orchestraCharacters)
                {
                    var orchestraWaveUnit = orchestraCharacter.waveData[_currentWaveIndex];
                    orchestraCharacter.Get<SkeletonAnimation>().AnimationName = orchestraWaveUnit.animationName;

                    if (_currentWaveIndex > 0)
                    {
                        var particle = await model.orchestra_Data.transition.InstantiateAsync();
                        particle.transform.position = orchestraCharacter.transform.position;
                        particle.Play();
                        await AsyncHelper.NextFrame();
                    }
                }

                foreach (var orchestraCharacter in _orchestraCharacters)
                {
                    await SetupCharacter(orchestraCharacter, waveHolder.waveHolder);
                }

                model.currentOrchestraWaveData = currentOrchestraWaveData;
                view.gameplayMenu_Orchestra.Initialize(model);
            }
        }

        private async Task SetupCharacter(OrchestraCharacter_Identifier orchestraCharacter, Transform waveHolder)
        {
            var orchestraWaveData = orchestraCharacter.waveData[_currentWaveIndex];
            currentOrchestraWaveData.waveUnits.Add(orchestraWaveData);

            var dropZone = await _orchestra_Data.dropZone_Identifier.InstantiateAsync(waveHolder);
            orchestraWaveData.dropZone = dropZone;

            if (orchestraWaveData.objectNames_force._spriteRenderers.Count < 1)
            {
                for (var i = 0; i < orchestraWaveData.objectNames.Count; i++)
                {
                    var texture = await orchestraCharacter.GetInstrumentForWave(_currentWaveIndex, i);
                    if (texture != null)
                    {
                        await texture.ClampAlphas();

                        var newSpriteRenderer_GameObject = new GameObject($"sp_{i}");
                        newSpriteRenderer_GameObject.transform.parent = dropZone.transform;
                        var allInOneChader = newSpriteRenderer_GameObject.AddComponent<AllIn1Shader>();
                        allInOneChader.currentShaderType = AllIn1Shader.ShaderTypes.ScaledTime;

                        var newSpriteRenderer = newSpriteRenderer_GameObject.AddComponent<SpriteRenderer>();
                        dropZone.spriteRenderer.Add(newSpriteRenderer);

                        dropZone.spriteRenderer[i].sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                        dropZone.spriteRenderer[i].sortingLayerName = orchestraWaveData.sortingLayerName;
                        dropZone.spriteRenderer[i].sortingOrder = orchestraWaveData.objectNames[i].value;
                    }
                }

                var combinedBounds = orchestraWaveData.relatedSkeletonParts[0].bounds;
                foreach (var renderer in orchestraWaveData.relatedSkeletonParts.Skip(1))
                {
                    combinedBounds.Encapsulate(renderer.bounds);
                }

                FitUniform(combinedBounds, dropZone);
            }
            else
            {
                dropZone.transform.position = orchestraWaveData.objectNames_force._spriteRenderers[0].transform.position;

                for (var i = 0; i < orchestraWaveData.objectNames.Count; i++)
                {
                    orchestraCharacter.Setup(_currentWaveIndex, i);
                }

                foreach (var item in orchestraWaveData.objectNames_force._spriteRenderers)
                {
                    var allInOneChader = item.gameObject.AddComponent<AllIn1Shader>();
                    allInOneChader.currentShaderType = AllIn1Shader.ShaderTypes.ScaledTime;

                    dropZone.spriteRenderer.Add(item);
                    item.transform.SetParent(dropZone.transform, true);
                }
            }

            foreach (var relatedSkeletonPart in orchestraWaveData.relatedSkeletonParts)
            {
                relatedSkeletonPart.gameObject.SetActive(false);
            }

            dropZone.Initialize(orchestraWaveData);

            orchestraWaveData.dropZone.gameObject.SetActive(true);
            orchestraWaveData.onComplete += OnDropZoneCompleted;
        }

        public bool IsDraggingCorrectly(OrchestraWaveUnit orchestraWaveUnit)
        {
            var distance = Vector2.Distance(Camera.main.ScreenToWorldPoint(orchestraWaveUnit.draggable._rectTransform.position),
                orchestraWaveUnit.dropZone.transform.position);

            return distance < 2;
        }

        public async void OnDropZoneCompleted(OrchestraWaveUnit orchestraWaveUnit)
        {
            var waveHolder = _waveHolders[_currentWaveIndex];

            onDropZoneCompleted?.Invoke(orchestraWaveUnit.dropZone);
            doConfetti?.Invoke(orchestraWaveUnit.dropZone.transform);

            var orchestraWaveData = orchestraWaveUnit.orchestraCharacter_Identifier.waveData[_currentWaveIndex];

            foreach (var relatedSkeletonPart in orchestraWaveData.relatedSkeletonParts)
            {
                relatedSkeletonPart.gameObject.SetActive(true);
            }

            orchestraWaveUnit.dropZone.gameObject.SetActive(false);
            orchestraWaveUnit.isComplete = true;
            orchestraWaveUnit.onComplete -= OnDropZoneCompleted;

            orchestraWaveData.orchestraCharacter_Identifier.Get<SkeletonAnimation>().AnimationName = orchestraWaveData.giveAnimation;
            waveHolder.PlaySound(orchestraWaveData.animationAudio);

            if (currentOrchestraWaveData.waveUnits.TrueForAll(x => x.isComplete))
            {
                var duration = orchestraWaveData.orchestraCharacter_Identifier.Get<SkeletonAnimation>().AnimationState.GetCurrent(0).Animation.Duration;
                await AsyncHelper.DelayFloat(duration);

                orchestraWaveData.orchestraCharacter_Identifier.Get<SkeletonAnimation>().AnimationState.ClearTracks();
                await AsyncHelper.NextFrame();

                OnWaveCompleted();
            }
            else
            {
                model.onWaveUnitCompleted?.Invoke(orchestraWaveUnit);
            }
        }

        private void FitUniform(Bounds bounds, DropZone_Identifier dropZone)
        {
            dropZone.transform.position = bounds.center;

            var originalScale = dropZone.transform.localScale;

            var spriteSize = dropZone.spriteRenderer[0].bounds.size;
            var boundsSize = bounds.size;

            var scaleX = boundsSize.x / spriteSize.x;
            var scaleY = boundsSize.y / spriteSize.y;

            var spriteAspect = spriteSize.x / spriteSize.y;
            var boundsAspect = boundsSize.x / boundsSize.y;

            float scaleFactor = 0;
            if (spriteAspect > boundsAspect) { scaleFactor = scaleX; }
            else { scaleFactor = scaleY - (scaleX - scaleY); }

            dropZone.transform.localScale = originalScale * scaleFactor;
        }

        private async void OnWaveCompleted()
        {
            var waveHolder = _waveHolders[_currentWaveIndex];

            DiService.Get<StateEnd_FX>()?.DoFX();

            model.onPanelVisibilityChanged?.Invoke(false);

            if (_currentWaveIndex > 0)
            {
                float duration = 0;

                MainMenu_GameState_Model.selectedActivityCategory.TryGetSetting_Float(nameof(model.orchestra_Data.farmOrchestra_delayBeforeFinalAnimation), model.orchestra_Data.farmOrchestra_delayBeforeFinalAnimation, out float delay);
                await AsyncHelper.DelayFloat(delay);

                foreach (var orchestraCharacter in _orchestraCharacters)
                {
                    var orchestraWaveData = orchestraCharacter.waveData[_currentWaveIndex];

                    orchestraWaveData.orchestraCharacter_Identifier.Get<SkeletonAnimation>().AnimationState.ClearTracks();

                    orchestraWaveData.orchestraCharacter_Identifier.Get<SkeletonAnimation>().AnimationName = orchestraWaveData.finalAnimation;
                    var duration_temp = orchestraWaveData.orchestraCharacter_Identifier.Get<SkeletonAnimation>().AnimationState.GetCurrent(0).Animation.Duration;
                    if (duration_temp > duration) duration = duration_temp;
                }
                waveHolder?.PlayFinish();

                await AsyncHelper.DelayFloat(duration);
            }

            bool HasNext = _currentWaveIndex < _waveCount - 1;
            if (HasNext)
            {
                foreach (var orchestraCharacter in _orchestraCharacters)
                {
                    var orchestraWaveData = orchestraCharacter.waveData[_currentWaveIndex];

                    orchestraWaveData.orchestraCharacter_Identifier.Get<SkeletonAnimation>().AnimationState.ClearTracks();
                    await AsyncHelper.NextFrame();

                    orchestraCharacter.Get<SkeletonAnimation>().AnimationName = orchestraWaveData.animationName;
                    await AsyncHelper.NextFrame();
                }
            }

            if (HasNext)
            {
                _currentWaveIndex++;
                await SetNextWave();

                model.onWaveCompleted(currentOrchestraWaveData);
                model.onPanelVisibilityChanged?.Invoke(true);
            }
            else
            {
                model.onWin?.Invoke();
            }
        }
    }
}