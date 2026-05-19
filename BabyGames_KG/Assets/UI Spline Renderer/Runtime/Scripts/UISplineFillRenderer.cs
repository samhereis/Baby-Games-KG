using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI_Spline_Renderer
{
    [RequireComponent(typeof(CanvasRenderer))]
    [ExecuteInEditMode]
    public class UISplineFillRenderer : MaskableGraphic
    {
        [SerializeField] UISplineRenderer _owner;

        public UISplineRenderer owner
        {
            get => _owner;
            set
            {
                if (_owner == value) return;
                _owner = value;
                SetVerticesDirty();
            }
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            if (_owner != null)
            {
                _owner.PopulateFillMesh(vh);
            }
            else
            {
                vh.Clear();
            }
        }
    }
}

