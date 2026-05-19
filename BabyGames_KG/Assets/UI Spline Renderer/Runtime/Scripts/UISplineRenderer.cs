using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Serialization; // 이 라인을 지우지 말 것.
using UnityEngine.Splines;
using UnityEngine.UI;
using Math = System.Math;
#if LETAI_TRUESHADOW
using LeTai.TrueShadow;
using LeTai.TrueShadow.PluginInterfaces;
#endif

[assembly: InternalsVisibleTo("UISplineRenderer.Editor", AllInternalsVisible = true)]

namespace UI_Spline_Renderer
{
    public enum UVMode
    {
        Tile,
        RepeatPerSegment,
        Stretch
    }

    public enum OffsetMode
    {
        Distance,
        Normalized
    }
    
    [RequireComponent(typeof(CanvasRenderer))]
    [ExecuteInEditMode]
    public class UISplineRenderer : MaskableGraphic
#if LETAI_TRUESHADOW
        , ITrueShadowCustomHashProvider
#endif
    {
        #region Inspector Fields

        [FormerlySerializedAs("splineContainer")][SerializeField] SplineContainer _splineContainer;
        [SerializeField] bool _fitPosition;
        
        [SerializeField] float _width = 10;
        [SerializeField] AnimationCurve _widthCurve = AnimationCurve.Linear(0, 1, 1, 1);
        [SerializeField] bool _smooth;
        [SerializeField] bool _roundEnds;
        [SerializeField] int _resolution = 5;
        [SerializeField] Vector2 _clipRange = new Vector2(0, 1);
        
        [SerializeField] Gradient _colorGradient = new Gradient();
        [SerializeField] bool _recursiveColor = true;
        [SerializeField] bool _recursiveMaterial = true;
        [SerializeField] Texture m_Texture;
        [SerializeField] bool _defaultTextureInitialized;

        [SerializeField] UVMode _uvMode;
        [SerializeField] Vector2 _uvMultiplier = new Vector2(1, 1);
        [SerializeField] Vector2 _uvOffset;

        [SerializeField] bool _fill;
        [SerializeField] Color _fillColor = Color.white;
        [SerializeField] Material _fillMaterial;
        [SerializeField] bool _fillRaycast = true;
        [SerializeField] UISplineFillRenderer _fillRenderer;

        [SerializeField] Sprite _startImageSprite;
        [SerializeField] float _startImageSize = 32;
        [SerializeField] OffsetMode _startImageOffsetMode;
        [SerializeField] float _startImageOffset;
        [SerializeField] float _normalizedStartImageOffset;

        [SerializeField] Sprite _endImageSprite;
        [SerializeField] float _endImageSize = 32;
        [SerializeField] OffsetMode _endImageOffsetMode;
        [SerializeField] float _endImageOffset;
        [SerializeField] float _normalizedEndImageOffset = 1;

        [SerializeField] bool _keepZeroZ = true;
        [SerializeField] bool _keepBillboard = true;

        #endregion

        #region Private Fields

        public List<Image> startImages = new();
        public List<Image> endImages = new();

#pragma warning disable CS0414
        bool _needToResample;
#pragma warning restore CS0414

        VertexHelper _vh;
        HashSet<IDisposable> _disposables = new HashSet<IDisposable>();
#if LETAI_TRUESHADOW
        TrueShadow _trueShadow;
#endif
        int _lastSiblingIndex = -1;

        #endregion

        #region Properties

        public SplineContainer splineContainer
        {
            get
            {
                if (_splineContainer == null) _splineContainer = GetComponent<SplineContainer>();
                return _splineContainer;
            }
            set
            {
                var isNew = _splineContainer != value;
                _splineContainer = value;
                if(isNew)
                {
                    SetAllDirty();
                    UpdateRaycastTargetRect();
                }
            }
        }

        public LineTexturePreset lineTexturePreset
        {
            get
            {
                if (m_Texture == UISplineRendererSettings.Instance.defaultLineTexture) return LineTexturePreset.Default;
                if (m_Texture == UISplineRendererSettings.Instance.uvTestLineTexture) return LineTexturePreset.UVTest;
                return LineTexturePreset.Custom;
            }
            set
            {
                switch (value)
                {
                    case LineTexturePreset.Default:
                        texture = UISplineRendererSettings.Instance.defaultLineTexture;
                        break;
                    case LineTexturePreset.UVTest:
                        texture = UISplineRendererSettings.Instance.uvTestLineTexture;
                        break;
                    case LineTexturePreset.Custom:
                        Debug.LogWarning("[UI Spline Renderer] If you want to change the line texture, " +
                                         "just set value to the \"texture\" property. " +
                                         "Then It will be automatically changed to LineTexturePreset.Custom");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(value), value, null);
                }
            }
        }

        public override Texture mainTexture => m_Texture == null ? s_WhiteTexture : m_Texture;

        public Texture texture
        {
            get => m_Texture;
            set
            {
                if (value != null && m_Texture == value) return;
                m_Texture = value;
                SetMaterialDirty();
                UpdateTrueShadowCustomHash();
            }
        }

