using Helpers;
using Loggers;
using Services;
using Sirenix.OdinInspector;
using Sounds;
using Spine;
using Spine.Unity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;
using Identifiers;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Modes.Coloring
{
    [Serializable]
    public class TextureCache
    {
        public Material material;
        public Texture texture;
    }

    [DisallowMultipleComponent]
    public class Spine_Identifier : IdentifierBase, ISelfValidator
    {
        public enum CollidersCalculateMode
        {
            First,
            Second
        }

        [Inject] private Content _content;
        [Inject] private GameController _gameController;

        public SkeletonAnimation skeletonAnimation;
        public List<AtlasRegion> regions { get; private set; } = new();

        [Space]
        public Sound animationAudioClip = new();

        [Space]
        public CollidersCalculateMode collidersCalculateMode = CollidersCalculateMode.First;

        public Vector3 initialPosition = Vector3.zero;
        public Vector3 initialScale = Vector3.one;

        [Space]
        public int maxLayoutOrder = 0;

        public float scale = 0.75f;
        public bool updateTexturesOnAnimationPlay = true;

        [Space]
        public List<TextureCache> textures = new List<TextureCache>();

        [Space]
        public bool overrideEyesData = false;
        public EyesSettings eyeSettings = new();

        [Space]
        public string assetPath = string.Empty;
        public string assetFolder = string.Empty;

        [Space]
        public List<Paintable_Identifier_SlotSeparation> _paintables = new();
        [ShowInInspector] public Dictionary<Material, List<Paintable_Identifier_SlotSeparation>> materialsWithTheirPaintables = new();

        public void Validate(SelfValidationResult result)
        {
#if UNITY_EDITOR
            transform.position = initialPosition;
            transform.localScale = initialScale;
#endif
        }

        private void Awake()
        {
            DiService.Inject(this);
        }

        [Button]
        public void UpdateData()
        {
            if (skeletonAnimation == null)
            {
                skeletonAnimation = Get<SkeletonAnimation>();
            }

#if UNITY_EDITOR
            if (Application.isPlaying)
            {
                return;
            }

            assetPath = AssetDatabase.GetAssetPath(gameObject);
            assetFolder = assetPath.Replace($"{gameObject.name}.prefab", "");

            Populate();
            SetMaterials();
            SetAudios();
#endif
        }

        private void Populate()
        {
#if UNITY_EDITOR
            var skeletonData = Directory.GetFiles(assetFolder).ToList().FindAll(x => x.Contains(".asset") && x.Contains("Skeleton"))[0];
            skeletonAnimation.skeletonDataAsset = AssetDatabase.LoadMainAssetAtPath(skeletonData) as SkeletonDataAsset;

            Get<SkeletonAnimation>().AnimationName = "idle";
#endif
        }

        private void SetAudios()
        {
#if UNITY_EDITOR
            var autioPaths = Directory.GetFiles(assetFolder).ToList().FindAll(x => x.Contains(".wav") || x.Contains(".mp3"));
            foreach (var audioPath in autioPaths)
            {
                AudioClip audio = AssetDatabase.LoadMainAssetAtPath(audioPath) as AudioClip;
                if (audio != null)
                {
                    animationAudioClip.SetSound(audio);
                    AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(audio), gameObject.name + "_audio");
                }
            }
#endif
        }

        [Button]
        private void SetMaterials()
        {
            foreach (var item in textures)
            {
                if (item.texture == null || item.material == null)
                {
                    textures.Remove(item);
                    return;
                }

                if (item.texture == null)
                {
                    Debug.LogError("No texture", gameObject);
                }

                item.material.mainTexture = item.texture;
                item.material.SetTexture("_MainTex", item.texture);
            }

#if UNITY_EDITOR
            if (Application.isPlaying)
            {
                return;
            }

            Get<MeshRenderer>().sharedMaterials = textures.Select(x => x.material).ToArray();

            var materialsPaths = Directory.GetFiles(assetFolder).ToList().FindAll(x => x.Contains(".mat"));
            foreach (var materialPath in materialsPaths)
            {
                Material foundMaterial = AssetDatabase.LoadMainAssetAtPath(materialPath) as Material;
                if (foundMaterial != null)
                {
                    if (textures.Find(x => x.material == foundMaterial) != null)
                    {
                        foundMaterial.mainTexture = textures.Find(x => x.material == foundMaterial).texture;
                    }
                    else
                    {
                        textures.SafeAdd(new TextureCache() { material = foundMaterial, texture = foundMaterial.mainTexture });
                    }
                }
            }

            foreach (var item in textures)
            {
                item.material.shader = Shader.Find("Shader Graphs/GlitterShader_Drawable");
                string path = AssetDatabase.GetAssetPath(item.texture);

                if (string.IsNullOrEmpty(path) == false)
                {
                    TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
                    if (textureImporter != null)
                    {
                        textureImporter.textureType = TextureImporterType.Default;
                        textureImporter.textureShape = TextureImporterShape.Texture2D;
                        textureImporter.wrapMode = TextureWrapMode.Repeat;
                        textureImporter.filterMode = FilterMode.Point;
                        textureImporter.isReadable = true;
                        textureImporter.mipmapEnabled = false;

                        bool wasChanged = false;

                        if (textureImporter.isReadable == false)
                        {
                            textureImporter.isReadable = true;
                            wasChanged = true;
                        }

                        if (textureImporter.alphaIsTransparency == false)
                        {
                            textureImporter.alphaIsTransparency = true;
                            wasChanged = true;
                        }

                        if (textureImporter.spriteImportMode != SpriteImportMode.Single)
                        {
                            textureImporter.spriteImportMode = SpriteImportMode.Single;
                            wasChanged = true;
                        }


                        TextureImporterPlatformSettings platformTextureSettings = textureImporter.GetPlatformTextureSettings("DefaultTexturePlatform");
                        if (platformTextureSettings.format != TextureImporterFormat.RGBA32)
                        {
                            platformTextureSettings.format = TextureImporterFormat.RGBA32;
                            textureImporter.SetPlatformTextureSettings(platformTextureSettings);
                            wasChanged = true;
                        }

                        if (wasChanged)
                        {
                            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                        }
                    }
                }
            }
#endif
        }

        [Button]
        public async Task Init_SlotSeparation()
        {
            SetMaterials();

            try
            {
                Separate();
                await AsyncHelper.NextFrame();

                Get<SkeletonRenderSeparator>().AddPartsRenderers(skeletonAnimation.separatorSlots.Count + 1);

                await AsyncHelper.NextFrame();

                regions = skeletonAnimation.SkeletonDataAsset.atlasAssets[0].GetAtlas().Regions;
                List<SkeletonPartsRenderer> skeletonPartsRenderers = GetComponentsInChildren<SkeletonPartsRenderer>().ToList();

                maxLayoutOrder = skeletonPartsRenderers.Max(x => x.MeshRenderer.sortingOrder);

                SpawnSeparatedRenderers(skeletonPartsRenderers);

                await AsyncHelper.NextFrame();
                SetOutlines(skeletonPartsRenderers);

                Get<HasOverrides_Identifier>()?.Initialize();

                transform.position = initialPosition;
                transform.localScale = initialScale;
            }
            catch (Exception e)
            {
                CustomLogger.instance?.LogException(e, "Error during Initialize()");
            }
        }

        [Button]
        private void Separate()
        {
            try
            {
                var slotsDatas = skeletonAnimation.skeletonDataAsset.GetSkeletonData(false).Slots;
                skeletonAnimation.separatorSlots.Clear();
                skeletonAnimation.separatorSlotNames.Clear();

                slotsDatas.ForEach(slotData =>
                {
                    Slot slot = skeletonAnimation.skeleton.FindSlot(slotData.Name);

                    skeletonAnimation.separatorSlots.Add(slot);
                    skeletonAnimation.separatorSlotNames.Add(slotData.Name);
                });

                skeletonAnimation.ReapplySeparatorSlotNames();
            }
            catch (Exception e)
            {
                CustomLogger.instance?.LogException(e, "Error during Initialize()");
            }
        }

        private void SpawnSeparatedRenderers(List<SkeletonPartsRenderer> skeletonPartsRenderers)
        {
            _paintables.Clear();
            SetLayers(skeletonPartsRenderers);

            Material lastMaterial = null;
            foreach (var item in skeletonPartsRenderers)
            {
                try
                {
                    item.gameObject.layer = LayerMask.NameToLayer("Snapshot");
                    if (item.name.Contains(Constants.DRAWABLE_ATLAS_PART) == false)
                    {
                        continue;
                    }

                    item.transform.localPosition -= Vector3.forward * item.MeshRenderer.sortingOrder / 1000f;

                    var paintable = item.gameObject.AddComponent<Paintable_Identifier_SlotSeparation>();
                    _paintables.Add(paintable);

                    Material material = paintable.Get<MeshRenderer>().sharedMaterial;
                    if (material == null)
                    {
                        paintable.Get<MeshRenderer>().sharedMaterial = lastMaterial;
                        material = paintable.Get<MeshRenderer>().sharedMaterial;
                    }

                    if (material == null)
                    {
                        continue;
                    }
                    else
                    {
                        if (material.mainTexture == null)
                        {
                            continue;
                        }

                        lastMaterial = material;
                    }

                    if (materialsWithTheirPaintables.ContainsKey(material) == false)
                    {
                        materialsWithTheirPaintables.Add(material, new() { paintable });
                    }
                    else
                    {
                        materialsWithTheirPaintables[material].Add(paintable);
                    }

                    var data = skeletonAnimation.separatorSlots.Find(x => x.Data.Name == item.name);
                    paintable.Init(this, regions.Find(x => x.name == data.Attachment.Name));
                }
                catch (Exception e)
                {
                    CustomLogger.instance?.LogException(e, "Error during SpawnSeparatedRenderers()");
                }
            }
        }

        private void SetLayers(List<SkeletonPartsRenderer> skeletonPartsRenderers)
        {
            try
            {
                _Override_GameobjectLayers gameobjectLayers = Get<_Override_GameobjectLayers>();

                foreach (var item in skeletonPartsRenderers)
                {
                    item.MeshRenderer.sortingLayerName = "Spine";

                    bool makeSnapshottable = item.name.Contains(Constants.DRAWABLE_ATLAS_PART);

                    if (item.name.Contains(Constants.OUTLINE_ATLAS_PART))
                    {
                        makeSnapshottable = true;
                    }

                    if (gameobjectLayers != null)
                    {
                        if (gameobjectLayers.makeAllSnapshottable)
                        {
                            makeSnapshottable = true;
                        }
                    }

                    if (gameobjectLayers != null)
                    {
                        if (gameobjectLayers.snapshottables.Contains(item.name))
                        {
                            makeSnapshottable = true;
                        }
                    }

                    if (makeSnapshottable)
                    {
                        item.gameObject.layer = Constants.SNAPSHOTTABLE;
                    }
                }
            }
            catch (Exception e)
            {
                CustomLogger.instance?.LogException(e, $"Error during SetSnapshottables({skeletonPartsRenderers})");
            }
        }

        private void SetOutlines(List<SkeletonPartsRenderer> skeletonPartsRenderers)
        {
            try
            {
                foreach (var item in _paintables)
                {
                    item.SetOutline(skeletonPartsRenderers.FindAll(x => Find(x)));

                    bool Find(SkeletonPartsRenderer skeletonRenderer)
                    {
                        bool contains = skeletonRenderer.name == item.name.Replace(Constants.DRAWABLE_ATLAS_PART, Constants.OUTLINE_ATLAS_PART);
                        return contains;
                    }
                }
            }
            catch (Exception e)
            {
                CustomLogger.instance?.LogException(e, "Error during SetOutlines()");
            }
        }

        public async Task AddGlitter(GameController gameController)
        {
            try
            {
                var glitterIdentifer = gameObject.AddComponent<Glitter_Identifier>();
                await glitterIdentifer.Init(gameController, _content.glitterMaterial_SlotSeparation);
            }
            catch (Exception e)
            {
                CustomLogger.instance?.LogException(e, "Error during AddGlitter()");
            }
        }

        [Button]
        public void DeleteAllCustomColliders()
        {
            foreach (var item in TryGetAll_List<MeshCollider>())
            {
                item.sharedMesh = item.GetComponent<MeshFilter>().sharedMesh;
            }
        }
    }
}