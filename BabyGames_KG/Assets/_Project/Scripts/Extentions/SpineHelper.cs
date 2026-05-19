using DataClasses;
using Loggers;
using Services;
using Spine;
using Spine.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using _Project.Scripts.Data;
using UnityEngine;
using Attachment = Spine.Attachment;

namespace Helpers
{
    public static class SpineHelper
    {
        private static ClearablesHolder _clearablesHolder_;

        private static ClearablesHolder _clearablesHolder
        {
            get
            {
                if (_clearablesHolder_ == null)
                {
                    _clearablesHolder_ = DiService.Get<ClearablesHolder>();
                }

                return _clearablesHolder_;
            }
        }

        public static Vector3 CalculatePosition(SkeletonAnimation skeleton, Slot slot, Attachment attachment)
        {
            Vector3 unityWorldPosition = Vector3.zero;

            Bone bone = slot.Bone;

            if (attachment is RegionAttachment regionAttachment)
            {
                float rotationRad = bone.Rotation * Mathf.Deg2Rad;
                float cos = Mathf.Cos(rotationRad);
                float sin = Mathf.Sin(rotationRad);

                float localX = regionAttachment.X * bone.WorldScaleX;
                float localY = regionAttachment.Y * bone.WorldScaleY;

                float worldX = bone.WorldX + (localX * cos - localY * sin);
                float worldY = bone.WorldY + (localX * sin + localY * cos);

                unityWorldPosition = skeleton.transform.TransformPoint(new Vector3(worldX, worldY, 0));
            }
            else if (attachment is MeshAttachment meshAttachment)
            {
                float[] vertices = new float[meshAttachment.WorldVerticesLength];
                meshAttachment.ComputeWorldVertices(slot, 0, meshAttachment.WorldVerticesLength, vertices, 0, 2);

                Vector2 averagePosition = Vector2.zero;
                int vertexCount = vertices.Length / 2;

                for (int i = 0; i < vertexCount; i++)
                {
                    averagePosition.x += vertices[i * 2];
                    averagePosition.y += vertices[i * 2 + 1];
                }

                averagePosition /= vertexCount;

                unityWorldPosition = skeleton.transform.TransformPoint(new Vector3(averagePosition.x, averagePosition.y, 0));
            }
            else if (attachment is PointAttachment pointAttachment)
            {
                pointAttachment.ComputeWorldPosition(bone, out float worldX, out float worldY);
                unityWorldPosition = skeleton.transform.TransformPoint(new Vector3(worldX, worldY, 0));
            }
            else if (attachment is BoundingBoxAttachment boundingBoxAttachment)
            {
                unityWorldPosition = skeleton.transform.TransformPoint(new Vector3(bone.WorldX, bone.WorldY, 0));
            }
            else
            {
                Debug.LogError("Attachment type not recognized or not supported for position calculation.");
            }

            return unityWorldPosition;
        }

        public static Vector3 CalculatePosition_2(SkeletonAnimation skeleton, Slot slot, Attachment attachment)
        {
            Vector3 unityWorldPosition = Vector3.zero;

            if (attachment is RegionAttachment regionAttachment)
            {
                float[] worldVertices = new float[8];
                regionAttachment.ComputeWorldVertices(slot.Bone, worldVertices, 0, 2);

                Vector2 averagePosition = Vector2.zero;
                for (int i = 0; i < 4; i++)
                {
                    averagePosition.x += worldVertices[i * 2];
                    averagePosition.y += worldVertices[i * 2 + 1];
                }

                averagePosition /= 4f;

                unityWorldPosition = skeleton.transform.TransformPoint(new Vector3(averagePosition.x, averagePosition.y, 0));
            }
            else if (attachment is MeshAttachment meshAttachment)
            {
                float[] vertices = new float[meshAttachment.WorldVerticesLength];
                meshAttachment.ComputeWorldVertices(slot, 0, meshAttachment.WorldVerticesLength, vertices, 0, 2);

                Vector2 averagePosition = Vector2.zero;
                int vertexCount = vertices.Length / 2;

                for (int i = 0; i < vertexCount; i++)
                {
                    averagePosition.x += vertices[i * 2];
                    averagePosition.y += vertices[i * 2 + 1];
                }

                averagePosition /= vertexCount;

                unityWorldPosition = skeleton.transform.TransformPoint(new Vector3(averagePosition.x, averagePosition.y, 0));
            }
            else if (attachment is PointAttachment pointAttachment)
            {
                pointAttachment.ComputeWorldPosition(slot.Bone, out float worldX, out float worldY);
                unityWorldPosition = skeleton.transform.TransformPoint(new Vector3(worldX, worldY, 0));
            }
            else if (attachment is BoundingBoxAttachment boundingBoxAttachment)
            {
                int worldVerticesCount = boundingBoxAttachment.WorldVerticesLength;
                float[] worldVertices = new float[worldVerticesCount];
                boundingBoxAttachment.ComputeWorldVertices(slot, 0, worldVerticesCount, worldVertices, 0, 2);

                Vector2 averagePosition = Vector2.zero;
                int vertexCount = worldVerticesCount / 2;
                for (int i = 0; i < vertexCount; i++)
                {
                    averagePosition.x += worldVertices[i * 2];
                    averagePosition.y += worldVertices[i * 2 + 1];
                }

                averagePosition /= vertexCount;

                unityWorldPosition = skeleton.transform.TransformPoint(new Vector3(averagePosition.x, averagePosition.y, 0));
            }
            else
            {
                Debug.LogError("Attachment type not recognized or not supported for position calculation.");
            }

            return unityWorldPosition;
        }