        public override Color color
        {
            get => base.color;
            set
            {
                base.color = value;
                if (recursiveColor)
                {
                    UpdateGraphicColors();
                    UpdateTrueShadowCustomHash();
                }
            }
        }

        public override Material material
        {
            get => base.material;
            set
            {
                base.material = value;
                if (recursiveMaterial)
                {
                    ManipulateOtherGraphics(x => x.material = value);
                    UpdateTrueShadowCustomHash();
                }
            }
        }

        public bool recursiveColor
        {
            get => _recursiveColor;
            set
            {
                _recursiveColor = value;
                UpdateGraphicColors();
                UpdateTrueShadowCustomHash();
            }
        }

        public bool recursiveMaterial
        {
            get => _recursiveMaterial;
            set
            {
                _recursiveMaterial = value;
                if (value)
                {
                    ManipulateOtherGraphics(x => x.material = material);
                    UpdateTrueShadowCustomHash();
                }
            }
        }

        public int resolution
        {
            get => _resolution;
            set
            {
                value = Mathf.Clamp(value, 1, 20);

                _resolution = value;
                _needToResample = true;
                SetVerticesDirty();
                UpdateTrueShadowCustomHash();
            }
        }

        public int vertexCount => _vh?.currentVertCount ?? 0;

        public float width
        {
            get => _width;
            set
            {
                _width = value;
                _needToResample = true;
                SetVerticesDirty();
                UpdateTrueShadowCustomHash();
            }
        }

        public bool smooth
        {
            get => _smooth;
            set
            {
                _smooth = value;
                _needToResample = true;
                SetVerticesDirty();
                UpdateTrueShadowCustomHash();
            }
        }

        public bool roundEnds
        {
            get => _roundEnds;
            set
            {
                _roundEnds = value;
                _needToResample = true;
                SetVerticesDirty();
                UpdateTrueShadowCustomHash();
            }
        }

        public bool keepZeroZ
        {
            get => _keepZeroZ;
            set
            {
                _keepZeroZ = value;
                if (_keepZeroZ)
                {
                    var p = transform.localPosition;
                    transform.localPosition = new Vector3(p.x, p.y, 0);
                }
                _needToResample = true;
                SetVerticesDirty();
                UpdateTrueShadowCustomHash();
            }
        }

        public bool keepBillboard
        {
            get => _keepBillboard;
            set
            {
                _keepBillboard = value;
                _needToResample = true;
                SetVerticesDirty();
                UpdateTrueShadowCustomHash();
            }
        }

        public UVMode uvMode
        {
            get => _uvMode;
            set
            {
                _uvMode = value;
                SetMaterialDirty();
                UpdateTrueShadowCustomHash();
            }
        }

        public Vector2 uvMultiplier
        {
            get => _uvMultiplier;
            set
            {
                _uvMultiplier = value;
                SetMaterialDirty();
                UpdateTrueShadowCustomHash();
            }
        }

        public Vector2 uvOffset
        {
            get => _uvOffset;
            set
            {
                _uvOffset = value;
                SetVerticesDirty();
                SetMaterialDirty();
                UpdateTrueShadowCustomHash();
            }
        }

        public Vector2 clipRange
        {
            get => _clipRange;
            set
            {
                _clipRange = value;
                _needToResample = true;
                SetVerticesDirty();
                UpdateTrueShadowCustomHash();
            }
        }

        public bool fill
        {
            get => _fill;
            set
            {
                _fill = value;
                UpdateFillRenderer();
                SetVerticesDirty();
                UpdateTrueShadowCustomHash();
            }
        }

        public Color fillColor
        {
            get => _fillColor;
            set
            {
                _fillColor = value;
                if (_fillRenderer != null) _fillRenderer.SetVerticesDirty();
                SetVerticesDirty();
                UpdateTrueShadowCustomHash();
            }
        }

        public Material fillMaterial
        {
            get => _fillMaterial;
            set
            {
                if (_fillMaterial == value) return;
                _fillMaterial = value;
                if (_fillRenderer != null) _fillRenderer.material = value;
            }
        }

        public bool fillRaycast
        {
            get => _fillRaycast;
            set
            {
                _fillRaycast = value;
            }
        }

