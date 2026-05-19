using _Project._Modes.Orchestra.Scripts;
using DataClasses;
using Helpers;
using Helpers.ImageCroppers;
using Identifiers;
using Services;
using Sirenix.OdinInspector;
using Spine.Unity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using _Project.Scripts.Data;
using UnityEngine;

namespace _Project.Scripts._Modes.Sorting
{
    [RequireComponent(typeof(SkeletonAnimation))]
    public class OrchestraCharacter_Identifier : IdentifierBase
    {
        public LayerMask captureLayer;

        public List<OrchestraWaveUnit> waveData = new();

        public string sortingLayerName = "Default";

        [Button]
        public async Task Initialize()
        {
            DiService.Inject(this);

            await Get<SkeletonAnimation>()?.Separate();

            foreach (var orchestraWaveUnit in waveData)
            {
                orchestraWaveUnit.orchestraCharacter_Identifier = this;
            }

            foreach (var item in TryGetAll_List<SkeletonPartsRenderer>())
            {
                item.MeshRenderer.sortingLayerName = sortingLayerName;
            }
        }

        [Button]
        public async Task<Texture2D> GetInstrumentForWave(int waveIndex, int objectNameIndex)
        {
            Texture2D result = null;

            List<MeshRenderer> meshRenderers = new List<MeshRenderer>();

            if (waveIndex == 0)
            {
                foreach (var meshRenderer in GetComponentsInChildren<MeshRenderer>(true))
                {
                    var waveData = this.waveData[waveIndex];

                    waveData.relatedSkeletonParts.Add(meshRenderer);
                    meshRenderers.Add(meshRenderer);
                }
            }
            else
            {
                foreach (var meshRenderer in GetComponentsInChildren<MeshRenderer>(true))
                {
                    var waveData = this.waveData[waveIndex];

                    if (waveData.objectNames[objectNameIndex].key.Exists(x =>
                    {
                        if (waveData.searchMode == OrchestraWaveUnit.SearchMode.Contains)
                        {
                            return meshRenderer.name.Contains(x);
                        }
                        else
                        {
                            return meshRenderer.name == x;
                        }
                    }))
                    {
                        waveData.relatedSkeletonParts.Add(meshRenderer);
                        meshRenderers.Add(meshRenderer);
                    }
                }
            }

            if (meshRenderers.Count == 0)
            {
                return result;
            }

            result = await SpineHelper.CaptureMeshRenderersSnapshot(meshRenderers, captureLayer, Screen.width, Screen.height);
            ClearablesHolder.instance.clearableTextures.SafeAdd(result);

            result = await ImageCropper_Implementation2.CropAlphas_Rectangular(result, 5, 5);
            ClearablesHolder.instance.clearableTextures.SafeAdd(result);

            return result;
        }

        [Button]
        public void Setup(int waveIndex, int objectNameIndex)
        {
            var waveData = this.waveData[waveIndex];
            foreach (var meshRenderer in GetComponentsInChildren<MeshRenderer>(true))
            {
                if (waveData.objectNames[objectNameIndex].key.Exists(x =>
                {
                    if (waveData.searchMode == OrchestraWaveUnit.SearchMode.Contains)
                    {
                        return meshRenderer.name.Contains(x);
                    }
                    else
                    {
                        return meshRenderer.name == x;
                    }
                }))
                {
                    waveData.relatedSkeletonParts.Add(meshRenderer);
                }
            }
        }

        public static void SaveTextureToDesktop(Texture2D texture, string fileName)
        {
            if (texture == null)
            {
                Debug.LogError("Texture is null, cannot save.");
                return;
            }

            var pngData = texture.EncodeToPNG();
            if (pngData != null)
            {
                var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                var fullPath = Path.Combine(desktopPath, fileName);
                File.WriteAllBytes(fullPath, pngData);
                Debug.Log("Texture saved to: " + fullPath);
            }
            else
            {
                Debug.LogError("Failed to encode texture to PNG.");
            }
        }
    }
}