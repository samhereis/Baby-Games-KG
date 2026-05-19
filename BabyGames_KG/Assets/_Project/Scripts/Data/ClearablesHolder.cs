using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Loggers;
using Sirenix.OdinInspector;
using UnityEngine;
using Object = UnityEngine.Object;

namespace _Project.Scripts.Data
{
    [Serializable]
    public class ClearablesHolder
    {
        public static ClearablesHolder instance { get; private set; }

        [field: SerializeField] public List<Texture> clearableTextures { get; protected set; } = new();
        [field: SerializeField] public List<Sprite> clearableSprites { get; protected set; } = new();
        [field: SerializeField] public List<AssetBundle> clearableBundles { get; protected set; } = new();

        public ClearablesHolder()
        {
            instance = this;
        }

        public async Task ClearAsync()
        {
            await Clear();
            GC.Collect();
            await Resources.UnloadUnusedAssets();
        }

        [Button]
        public void Clear_Textures()
        {
            foreach (var item in clearableTextures)
            {
                Object.Destroy(item);
            }
        }

        [Button]
        public void Clear_Sprites()
        {
            foreach (var item in clearableSprites)
            {
                Object.Destroy(item);
            }
        }

        [Button]
        public async Task Clear_Bundles()
        {
            try
            {
                foreach (var item in clearableBundles)
                {
                    if (item == null) { continue; }

                    var t = item.GetAllAssetNames().ToList();
                    if (t.Exists(x => x.Contains("Localization")))
                    {
                        continue;
                    }
                    if (item.name.Contains("monoscripts.bundle"))
                    {
                        continue;
                    }

                    await item?.UnloadAsync(true);
                }
            }
            catch (Exception e)
            {
                CustomLogger.instance.LogException(e, "Error clearing bundles");
            }
        }

        [Button]
        public async Task Clear()
        {
            try
            {
                Clear_Textures();
                Clear_Sprites();
                await Clear_Bundles();

                DeleteNulls();
            }
            catch (Exception ex)
            {
                CustomLogger.instance.LogException(ex, "Error clearing junk");
            }
        }

        [Button]
        public void DeleteNulls()
        {
            clearableTextures.RemoveAll(x => x == null);
            clearableSprites.RemoveAll(x => x == null);
            clearableBundles.RemoveAll(x => x == null);
        }
    }
}