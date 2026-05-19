using PaintCore;
using PaintIn3D;
using Spine;
using UnityEngine;

namespace Modes.Coloring
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshCollider))]
    [RequireComponent(typeof(CwPaintableMeshTexture))]
    public class Paintable_Identifier_Background : Paintable_Identifier_Basic
    {
        public override void Init(Spine_Identifier spine_Identifier, AtlasRegion atlasRegion)
        {
            base.Init(spine_Identifier, atlasRegion);
            paintableTexture.UndoRedo = CwPaintableTexture.UndoRedoType.FullTextureCopy;
        }
    }
}