using DataClasses;
using Helpers;
using Identifiers;
using Loggers;
using Services;
using Sirenix.OdinInspector;
using Spine.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using _Project.Scripts.Data;
using UnityEngine;

namespace Modes.Coloring
{
    [DisallowMultipleComponent]
    public class Glitter_Identifier : IdentifierBase
    {
        private static GameController _gameController;

        [field: SerializeField] public bool autoSet { get; set; } = false;

        [SerializeField] private MeshRenderer _meshRenderer;
        [SerializeField] private Material _copySettingsFrom;

        private void Awake()
        {
            DiService.Inject(this);
        }

        public async Task Init(GameController gameController, Material copySettingsFrom)
        {
            if (_gameController == null)
            {
                _gameController = DiService.Get<GameController>();
            }

            if (_meshRenderer == null)
            {
                _meshRenderer = GetComponent<MeshRenderer>();
            }

            _copySettingsFrom = copySettingsFrom;

            SetGlitterActive(true);
            CopyMaterialSettings();

            if (gameController.model.hasLocalSave == false)
            {
                await DeleteGlitterAlhpas();
            }
        }

        private void Update()
        {
            if (autoSet == true)
            {
                SetGlitterActive(true);
                CopyMaterialSettings();
            }
        }

        public List<Material> materials = new();

        [Button]
        public async Task DeleteGlitterAlhpas()
        {
            SkeletonAnimation skeletonDataAsset = GetComponent<SkeletonAnimation>();
            if (skeletonDataAsset?.skeletonDataAsset == null)
            {
                materials = _meshRenderer.sharedMaterials.ToList();
            }
            else
            {
                materials = skeletonDataAsset.skeletonDataAsset.atlasAssets[0].Materials.ToList();
            }

            try
            {
                foreach (var item in materials)
                {
                    if (item == null)
                    {
                        continue;
                    }

                    if (item.mainTexture == null)
                    {
                        continue;
                    }

                    Texture2D texture = item.mainTexture.MakeTexture2D(ClearablesHolder.instance.clearableTextures);
                    item.mainTexture = await DeleteGlitterAlhpas(texture);
                }
            }
            catch (Exception exception)
            {
                CustomLogger.instance?.LogException(exception, "Error openning empty scene");
            }
        }

        public static async Task<Texture2D> DeleteGlitterAlhpas(Texture2D texture)
        {
            if (texture == null)
            {
                return texture;
            }

            Color[] colors = texture.GetPixels();
            float nonGlitterAlpha = _gameController.model.gameSettings.nonGlitterAlpha;

            await Task.Run(() =>
            {
                for (int i = 0; i < colors.Length; i++)
                {
                    Color color = colors[i];

                    if (color.a > nonGlitterAlpha)
                    {
                        colors[i].a = nonGlitterAlpha;
                    }
                }
            });

            texture.SetPixels(colors);
            texture.Apply();

            return texture;
        }

        public void SetGlitterActive(bool isActive)
        {
            if (_meshRenderer == null)
            {
                return;
            }

            foreach (var materials in _meshRenderer.sharedMaterials)
            {
                if (materials == null)
                {
                    continue;
                }

                materials.SetFloat("_GlitterActive", isActive ? 1 : 0);
            }
        }

        public void CopyMaterialSettings()
        {
            if (_gameController.model.gameSettings == null)
            {
                return;
            }

            foreach (var materials in _meshRenderer.sharedMaterials)
            {
                if (materials == null)
                {
                    continue;
                }

                materials.renderQueue = _copySettingsFrom.renderQueue;
                materials.shader = _copySettingsFrom.shader;

                foreach (var setting in _gameController.model.gameSettings.drawSettings.settings_float)
                {
                    if (materials.HasFloat(setting))
                    {
                        materials.SetFloat(setting, _copySettingsFrom.GetFloat(setting));
                    }
                }
            }
        }

        public static Vector3 PixelUVToWorld(Vector2 pixel, MeshRenderer renderer)
        {
            Texture tex = renderer.material.mainTexture;
            int w = tex.width, h = tex.height;
            Vector2 uv = new Vector2(pixel.x / w, pixel.y / h);

            Camera cam = Camera.main;
            Vector3 screenPos = cam.WorldToScreenPoint(renderer.bounds.center);
            screenPos.x = screenPos.x - (renderer.bounds.size.x / 2) + uv.x * renderer.bounds.size.x;
            screenPos.y = screenPos.y - (renderer.bounds.size.y / 2) + uv.y * renderer.bounds.size.y;

            Ray ray = cam.ScreenPointToRay(screenPos);
            if (Physics.Raycast(ray, out RaycastHit hit) &&
                hit.collider.gameObject == renderer.gameObject)
            {
                return hit.point;
            }
            else
            {
                return renderer.transform.position;
            }
        }
    }
}