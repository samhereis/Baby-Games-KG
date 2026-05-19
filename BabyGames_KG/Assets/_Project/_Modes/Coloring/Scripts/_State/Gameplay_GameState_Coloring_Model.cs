using DataClasses;
using GameState;
using Helpers;
using Interfaces.Providers;
using Observables;
using Saratan.Coloring;
using Services;
using SO;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using _Project.Scripts.Data;
using UnityEngine;
using Zenject;

namespace Modes.Coloring
{
    public class Gameplay_GameState_Coloring_Model
    {
        public Action<Exception> onFatalError { get; set; }
        public Action onClearRequested { get; set; }

        public Func<Task> onAnimationStarting { get; set; }
        public Action onAnimationEnded { get; set; }
        public Action onAnimationEndedAction { get; set; }

        public ObservableValue<bool> isInitialized { get; protected set; } = new(nameof(isInitialized));
        public ObservableValue<bool> isAnimationPlaying { get; protected set; } = new(nameof(isAnimationPlaying));
        public ObservableValue<bool> isAnimationPlayed { get; protected set; } = new(nameof(isAnimationPlayed));

        [Inject] public ObservableValue<Paintable_Identifier_Basic> tapObject { get; protected set; }

        [Inject] public IGameStateService gameStateService { get; protected set; }
        [Inject] public ISoundPlayer soundPlayer { get; protected set; }
        [Inject] public IActivity_Identifier_Provider activity_Identifier_Provider { get; protected set; }
        [Inject] public GameController gameController { get; private set; }

        [Inject] public ListOfAllMenus_SO listOfAllMenus { get; protected set; }
        [Inject] public GameConfigs_Coloring_SO gameSettings { get; protected set; }
        [Inject] public Content content { get; protected set; }

        [Inject] public BackgroundPage_Identifier backgroundPage { get; protected set; }
        [Inject] public EngineEyes engineEyes { get; protected set; }
        [Inject] public Tools_Identifier tooldIdentifier { get; protected set; }
        [Inject] public GameSaveService gameSaveService { get; protected set; }

        public bool hasLocalSave => localSaves.Count > 0 && localSaves.Count == currentSpine.textures.Count;
        public List<Texture2D> localSaves { get; protected set; } = new();

        public Pallete pallete { get; set; }
        public ScreenshotService screenshotService { get; protected set; }
        public Spine_Identifier currentSpine { get; protected set; }

        public Activity activity { get; protected set; }

        public Gameplay_GameState_Coloring_Model(Activity newActivity)
        {
            activity = newActivity;
        }

        public virtual void Initialize()
        {
            DiService.Inject(this);
            screenshotService = new ScreenshotService(this);
        }

        public void SetSpine(Spine_Identifier spine_Identifier)
        {
            currentSpine = spine_Identifier;

            int index = 0;
            foreach (var textureCache in currentSpine.textures)
            {
                Texture2D texture = gameSaveService.GetDrawing(activity.acitvityCategory, activity.GetName(), index);

                if (texture != null)
                {
                    texture.wrapMode = TextureWrapMode.Repeat;
                    texture.filterMode = FilterMode.Bilinear;
                    texture.anisoLevel = 1;

                    localSaves.Add(texture);
                    textureCache.material.mainTexture = texture;
                }
                else
                {
                    var t = (textureCache.texture as Texture2D).GetReadableCopy();
                    ClearablesHolder.instance.clearableTextures.SafeAdd(t);

                    textureCache.material.mainTexture = t;
                    textureCache.texture = textureCache.material.mainTexture;
                }

                index++;
            }
        }
    }
}