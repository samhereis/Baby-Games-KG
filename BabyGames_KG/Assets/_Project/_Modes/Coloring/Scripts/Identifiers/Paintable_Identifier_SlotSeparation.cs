using DataClasses;
using Helpers;
using Loggers;
using PaintCore;
using PaintIn3D;
using Sirenix.OdinInspector;
using Spine;
using Spine.Unity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using _Project.Scripts.Data;
using UnityEngine;
using Slot = Spine.Slot;

namespace Modes.Coloring
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MeshCollider))]
    [RequireComponent(typeof(CwPaintableMesh))]
    [RequireComponent(typeof(CwPaintableMeshTexture))]
    public class Paintable_Identifier_SlotSeparation : Paintable_Identifier_Basic
    {
        public Action onSetOutputTexture;

        [field: SerializeField] public Spine_Identifier spine_Identifier { get; private set; }
        [field: SerializeField] public CwModel cwModel { get; private set; }
        [field: SerializeField] public SkeletonPartsRenderer skeletonPartsRenderer { get; private set; }
        [field: SerializeField] public AtlasRegion atlasRegion { get; private set; }

        [SerializeField] private int _sortingOrder;
        [SerializeField] private PaintableAdditiveData _additiveData = new();
        [SerializeField] private Vector3 _meshPosition;

        public Texture2D outputTexture;

        private bool _isCopy = false;

        private void Update()
        {
            if (_isCopy == true)
            {
                _meshCollider.enabled = false;
            }
        }

        public async override void Init(Spine_Identifier spine_Identifier, AtlasRegion atlasRegion)
        {
            try
            {
                this.spine_Identifier = spine_Identifier;
                this.atlasRegion = atlasRegion;

                base.Init(spine_Identifier, atlasRegion);

                paintableTexture.UndoRedo = CwPaintableTexture.UndoRedoType.LocalCommandCopy;
                paintableTexture.Filter = CwPaintableTexture.FilterType.Point;

                skeletonPartsRenderer = Get<SkeletonPartsRenderer>();
                _sortingOrder = skeletonPartsRenderer.MeshRenderer.sortingOrder;
                cwModel = Get<CwModel>();

                paintableTexture.CopySize();

                await AsyncHelper.DelayFloat(0.25f);

                if (gameObject.name.Contains("_copy"))
                {
                    MarkAsCopy();
                }
                else
                {
                    await UpdateColliders();
                }
            }
            catch (Exception e)
            {
                CustomLogger.instance?.LogException(e);
            }
        }

        public void SetOutline(List<SkeletonPartsRenderer> outlinesAndStatics)
        {
            outlinesAndStatics.ForEach(x => { _additiveData.additivesSortingOrders.Add(x, x.MeshRenderer.sortingOrder); });
        }

        public Texture2D GetOutputTexture()
        {
            outputTexture = SpineHelper.ExtractRegion(atlasRegion, paintableTexture.Current);
            ClearablesHolder.instance.clearableTextures.SafeAdd(outputTexture);

            return outputTexture;
        }

        public void MarkAsCopy()
        {
            _isCopy = true;
        }

        [Button]
        public async Task UpdateColliders()
        {
            try
            {
                while (paintableTexture.Current == null)
                {
                    await AsyncHelper.NextFrame();
                }

                string nameToSearch = gameObject.name.Replace(Constants.DRAWABLE_ATLAS_PART, Constants.OUTLINE_ATLAS_PART);

                if (spine_Identifier.TryGet<_Override_PaintableColliders>(out var paintableColliders))
                {
                    if (paintableColliders.deleteCollider.Contains(gameObject.name))
                    {
                        _meshCollider.sharedMesh = null;
                        return;
                    }

                    if (paintableColliders.ignoreAll)
                    {
                        return;
                    }

                    if (paintableColliders.ignores.Contains(nameToSearch))
                    {
                        return;
                    }

                    if (paintableColliders.ignores.Contains(gameObject.name))
                    {
                        return;
                    }
                }

                Slot slot = spine_Identifier.Get<SkeletonAnimation>().separatorSlots.Find(x => x.Data.Name == nameToSearch.Replace("outline", "back"));
                if (slot != null)
                {
                    if (spine_Identifier.collidersCalculateMode == Spine_Identifier.CollidersCalculateMode.First)
                    {
                        _meshPosition = SpineHelper.CalculatePosition(spine_Identifier.Get<SkeletonAnimation>(), slot, slot.Attachment);
                    }
                    else if (spine_Identifier.collidersCalculateMode == Spine_Identifier.CollidersCalculateMode.Second)
                    {
                        _meshPosition = SpineHelper.CalculatePosition_2(spine_Identifier.Get<SkeletonAnimation>(), slot, slot.Attachment);
                    }
                }
                else
                {
                    Debug.LogWarning($"{nameToSearch} is not found");
                }

                var texture = GetOutputTexture();
                await AsyncHelper.NextFrame();

                await AddPolygonColliderSprite(texture, spine_Identifier.transform.localScale.x, spine_Identifier.transform.localScale.y, _meshPosition);
                Destroy(texture);
            }
            catch (Exception ex)
            {
                CustomLogger.instance?.LogException(ex, $"Error during UpdateColliders {spine_Identifier.gameObject.name} - {gameObject.name}");
            }
        }

        protected async Task AddPolygonColliderSprite(Texture2D tex, float scaleX, float scaleY, Vector3 position)
        {
            try
            {
                Vector3? offset = spine_Identifier?.Get<_Override_PaintableColliders>()?.offsets?.Find(x => x.key == gameObject.name)?.value;
                if (offset == null)
                {
                    offset = Vector3.zero;
                }

                Vector3? scale = spine_Identifier?.Get<_Override_PaintableColliders>()?.scales?.Find(x => x.key == gameObject.name)?.value;
                if (scale == null || scale == Vector3.zero)
                {
                    scale = Vector3.one;
                }

                GameObject newSprite = new GameObject("Sprite-" + name);
                newSprite.transform.position = position + offset.Value;
                newSprite.transform.position -= spine_Identifier.initialPosition;
                newSprite.transform.localScale = spine_Identifier.initialScale;
                newSprite.transform.localScale /= spine_Identifier.scale;
                newSprite.transform.localScale = new(newSprite.transform.localScale.x * scale.Value.x, newSprite.transform.localScale.y * scale.Value.y,
                    newSprite.transform.localScale.z * scale.Value.z);

                newSprite.transform.parent = transform;

                Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));

                var spriteRenderer = newSprite.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = sprite;

                var polygonCollider2D = newSprite.AddComponent<PolygonCollider2D>();
                polygonCollider2D.useDelaunayMesh = true;

                var ridigbody = newSprite.AddComponent<Rigidbody2D>();
                ridigbody.bodyType = RigidbodyType2D.Kinematic;

                transform.localScale /= spine_Identifier.transform.localScale.z;
                await AsyncHelper.NextFrame();

                Mesh mesh = polygonCollider2D.CreateMesh(true, true, false);
                await AsyncHelper.NextFrame();

                _meshCollider.sharedMesh = mesh;
                transform.localScale = Vector3.one;

                Destroy(newSprite);
                Destroy(polygonCollider2D.gameObject);
            }
            catch (Exception ex)
            {
                CustomLogger.instance?.LogException(ex, $"Error during AddPolygonColliderSprite {spine_Identifier.gameObject.name} - {gameObject.name}");
            }
        }
    }
}