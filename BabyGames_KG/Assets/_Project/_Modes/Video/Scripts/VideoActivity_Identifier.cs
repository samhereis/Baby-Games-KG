using DataClasses;
using Interfaces.Services;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using _Project.Scripts.Data;
using UnityEditor;
using UnityEngine;
using UnityEngine.Video;

namespace Identifiers
{
    public class VideoActivity_Identifier : _ActivityBase_Identifier
    {
        public string videoName;
        public string videoDisplayName;
        public VideoClip videoClip;

        public override void UpdateData(Activity activity)
        {
            base.UpdateData(activity);

            type = ActivityType.Video;

#if UNITY_EDITOR
            var videoPath = Directory.GetFiles(assetFolderPath).FirstOrDefault(x => x.EndsWith(".mp4"));
            var iconPath = Directory.GetFiles(assetFolderPath).FirstOrDefault(x => x.EndsWith(".jpg"));
            if (iconPath == null) { iconPath = Directory.GetFiles(assetFolderPath).FirstOrDefault(x => x.EndsWith(".png")); }

            if (videoPath != null && iconPath != null)
            {
                videoPath = videoPath.Replace("\\", "/");
                iconPath = iconPath.Replace("\\", "/");

                var foundVideo = AssetDatabase.LoadAssetAtPath<VideoClip>(videoPath);
                var foundIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(iconPath);

                if (foundVideo != null && foundIcon != null)
                {
                    string newVideoName = $"{gameObject.name.ToLower()}_video";
                    string newIconName = $"{gameObject.name.ToLower()}_icon";

                    AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(foundVideo), newVideoName);
                    AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(foundIcon), newIconName);

                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();

                    foundVideo.name = newVideoName;
                    foundIcon.name = newIconName;

                    videoClip = foundVideo;
                    videoName = foundVideo.name;
                }
            }

            activity.displayName = videoDisplayName;
#endif
        }

        public async Task<VideoClip> GetVideoClipAsync(Activity activity, IContentDeliveryService contentDeliveryService, Action<float> onDownloadingUpdate = null)
        {
            var bundle = await contentDeliveryService.GetAsset(activity.GetUrl(), videoName, activity.version, onDownloadingUpdate);
            var foundVideoClip = bundle.LoadAsset<VideoClip>(videoName);
            var assets = bundle.LoadAllAssets();

            ClearablesHolder.instance.clearableBundles.Add(bundle);

            return videoClip;
        }

        public override async Task DownloadAdditionals(Activity activity, IContentDeliveryService contentDeliveryService, Action<float> onDownloadingUpdate = null)
        {
            var bundle = await contentDeliveryService.GetAsset(activity.GetUrl(), videoName, activity.version, onDownloadingUpdate);
            bundle.Unload(true);
        }
    }
}