        public static Texture2D ExtractRegion(AtlasRegion atlasRegion, RenderTexture renderTexture)
        {
            Texture2D newTexture = new Texture2D(atlasRegion.width, atlasRegion.height, TextureFormat.RGBA32, false);
            _clearablesHolder.clearableTextures.SafeAdd(newTexture);

            try
            {
                RenderTexture.active = renderTexture;

                int x = atlasRegion.x;
                int y = renderTexture.height - atlasRegion.y - atlasRegion.height;

                Rect region = new Rect(x, y, atlasRegion.width, atlasRegion.height);
                newTexture.ReadPixels(region, 0, 0);
                newTexture.Apply();
            }
            catch (Exception ex)
            {
                CustomLogger.instance?.LogException(ex);
            }
            finally
            {
                RenderTexture.active = null;
            }

            return newTexture;
        }

        public static async Task Separate(this SkeletonAnimation skeletonAnimation)

        {
            try
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

                await AsyncHelper.NextFrame();

                skeletonAnimation.GetComponent<SkeletonRenderSeparator>().AddPartsRenderers(skeletonAnimation.separatorSlots.Count + 1);
            }
            catch (Exception e)
            {
                CustomLogger.instance?.LogException(e, "Error during Initialize()");
            }
        }

        public static async Task Separate(this SkeletonGraphic skeletonAnimation)

        {
            try
            {
                try
                {
                    var slotsDatas = skeletonAnimation.skeletonDataAsset.GetSkeletonData(false).Slots;
                    skeletonAnimation.separatorSlots.Clear();
                    skeletonAnimation.separatorSlotNames.Clear();

                    slotsDatas.ForEach(slotData =>
                    {
                        Slot slot = skeletonAnimation.Skeleton.FindSlot(slotData.Name);

                        skeletonAnimation.separatorSlots.Add(slot);
                        skeletonAnimation.separatorSlotNames.Add(slotData.Name);
                    });

                    skeletonAnimation.ReapplySeparatorSlotNames();
                }
                catch (Exception e)
                {
                    CustomLogger.instance?.LogException(e, "Error during Initialize()");
                }

                await AsyncHelper.NextFrame();

                skeletonAnimation.GetComponent<SkeletonRenderSeparator>().AddPartsRenderers(skeletonAnimation.separatorSlots.Count + 1);
            }
            catch (Exception e)
            {
                CustomLogger.instance?.LogException(e, "Error during Initialize()");
            }
        }

        public static async Task<Texture2D> CaptureMeshRenderersSnapshot(List<MeshRenderer> renderers, LayerMask captureLayer, int textureWidth, int textureHeight)
        {
            if (renderers == null || renderers.Count == 0)
                return null;

            int mask = captureLayer.value;
            if (mask == 0)
            {
                Debug.LogError("Invalid captureLayer: must represent a non-zero single layer.");
                return null;
            }

            int layerIndex = (int)Mathf.Log(mask, 2);

            Dictionary<GameObject, int> originalLayers = new Dictionary<GameObject, int>();
            foreach (var renderer in renderers)
            {
                GameObject go = renderer.gameObject;
                originalLayers[go] = go.layer;
                go.layer = layerIndex;
            }

            await AsyncHelper.NextFrame();

            // Create a temporary camera.
            GameObject cameraGO = new GameObject("TempCaptureCamera");
            Camera captureCamera = cameraGO.AddComponent<Camera>();
            captureCamera.orthographic = true;
            captureCamera.orthographicSize = 5;
            captureCamera.cullingMask = 1 << layerIndex;
            captureCamera.clearFlags = CameraClearFlags.SolidColor;
            captureCamera.backgroundColor = new Color(0, 0, 0, 0);

            Bounds combinedBounds = renderers[0].bounds;
            foreach (var renderer in renderers.Skip(1))
                combinedBounds.Encapsulate(renderer.bounds);

            // Position the camera to cover the combined bounds.
            captureCamera.transform.position = combinedBounds.center - new Vector3(0, 0, 10);

            // Create a RenderTexture and assign it to the camera.
            RenderTexture rt = new RenderTexture(textureWidth, textureHeight, 24);
            captureCamera.targetTexture = rt;

            // Render the camera to the RenderTexture.
            RenderTexture previousRT = RenderTexture.active;
            RenderTexture.active = rt;
            captureCamera.Render();

            // Capture the entire RenderTexture.
            Texture2D snapshot = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false);
            snapshot.ReadPixels(new Rect(0, 0, textureWidth, textureHeight), 0, 0);
            snapshot.Apply();

            // Cleanup: restore the active RenderTexture, destroy temporary objects.
            RenderTexture.active = previousRT;
            GameObject.DestroyImmediate(cameraGO);
            rt.Release();
            GameObject.DestroyImmediate(rt);

            // Restore original layers.
            foreach (var kvp in originalLayers)
            {
                kvp.Key.layer = kvp.Value;
            }

            return snapshot;
        }
    }
}