        public StartEndImagePreset startImagePreset
        {
            get => InternalUtility.GetCurrentStartImagePreset(startImageSprite);

            set
            {
                switch (value)
                {
                    case StartEndImagePreset.None:
                        startImageSprite = null;
                        break;
                    case StartEndImagePreset.Triangle:
                        startImageSprite = UISplineRendererSettings.Instance.triangleHead;
                        break;
                    case StartEndImagePreset.Arrow:
                        startImageSprite = UISplineRendererSettings.Instance.arrowHead;
                        break;
                    case StartEndImagePreset.EmptyCircle:
                        startImageSprite = UISplineRendererSettings.Instance.emptyCircleHead;
                        break;
                    case StartEndImagePreset.FilledCircle:
                        startImageSprite = UISplineRendererSettings.Instance.filledCircleHead;
                        break;
                    case StartEndImagePreset.Custom:
                        Debug.LogWarning("[UI Spline Renderer] If you want to change the start image, " +
                                         "just set value to the \"startImageSprite\" property. " +
                                         "Then It will be automatically changed to StartEndImagePreset.Custom");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(value), value, null);
                }
            }
        }

        public Sprite startImageSprite
        {
            get => _startImageSprite;
            set
            {
                _startImageSprite = value;
                UpdateStartEndImages(true);
            }
        }

        public float startImageSize
        {
            get => _startImageSize;
            set
            {
                var isNew = Math.Abs(_startImageSize - value) > 0.0001f;
                if (isNew)
                {
                    _startImageSize = value;
                    UpdateStartEndImages(true);
                }
            }
        }

        public OffsetMode startImageOffsetMode
        {
            get => _startImageOffsetMode;
            set
            {
                var isNew = _startImageOffsetMode != value;
                if (isNew)
                {
                    _startImageOffsetMode = value;
                    UpdateStartEndImages(true);
                }
            }
        }

        public float startImageOffset
        {
            get => _startImageOffset;
            set
            {
                var isNew = Math.Abs(_startImageOffset - value) > 0.0001f;
                if (isNew)
                {
                    _startImageOffset = value;
                    UpdateStartEndImages(true);
                }
            }
        }

        public float normalizedStartImageOffset
        {
            get => _normalizedStartImageOffset;
            set
            {
                var isNew = Math.Abs(_normalizedStartImageOffset - value) > 0.0001f;
                if (isNew)
                {
                    _normalizedStartImageOffset = value;
                    UpdateStartEndImages(true);
                }
            }
        }

        public StartEndImagePreset endImagePreset
        {
            get => InternalUtility.GetCurrentStartImagePreset(endImageSprite);

            set
            {
                switch (value)
                {
                    case StartEndImagePreset.None:
                        endImageSprite = null;
                        break;
                    case StartEndImagePreset.Triangle:
                        endImageSprite = UISplineRendererSettings.Instance.triangleHead;
                        break;
                    case StartEndImagePreset.Arrow:
                        endImageSprite = UISplineRendererSettings.Instance.arrowHead;
                        break;
                    case StartEndImagePreset.EmptyCircle:
                        endImageSprite = UISplineRendererSettings.Instance.emptyCircleHead;
                        break;
                    case StartEndImagePreset.FilledCircle:
                        endImageSprite = UISplineRendererSettings.Instance.filledCircleHead;
                        break;
                    case StartEndImagePreset.Custom:
                        Debug.LogWarning("[UI Spline Renderer] If you want to change the end image, " +
                                         "just set value to the \"endImageSprite\" property. " +
                                         "Then It will be automatically changed to StartEndImagePreset.Custom");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(value), value, null);
                }
            }
        }

        public Sprite endImageSprite
        {
            get => _endImageSprite;
            set
            {
                _endImageSprite = value;
                UpdateStartEndImages(false);
            }
        }

        public float endImageSize
        {
            get => _endImageSize;
            set
            {
                var isNew = Math.Abs(_endImageSize - value) > 0.0001f;
                if (isNew)
                {
                    _endImageSize = value;
                    UpdateStartEndImages(false);
                }
            }
        }

        public OffsetMode endImageOffsetMode
        {
            get => _endImageOffsetMode;
            set
            {
                var isNew = _endImageOffsetMode != value;
                if (isNew)
                {
                    _endImageOffsetMode = value;
                    UpdateStartEndImages(false);
                }
            }
        }

        public float endImageOffset
        {
            get => _endImageOffset;
            set
            {
                var isNew = Math.Abs(_endImageOffset - value) > 0.0001f;
                if (isNew)
                {
                    _endImageOffset = value;
                    UpdateStartEndImages(false);
                }
            }
        }

        public float normalizedEndImageOffset
        {
            get => _normalizedEndImageOffset;
            set
            {
                var isNew = Math.Abs(_normalizedEndImageOffset - value) > 0.0001f;
                if (isNew)
                {
                    _normalizedEndImageOffset = value;
                    UpdateStartEndImages(false);
                }
            }
        }

        #endregion

        #region Unity Lifecycle

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            
            if (_splineContainer == null) _splineContainer = GetComponent<SplineContainer>();

            if (_keepZeroZ)
            {
                var p = transform.localPosition;
                transform.localPosition = new Vector3(p.x, p.y, 0);
            }

            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (this == null) return;
                UpdateFillRenderer();
            };

            if (_fillRenderer != null) _fillRenderer.SetVerticesDirty();

            SetVerticesDirty();
            SetMaterialDirty();
            UpdateTrueShadowCustomHash();
            UpdateGraphicColors();
        }
