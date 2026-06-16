// Unity 6000.0.36 breaks SpriteRenderer with MaterialPropertyBlock, so manually draw the sprites?
#if UNITY_6000_0_OR_NEWER && !(UNITY_6000_0_0 || UNITY_6000_0_1 || UNITY_6000_0_2 || UNITY_6000_0_3 || UNITY_6000_0_4 || UNITY_6000_0_5 || UNITY_6000_0_6 || UNITY_6000_0_7 || UNITY_6000_0_8 || UNITY_6000_0_9 || UNITY_6000_0_10 || UNITY_6000_0_11 || UNITY_6000_0_12 || UNITY_6000_0_13 || UNITY_6000_0_14 || UNITY_6000_0_15 || UNITY_6000_0_16 || UNITY_6000_0_17 || UNITY_6000_0_18 || UNITY_6000_0_19 || UNITY_6000_0_20 || UNITY_6000_0_21 || UNITY_6000_0_22 || UNITY_6000_0_23 || UNITY_6000_0_24 || UNITY_6000_0_25 || UNITY_6000_0_26 || UNITY_6000_0_27 || UNITY_6000_0_28 || UNITY_6000_0_29 || UNITY_6000_0_30 || UNITY_6000_0_31 || UNITY_6000_0_32 || UNITY_6000_0_33 || UNITY_6000_0_34 || UNITY_6000_0_35)
	#define CW_SPRITERENDERER_BROKEN
#endif
using UnityEngine;
using System.Collections.Generic;

namespace PaintIn2D
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(MeshFilter))]
	[RequireComponent(typeof(MeshRenderer))]
	[AddComponentMenu("")]
	public class CwSpriteRenderer : MonoBehaviour
	{
		public static bool IsBroken
		{
			get
			{
				#if CW_SPRITERENDERER_BROKEN
					return true;
				#else
					return false;
				#endif
			}
		}

		private static Vector3[] tempPositions = new Vector3[4];
		private static Vector2[] tempCoords    = new Vector2[4];
		private static int[]     tempIndices   = new int[] { 0, 2, 1, 0, 3, 2 };
		private static Color[]   tempColors    = new Color[4];

		[System.NonSerialized]
		private SpriteRenderer parent;

		[System.NonSerialized]
		private Mesh generatedMesh;

		[System.NonSerialized]
		private MeshFilter cachedMeshFilter;

		[System.NonSerialized]
		private MeshRenderer cachedMeshRenderer;

		private static MaterialPropertyBlock properties;

		private static Dictionary<SpriteRenderer, CwSpriteRenderer> spriteRendererMap = new Dictionary<SpriteRenderer, CwSpriteRenderer>();

		public static CwSpriteRenderer TryFix(SpriteRenderer parent)
		{
			#if CW_SPRITERENDERER_BROKEN
				var sr = default(CwSpriteRenderer);

				if (spriteRendererMap.TryGetValue(parent, out sr) == false)
				{
					parent.forceRenderingOff = true;

					var go = new GameObject("CwSpriteRenderer");

					sr = go.AddComponent<CwSpriteRenderer>();

					sr.parent = parent;

					spriteRendererMap.Add(parent, sr);

					go.transform.SetParent(parent.transform, false);

					return sr;
				}

				sr.DrawSprite();
			#endif
			return null;
		}

		protected virtual void OnEnable()
		{
			cachedMeshFilter   = GetComponent<MeshFilter>();
			cachedMeshRenderer = GetComponent<MeshRenderer>();
		}

		protected virtual void OnDestroy()
		{
			DestroyImmediate(generatedMesh);

			if (parent != null)
			{
				spriteRendererMap.Remove(parent);
			}
		}

		protected virtual void LateUpdate()
		{
			if (parent != null && parent.transform == transform.parent)
			{
				DrawSprite();
			}
			else
			{
				DestroyImmediate(gameObject);
			}
		}

		private void DrawSprite()
		{
			if (enabled == true && parent != null && parent.sharedMaterial != null)
			{
				var sprite = parent.sprite;

				if (sprite != null)
				{
					var spriteSize  = sprite.rect.size / sprite.pixelsPerUnit;
					var pivotOffset = new Vector2(sprite.pivot.x / sprite.rect.width - 0.5f, sprite.pivot.y / sprite.rect.height - 0.5f) * -spriteSize;

					tempPositions[0] = new Vector3(-spriteSize.x / 2.0f + pivotOffset.x, -spriteSize.y / 2.0f + pivotOffset.y, 0.0f);
					tempPositions[1] = new Vector3( spriteSize.x / 2.0f + pivotOffset.x, -spriteSize.y / 2.0f + pivotOffset.y, 0.0f);
					tempPositions[2] = new Vector3( spriteSize.x / 2.0f + pivotOffset.x,  spriteSize.y / 2.0f + pivotOffset.y, 0.0f);
					tempPositions[3] = new Vector3(-spriteSize.x / 2.0f + pivotOffset.x,  spriteSize.y / 2.0f + pivotOffset.y, 0.0f);

					tempCoords[0] = new Vector2(sprite.rect.xMin / sprite.texture.width, sprite.rect.yMin / sprite.texture.height);
					tempCoords[1] = new Vector2(sprite.rect.xMax / sprite.texture.width, sprite.rect.yMin / sprite.texture.height);
					tempCoords[2] = new Vector2(sprite.rect.xMax / sprite.texture.width, sprite.rect.yMax / sprite.texture.height);
					tempCoords[3] = new Vector2(sprite.rect.xMin / sprite.texture.width, sprite.rect.yMax / sprite.texture.height);

					tempColors[0] = tempColors[1] = tempColors[2] = tempColors[3] = parent.color;

					if (generatedMesh == null)
					{
						generatedMesh = new Mesh();
						generatedMesh.hideFlags = HideFlags.DontSave;
						generatedMesh.vertices  = tempPositions;
						generatedMesh.uv        = tempCoords;
						generatedMesh.colors    = tempColors;
						generatedMesh.triangles = tempIndices;

						cachedMeshFilter.sharedMesh = generatedMesh;
					}
					else
					{
						generatedMesh.vertices  = tempPositions;
						generatedMesh.uv        = tempCoords;
						generatedMesh.colors    = tempColors;
					}

					if (properties == null)
					{
						properties = new MaterialPropertyBlock();
					}

					parent.GetPropertyBlock(properties);
					cachedMeshRenderer.SetPropertyBlock(properties);

					cachedMeshRenderer.sharedMaterial   = parent.sharedMaterial;
					cachedMeshRenderer.sortingLayerID   = parent.sortingLayerID;
					cachedMeshRenderer.sortingLayerName = parent.sortingLayerName;
					cachedMeshRenderer.sortingOrder     = parent.sortingOrder;

					cachedMeshRenderer.gameObject.layer = parent.gameObject.layer;
				}
			}
		}
	}
}