#endif

        protected override void OnEnable()
        {
            base.OnEnable();
            Spline.Changed += OnSplineChanged;
            SplineContainer.SplineAdded += OnSplineAddedOrRemoved;
            SplineContainer.SplineRemoved += OnSplineAddedOrRemoved;

            if (!_defaultTextureInitialized && m_Texture == null)
            {
                m_Texture = UISplineRendererSettings.Instance.defaultLineTexture;
            }

            UpdateFillRenderer();

            SetVerticesDirty();
            SetMaterialDirty();
        }

        protected override void Start()
        {
#if UNITY_EDITOR
            var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            if (prefabStage != null && prefabStage.IsPartOfPrefabContents(gameObject))
            {
                UpdateRaycastTargetRect();
            }      
#endif
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            Spline.Changed -= OnSplineChanged;
            SplineContainer.SplineAdded -= OnSplineAddedOrRemoved;
            SplineContainer.SplineRemoved -= OnSplineAddedOrRemoved;
            
            if (_fillRenderer != null) _fillRenderer.gameObject.SetActive(false);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            if (_fillRenderer != null)
            {
                if (_fillRenderer.transform.parent != transform)
                {
                    if (Application.isPlaying)
                        Destroy(_fillRenderer.gameObject);
                    else
                        DestroyImmediate(_fillRenderer.gameObject);
                }
            }
            
            startImages.Clear();
            endImages.Clear();
        }

        protected override void OnRectTransformDimensionsChange()
        {
            base.OnRectTransformDimensionsChange();
            if (_fill)
            {
                SyncFillRendererTransform();
            }
        }

        protected override void OnTransformParentChanged()
        {
            base.OnTransformParentChanged();
            if (_fill)
            {
                SyncFillRendererTransform();
            }
        }

        protected void LateUpdate()
        {
            if (_fill)
            {
                var currentSiblingIndex = transform.GetSiblingIndex();
                if (transform.hasChanged || currentSiblingIndex != _lastSiblingIndex)
                {
                    SyncFillRendererTransform();
                    transform.hasChanged = false;
                    _lastSiblingIndex = transform.GetSiblingIndex();
                }
            }
        }

        #endregion

        #region MaskableGraphic Overrides

        public override void SetMaterialDirty()
        {
            if(splineContainer == null) return;
            base.SetMaterialDirty();
            ManipulateOtherGraphics(x => x.maskable = maskable);
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            Profiler.BeginSample("UISplineRenderer.OnPopulateMesh", this);
            _vh ??= vh;
            vh.Clear();
            DoExtrudeSplineJobAll(vh, false, true);
            Profiler.EndSample();
        }

        protected override void OnDidApplyAnimationProperties()
        {
            base.OnDidApplyAnimationProperties();
            UpdateStartEndImages(true);
            UpdateStartEndImages(false);
        }

        public override bool Raycast(Vector2 sp, Camera eventCamera)
        {
            if (!base.Raycast(sp, eventCamera)) return false;
            if(SplineRaycast(sp, eventCamera)) return true;
            if (_fill && _fillRaycast && _fillRenderer != null)
            {
                if (_fillRenderer.Raycast(sp, eventCamera)) return true;
            }

            return false;
        }

        public override void CrossFadeAlpha(float alpha, float duration, bool ignoreTimeScale)
        {
            base.CrossFadeAlpha(alpha, duration, ignoreTimeScale);

            ManipulateOtherGraphics(x => x.CrossFadeAlpha(alpha, duration, ignoreTimeScale));
        }

        public override void CrossFadeColor(Color targetColor, float duration, bool ignoreTimeScale, bool useAlpha)
        {
            base.CrossFadeColor(targetColor, duration, ignoreTimeScale, useAlpha);
            ManipulateOtherGraphics(x => x.CrossFadeColor(targetColor, duration, ignoreTimeScale, useAlpha));
        }

        public override void CrossFadeColor(Color targetColor, float duration, bool ignoreTimeScale, bool useAlpha, bool useRGB)
        {
            base.CrossFadeColor(targetColor, duration, ignoreTimeScale, useAlpha, useRGB);
            ManipulateOtherGraphics(x => x.CrossFadeColor(targetColor, duration, ignoreTimeScale, useAlpha, useRGB));
        }

        #endregion

        #region Public Methods

        public void PopulateFillMesh(VertexHelper vh)
        {
            if (!_fill)
            {
                vh.Clear();
                return;
            }
            Profiler.BeginSample("UISplineRenderer.PopulateFillMesh", this);
            vh.Clear();
            DoExtrudeSplineJobAll(vh, true, false);
            Profiler.EndSample();
        }

        public void UpdateRaycastTargetRect()
        {
            if (splineContainer == null) return;
            if (splineContainer.Splines.Count == 0) return;
            if (splineContainer.Splines.Count == 1 && splineContainer.Spline.Count < 2) return;

            if (_fitPosition && splineContainer.transform != transform)
            {
                transform.position = splineContainer.transform.position;
            }

            var localBounds = new Bounds(Vector3.zero, Vector3.zero);
            bool boundsInitialized = false;

            for (int i = 0; i < splineContainer.Splines.Count; i++)
            {
                var spline = splineContainer[i];

                // Use a reasonable sample count. vertexCount might be 0 initially.
                var sampleCount = Mathf.Max(10, spline.Count * resolution);
                for (int j = 0; j < sampleCount; j++)
                {
                    var t = (float)j / (sampleCount - 1);
                    var worldPos = splineContainer.EvaluatePosition(spline, t);
                    var localPos = transform.InverseTransformPoint(worldPos);
                    localPos.z = 0;

                    var w = GetWidthAt(t);
                    var b = new Bounds(localPos, Vector3.one * w);

                    if (!boundsInitialized)
                    {
                        localBounds = b;
                        boundsInitialized = true;
                    }
                    else
                    {
                        localBounds.Encapsulate(b);
                    }
                }
            }

            if (!boundsInitialized) return;

            var size = localBounds.size;
            var center = localBounds.center;

            if (float.IsNaN(size.x) || float.IsNaN(size.y) || float.IsInfinity(size.x) || float.IsInfinity(size.y)) return;

            rectTransform.sizeDelta = new Vector2(size.x, size.y);

            float pivotX = size.x > 1e-5f ? 0.5f - (center.x / size.x) : 0.5f;
            float pivotY = size.y > 1e-5f ? 0.5f - (center.y / size.y) : 0.5f;

            rectTransform.pivot = new Vector2(pivotX, pivotY);

            // 이 쓸모없어보이는 라인을 지우지 말 것.
            // 유니티의 RectTransform은 프로퍼티를 읽을때 초기화되는 병신같은 기믹이 있어서
            // 이 라인을 지우면 오브젝트를 복제할때 이 게임오브젝트의 위치가 변경됨
            if (rectTransform.parent is RectTransform parent)
            {
                _ = parent.rect;
            }

            UpdateStartEndImages(true);
            UpdateStartEndImages(false);
        }

        public void UpdateTrueShadowCustomHash()
        {
#if LETAI_TRUESHADOW
            if (_trueShadow == null) _trueShadow = GetComponent<TrueShadow>();
            if (_trueShadow == null) return;

            var knotCountToHash = 0;
            var verticesToHash = 0;
            for (int i = 0; i < splineContainer.Splines.Count; i++)
            {
                var spline = splineContainer.Splines[i];
                knotCountToHash += spline.Count;
                for (int j = 0; j < spline.Count; j++)
                {
                    var knot = spline[j];
                    verticesToHash += knot.Position.GetHashCode();
                    verticesToHash += knot.Rotation.GetHashCode();
                    verticesToHash += knot.TangentIn.GetHashCode();
                    verticesToHash += knot.TangentOut.GetHashCode();
                }
            }

            _trueShadow.CustomHash = LeTai.HashUtils.CombineHashCodes(
                texture == null ? 0 : texture.GetHashCode(),
                material == null ? 0 : material.GetHashCode(),
                color.GetHashCode(),
                _colorGradient.GetHashCode(),
                uvOffset.GetHashCode(),
                uvMultiplier.GetHashCode(),
                knotCountToHash,
                verticesToHash
            );
#endif
        }

        public Color GetColorAt(float t)
        {
            t = Mathf.Clamp01(t);
            return color * _colorGradient.Evaluate(t);
        }

        public float GetWidthAt(float t)
        {
            return _widthCurve.Evaluate(t) * width;
        }

        /// <summary>
        /// Change Width Animation Curve.
        /// </summary>
        /// <param name="curve"></param>
        public void SetWidthCurve(AnimationCurve curve)
        {
            _widthCurve = curve;
            SetVerticesDirty();
        }

        /// <summary>
        /// Change a single keyframe of widthCurve.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="key"></param>
        public void ChangeWidthCurveKey(int index, Keyframe key)
        {
            _widthCurve.MoveKey(index, key);
            SetVerticesDirty();
        }

        /// <summary>
        /// Change Color Gradient.
        /// </summary>
        /// <param name="gradient"></param>
        public void SetColorGradient(Gradient gradient)
        {
            _colorGradient = gradient;
            SetVerticesDirty();
            UpdateGraphicColors();
        }

        /// <summary>
        /// Change a single alpha key of colorGradient;
        /// </summary>
        public void ChangeColorGradientAlphaKey(int index, GradientAlphaKey key)
        {
            _colorGradient.alphaKeys[index] = key;
            SetVerticesDirty();
            UpdateGraphicColors();
        }
        /// <summary>
        /// Change a single color key of colorGradient;
        /// </summary>
        public void ChangeColorGradientAlphaKey(int index, GradientColorKey key)
        {
            _colorGradient.colorKeys[index] = key;
            SetVerticesDirty();
            UpdateGraphicColors();
        }

        public void ForceUpdate()
        {
            DoExtrudeSplineJobAll();
            Rebuild(CanvasUpdate.Layout);
        }

        /// <summary>
        /// Rotate all knots to screen direction.
        /// </summary>
        [ContextMenu("ReorientKnots")]
        public void ReorientKnots()
        {
            splineContainer.ReorientKnots();
        }

        #endregion

        #region Internal/Private Methods

        bool SplineRaycast(Vector2 sp, Camera eventCamera)
        {
            Vector3 point = default;
            if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                var flatten = new Vector3(sp.x, sp.y, transform.position.z);
                point = transform.InverseTransformPoint(flatten);    
            }
            else 
            {
                var wp = eventCamera.ScreenToWorldPoint(new Vector3(sp.x, sp.y, transform.position.z - eventCamera.transform.position.z));
                point = transform.InverseTransformPoint(wp);
                point.z = 0;    
            }
            
            foreach (var spline in splineContainer.Splines)
            {
                var distance = SplineUtility.GetNearestPoint(spline, point, out _, out var t);
                if (distance <= GetWidthAt(t))
                {
                    return true;
                }
            }

            return false;
        }

        void OnSplineChanged(Spline spline, int knotIndex, SplineModification modification)
        {
            if(spline == null) return;
            var isMySpline = false;
            if(splineContainer == null) return;
            for (int i = 0; i < splineContainer.Splines.Count; i++)
            {
                if (splineContainer.Splines[i] == spline)
                {
                    isMySpline = true;

                    if (modification == SplineModification.KnotModified)
                    {
                        var knot = spline[knotIndex];
                        var pos = knot.Position;
                        if (keepZeroZ) knot.Position = new float3(pos.x, pos.y, 0);

                        spline.SetKnotNoNotify(knotIndex, knot);
                    }

                    break;
                }
            }

            if (!isMySpline) return;
            if (keepZeroZ) transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, 0);


            SetVerticesDirty();
            SetMaterialDirty();

            UpdateRaycastTargetRect();

            UpdateTrueShadowCustomHash();

        }

        void OnSplineAddedOrRemoved(SplineContainer container, int i)
        {
            if (container != splineContainer) return;
            SetVerticesDirty();
            SetMaterialDirty();

            UpdateRaycastTargetRect();

            UpdateTrueShadowCustomHash();

        }

        void DoExtrudeSplineJobAll(VertexHelper vh = null, bool generateFill = false, bool generateStroke = true)
        {
            if (vh == null) vh = _vh;
            if (splineContainer == null)
            {
                // 그리기 과정에서 콜백으로 인해 루프에 빠지는 것을 방지하기 위해 꼭 _splineContainer에 값을 넣어야함.
                _splineContainer = GetComponent<SplineContainer>();
            }
            if (splineContainer == null) return;
            var splineCount = splineContainer.Splines.Count;
            if (splineCount <= 0) return;
            if (width == 0) return;

            var vertices = new NativeList<UIVertex>(Allocator.TempJob);
            var triangles = new NativeList<int3>(Allocator.TempJob);
            _disposables.Add(vertices);
            _disposables.Add(triangles);

            var gradient = _colorGradient.ToNative();
            _disposables.Add(gradient);

            var widthCurve = new NativeCurve(_widthCurve, Allocator.TempJob);
            _disposables.Add(widthCurve);


            var handle = DoExtrudeSplinesJob(widthCurve, gradient, vertices, triangles, generateFill, generateStroke);


            handle.Complete();
            for (int i = 0; i < vertices.Length; i++)
            {
                vh.AddVert(vertices[i]);
            }

            for (int i = 0; i < triangles.Length; i++)
            {
                var tri = triangles[i];
                vh.AddTriangle(tri.x, tri.y, tri.z);
            }


            foreach (var disposable in _disposables)
            {
                disposable.Dispose();
            }
            _disposables.Clear();
            _needToResample = false;
        }
        

        JobHandle DoExtrudeSplinesJob(
            NativeCurve widthCurve, NativeColorGradient gradient,
            NativeList<UIVertex> vertices, NativeList<int3> triangles,
            bool generateFill, bool generateStroke)
        {
            JobHandle handle = default;
            for (int i = 0; i < splineContainer.Splines.Count; i++)
            {
                var nSpline = new NativeSpline(splineContainer[i], Allocator.TempJob);
                _disposables.Add(nSpline);
                var job = new SplineExtrudeJob
                {
                    spline = nSpline,
                    widthCurve = widthCurve,
                    resolution = resolution,
                    smooth = smooth,
                    keepBillboard = keepBillboard,
                    keepZeroZ = keepZeroZ,
                    roundEnds = roundEnds,
                    fill = generateFill,
                    fillColor = fillColor,
                    drawStroke = generateStroke,
                    clipRange = clipRange,
                    uvMultiplier = uvMultiplier,
                    uvOffset = uvOffset,
                    color = color,
                    colorGradient = gradient,
                    uvMode = uvMode,
                    width = width,
                    vertices = vertices,
                    triangles = triangles
                };
                handle = job.Schedule(handle);
            }

            return handle;
        }


        void UpdateGraphicColors()
        {
            if (splineContainer == null) return;
            if (splineContainer.Splines.Count == 0) return;
            if (splineContainer.Splines.Count == 1 && splineContainer.Spline.Count < 2) return;
            
            if (!_recursiveColor) return;

            var startImageOffsetT = _startImageOffset / splineContainer.CalculateLength();
            for (int i = 0; i < startImages.Count; i++)
            {
                startImages[i].color = GetColorAt(startImageOffsetT);
            }

            var endImageOffsetT = 1f - (_endImageOffset / splineContainer.CalculateLength());
            for (int i = 0; i < endImages.Count; i++)
            {
                endImages[i].color = GetColorAt(endImageOffsetT);
            }
        }

        void ManipulateOtherGraphics(Action<MaskableGraphic> graphic)
        {
            RemoveInvalidOtherGraphics();
            for (int i = 0; i < startImages.Count; i++)
            {
                graphic.Invoke(startImages[i]);
            }

            for (int i = 0; i < endImages.Count; i++)
            {
                graphic.Invoke(endImages[i]);
            }
        }

        void RemoveInvalidOtherGraphics()
        {
            startImages.RemoveAll(x => x == null);
            endImages.RemoveAll(x => x == null);
        }
        internal void UpdateStartEndImages(bool isStartImage)
        {
            var sprite = isStartImage ? _startImageSprite : _endImageSprite;
            var images = isStartImage ? startImages : endImages;
            var offset = isStartImage ? _startImageOffset : _endImageOffset;
            var size = isStartImage ? _startImageSize : _endImageSize;
            var offsetMode = isStartImage ? _startImageOffsetMode : _endImageOffsetMode;
            var nOffset = isStartImage ? _normalizedStartImageOffset : _normalizedEndImageOffset;

            RemoveInvalidOtherGraphics();
            
            if (sprite == null)
            {
                ClearImages(images);
            }
            else
            {
                var validSplineCount = splineContainer.Splines.Count(x => x.Count > 1);
                
                // Adjust image count
                if (images.Count > validSplineCount)
                {
                    for (int i = images.Count - 1; i >= validSplineCount; i--)
                    {
                        DestroyImage(images[i]);
                        images.RemoveAt(i);
                    }
                }
                else if (images.Count < validSplineCount)
                {
                    var diff = validSplineCount - images.Count;
                    for (int i = 0; i < diff; i++)
                    {
                        var GO = new GameObject($"Spline UI Renderer - {(isStartImage ? "StartImage" : "EndImage")}[{images.Count + i}]");
                        GO.transform.SetParent(transform);
                        GO.layer = LayerMask.NameToLayer("UI");
                        var img = GO.AddComponent<Image>();
                        img.SetNativeSize();
                        images.Add(img);
                    }
                }

                for (int i = 0; i < splineContainer.Splines.Count; i++)
                {
                    var spline = splineContainer[i];
                    if (spline.Count < 2) continue;

                    if (i > images.Count - 1) continue;
                    var img = images[i];
                    img.rectTransform.sizeDelta = Vector2.one * size;
                    img.sprite = sprite;

                    float3 pos;
                    quaternion rot;
                    float t;

                    var length = spline.GetLength();

                    if (offsetMode == OffsetMode.Distance)
                    {
                        t = isStartImage ? offset / length : 1 + offset / length;
                    }
                    else
                    {
                        t = nOffset;
                    }


                    var outward = t is < 0 or > 1;

                    if (outward)
                    {
                        var tt = isStartImage ? 0f : 1f;
                        if (isStartImage && t > 1) tt = 1;
                        else if (!isStartImage && t < 0) tt = 0;
                        if (offsetMode == OffsetMode.Normalized) tt = t;

                        pos = splineContainer.EvaluatePosition(tt);
                        var tan = splineContainer.EvaluateTangent(tt);

                        // resolve tangent
                        if ((Vector3)tan == Vector3.zero)
                        {
                            var p = splineContainer.EvaluatePosition(spline, isStartImage ? 0.01f : 0.99f);
                            tan = isStartImage ? p - pos : pos - p;
                        }


                        if (keepBillboard) rot = quaternion.LookRotation(new float3(0, 0, 1), tan);
                        else
                        {
                            var up = splineContainer.EvaluateUpVector(tt);
                            rot = quaternion.LookRotation(up, tan);
                        }


                        var outwardOffset = offset;
                        if(offsetMode == OffsetMode.Distance)
                        {
                            if (isStartImage && t > 1)
                            {
                                outwardOffset -= length;
                            }
                            else if (!isStartImage && t < 0)
                            {
                                outwardOffset += length;
                            }
                        }
                        else
                        {
                            outwardOffset = length * (t > 1 ? t - 1 : t);
                        }

                        pos = (Vector3)pos + (Quaternion)rot * (Vector3.up * outwardOffset);

                        if (keepZeroZ) pos.z = transform.position.z;

                        if (recursiveColor) img.color = GetColorAt(isStartImage ? 0 : 1);
                        if (recursiveMaterial) img.material = material;
                    }
                    else
                    {
                        splineContainer.Evaluate(spline, t, out pos, out var tan, out var up);

                        // resolve tangent
                        if ((Vector3)tan == Vector3.zero)
                        {
                            var acc = spline.EvaluateAcceleration(t);
                            tan = isStartImage ? acc - pos : pos - acc;
                        }

                        if (keepZeroZ) pos.z = transform.position.z;

                        if (keepBillboard) rot = quaternion.LookRotation(new float3(0, 0, 1), tan);
                        else rot = quaternion.LookRotation(up, tan);

                        if (recursiveColor) img.color = GetColorAt(t);
                        if (recursiveMaterial) img.material = material;
                    }

                    images[i].transform.SetPositionAndRotation(
                        pos,
                        rot
                    );
                }
            }
        }

        private void ClearImages(List<Image> images)
        {
            for (int i = images.Count - 1; i >= 0; i--)
            {
                DestroyImage(images[i]);
            }
            images.Clear();
        }

        private void DestroyImage(Image image)
        {
            if (image != null)
            {
                if (Application.isPlaying)
                    Destroy(image.gameObject);
                else
                    DestroyImmediate(image.gameObject);
            }
        }

        void UpdateFillRenderer()
        {
            if (_fill)
            {
                if (_fillRenderer != null && _fillRenderer.owner != this)
                {
                    _fillRenderer = null;
                }

                if (_fillRenderer == null)
                {
                    var siblingName = $"{name} Fill";
                    var parent = transform.parent;
                    if (parent != null)
                    {
                        var t = parent.Find(siblingName);
                        if (t != null)
                        {
                            var foundRenderer = t.GetComponent<UISplineFillRenderer>();
                            if (foundRenderer != null && (foundRenderer.owner == null || foundRenderer.owner == this))
                            {
                                _fillRenderer = foundRenderer;
                            }
                        }
                    }

                    if (_fillRenderer == null)
                    {
                        var go = new GameObject(siblingName);
                        go.hideFlags = HideFlags.HideInHierarchy;
                        if (parent != null) go.transform.SetParent(parent, false);
                        else go.transform.SetParent(transform, false);

                        _fillRenderer = go.AddComponent<UISplineFillRenderer>();
                    }
                }

                _fillRenderer.owner = this;
                _fillRenderer.material = _fillMaterial;
                _fillRenderer.gameObject.SetActive(true);

                SyncFillRendererTransform();
            }
            else
            {
                if (_fillRenderer != null)
                {
                    _fillRenderer.gameObject.SetActive(false);
                }
            }
        }

        void SyncFillRendererTransform()
        {
            if (_fillRenderer == null) return;

            var t = _fillRenderer.rectTransform;
            var myT = rectTransform;

            if (t.parent != myT.parent) t.SetParent(myT.parent, false);

            var myIndex = myT.GetSiblingIndex();
            var tIndex = t.GetSiblingIndex();

            if (tIndex > myIndex)
            {
                t.SetSiblingIndex(myIndex);
            }
            else if (tIndex < myIndex - 1)
            {
                t.SetSiblingIndex(Mathf.Max(0, myIndex - 1));
            }

            t.anchorMin = myT.anchorMin;
            t.anchorMax = myT.anchorMax;
            t.pivot = myT.pivot;
            t.sizeDelta = myT.sizeDelta;
            t.localPosition = myT.localPosition;
            t.localRotation = myT.localRotation;
            t.localScale = myT.localScale;
        }

        #endregion

        #region Static API

        /// <summary>
        /// Create SplineContainer and UISplineRenderer at once using presets.
        /// All tangent mode of the knots are TangentMode.AutoSmooth.
        /// </summary>
        /// <param name="positions">World positions of knots. all rotations of the knots are -forward</param>
        /// <param name="parent">A UI gameObject to belong to</param>
        /// <param name="isLocal">True if the positions are local positions. False if the positions are world positions</param>
        /// <param name="lineTexture">A preset of the line texture. If you want to use custom texture, change texture property after this object is created</param>
        /// <param name="startImage">A preset of the start image (first point). If you want to use custom sprite, change startImageSprite property after this object is created</param>
        /// <param name="endImage">A preset of the end image (last point). If you want to use custom sprite, change endImageSprite property after this object is created</param>
        /// <returns></returns>
        public static UISplineRenderer Create(
            IEnumerable<Vector3> positions,
            RectTransform parent,
            bool isLocal = false,
            LineTexturePreset lineTexture = LineTexturePreset.Default,
            StartEndImagePreset startImage = StartEndImagePreset.None,
            StartEndImagePreset endImage = StartEndImagePreset.None)
        {
            var t = new GameObject("New UI Spline Renderer").AddComponent<RectTransform>();
            t.SetParent(parent, false);
            var container = t.gameObject.AddComponent<SplineContainer>();

            foreach (var p in positions)
            {
                var localP = isLocal ? p : container.transform.InverseTransformPoint(p);
                var knot = new BezierKnot(localP);
                container.Spline.Add(knot);
            }

            container.ReorientKnotsAndSmooth();

            var renderer = container.gameObject.AddComponent<UISplineRenderer>();
            renderer.lineTexturePreset = lineTexture;
            renderer.startImagePreset = startImage;
            renderer.endImagePreset = endImage;

            return renderer;
        }

        public static UISplineRenderer Create(
            IEnumerable<Vector3> positions,
            RectTransform parent,
            bool isLocal,
            Texture lineTexture,
            Sprite startImage,
            Sprite endImage)
        {
            var t = new GameObject("New UI Spline Renderer").AddComponent<RectTransform>();
            t.SetParent(parent, false);
            var container = t.gameObject.AddComponent<SplineContainer>();

            foreach (var p in positions)
            {
                var localP = isLocal ? p : container.transform.InverseTransformPoint(p);
                var knot = new BezierKnot(localP);
                container.Spline.Add(knot);
            }

            container.ReorientKnotsAndSmooth();

            var renderer = container.gameObject.AddComponent<UISplineRenderer>();
            renderer.texture = lineTexture;
            renderer.startImageSprite = startImage;
            renderer.endImageSprite = endImage;

            return renderer;
        }

        #endregion
    }
}
