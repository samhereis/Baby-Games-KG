using System;
using System.Reflection;
using System.Threading.Tasks;
using CustomAttributes;
using Helpers;
using Identifiers;
using Loggers;
using Modes.Puzzle;
using PaintCore;
using PaintIn2D;
using PaintIn3D;
using Services;
using Sirenix.OdinInspector;
using Sounds;
using UnityEngine;

namespace _Project._Modes.Puzzle.Scripts
{
    public class DrawableP2D : IdentifierBase, ISelfValidator
    {
        public enum DrawMode { Paint, Erase, Reveal }

        [Fg_Se] public DrawMode drawMode = DrawMode.Paint;
        [Fg_Se, SerializeField] private Color _penColour = Color.white;
        [Fg_Se, SerializeField] private float _brushRadius = 0.05f;
        [Fg_Se, SerializeField] private Sound _drawAudion;
        [Fg_Se, SerializeField] private AudioSource _soundPlayer;

        [Fg_Co, SerializeField] private CwPaintableSprite _paintableSprite;
        [Fg_Co, SerializeField] private CwPaintableSpriteTexture _paintableSpriteTexture;
        [Fg_Co, SerializeField] private CwChangeCounter _changeCounter;
        [Fg_Co, SerializeField] private CwHitScreen2D _hitScreen2D;
        [Fg_Co, SerializeField] private CwPaintSphere _paintSphere;

        [Fg_De, SerializeField] private Drawable _sibling;
        [field: Fg_De, SerializeField] public bool isDrawing { get; private set; }
        [field: Fg_De, SerializeField] public float percentageOfColoring { get; private set; }
        [field: Fg_De, SerializeField] public bool ignoreFinger { get; set; } = false;
        [Fg_De] public bool isActive = false;
        [Fg_De] public Sprite currentSprite;

        public void Validate(SelfValidationResult result)
        {
            _sibling = GetComponent<Drawable>();
        }

        private void Awake()
        {
            DiService.Inject(this);
            Validate(null);
        }

        private void Update()
        {
            if (_hitScreen2D == null || isActive == false || _paintableSprite == false)
            {
                if (_soundPlayer != null && _soundPlayer.isPlaying) { _soundPlayer?.Stop(); }
                return;
            }

            if (ignoreFinger)
            {
                _paintableSprite.enabled = false;
            }
            else
            {
                _paintableSprite.enabled = true;

                var pointer = UnityEngine.InputSystem.Pointer.current;
                isDrawing = pointer != null && pointer.press.isPressed;

                if (isDrawing)
                {
                    if (_soundPlayer && _soundPlayer?.isPlaying == false) { _soundPlayer?.Play(); }
                }
                else
                {
                    if (_soundPlayer) { _soundPlayer?.Stop(); }
                }
            }
        }

        private void LateUpdate()
        {
            if (_changeCounter != null) { percentageOfColoring = _changeCounter.Ratio * 100f; }
        }

        private void OnDisable()
        {
            isDrawing = false;
            if (_soundPlayer) { _soundPlayer?.Stop(); }
        }

        [Button]
        public void Initialize()
        {
            Initialize(Get<SpriteRenderer>().sprite);
        }

        public async void Initialize(Sprite sprite)
        {
            try
            {
                if (sprite == null) { return; }
                currentSprite = sprite;
                Get<SpriteRenderer>().sprite = sprite;
                var spriteMask = Get<SpriteMask>();
                if (spriteMask != null) { spriteMask.sprite = sprite; }

                { // Disable sprite mask, because old solution has it
                    if (drawMode == DrawMode.Reveal)
                    {
                        Get<SpriteRenderer>().enabled = true;
                        if (Get<SpriteMask>() != null) { Get<SpriteMask>().enabled = false; }

                        foreach (var sr in GetComponentsInChildren<SpriteRenderer>(true))
                        {
                            if (sr.gameObject != gameObject) { sr.gameObject.SetActive(false); }
                        }
                    }
                }

                { // Reinitialize all drawing components
                    foreach (var item in TryGetAll_List<CwPaintableSpriteTexture>()) { Destroy(item); }
                    foreach (var item in TryGetAll_List<CwPaintableSprite>()) { Destroy(item); }
                    foreach (var item in TryGetAll_List<CwChangeCounter>()) { Destroy(item); }
                    await AsyncHelper.NextFrame();

                    if (_paintableSpriteTexture == null) { _paintableSpriteTexture = gameObject.AddComponent<CwPaintableSpriteTexture>(); }
                    if (_paintableSprite == null) { _paintableSprite = gameObject.AddComponent<CwPaintableSprite>(); }
                    if (_changeCounter == null) { _changeCounter = gameObject.AddComponent<CwChangeCounter>(); }

                    while (_paintableSpriteTexture == null || _paintableSprite == null || _changeCounter == null)
                    {
                        _paintableSprite = Get<CwPaintableSprite>();
                        _paintableSpriteTexture = Get<CwPaintableSpriteTexture>();
                        _changeCounter = Get<CwChangeCounter>();
                        await AsyncHelper.NextFrame();
                    }

                    _paintableSprite.Activate();
                    _paintableSpriteTexture.Activate();
                }

                if (drawMode == DrawMode.Reveal)
                {
                    _paintableSpriteTexture.Clear(_paintableSpriteTexture.Texture, new Color(1f, 1f, 1f, 1f / 255f));
                }

                _paintableSprite.AutoSize = true;
                _paintableSprite.AutoTexture = true;
                _paintableSprite.AutoMask = true;

                _changeCounter.PaintableTexture = _paintableSpriteTexture;
                _changeCounter.Threshold = 0.05f;

                if (_sibling != null)
                {
                    _penColour = ReadField<Color>(_sibling, "_penColour");
                    _drawAudion = ReadField<Sound>(_sibling, "_drawAudion");
                    _soundPlayer = ReadField<AudioSource>(_sibling, "_soundPlayer");
                    _brushRadius = ReadField<int>(_sibling, "_penWidth") / currentSprite.pixelsPerUnit;
                    if (_brushRadius <= 0f) { _brushRadius = 0.05f; }
                }

                if (_soundPlayer == null) { _soundPlayer = Get<AudioSource>(); }
                if (_soundPlayer != null && _drawAudion != null)
                {
                    _soundPlayer.clip = await _drawAudion.GetSound();
                    _soundPlayer.loop = true;
                    _soundPlayer.Stop();
                }

                isActive = true;
                SpawnPainter();
            } catch (Exception ex) { CustomLogger.instance?.LogException(ex); }
        }

        public void SetBrushColor(Color color)
        {
            _penColour = color;
            if (_paintSphere != null) { _paintSphere.Color = color; }
        }

        private void SpawnPainter()
        {
            try
            {
                var existing = MonobehaviorHelper.FindAll<CwHitScreen2D>();
                if (existing.Count > 0)
                {
                    _hitScreen2D = existing[0];
                    _hitScreen2D.GuiLayers = 0;
                    _paintSphere = _hitScreen2D.GetComponent<CwPaintSphere>();
                    ApplyBrushMode();
                    return;
                }

                var prefab = Resources.Load<GameObject>("_Prefabs/Paint Decal");
                if (prefab == null) { return; }

                var go = Instantiate(prefab);
                _hitScreen2D = go.GetComponent<CwHitScreen2D>();
                _paintSphere = go.GetComponent<CwPaintSphere>();

                go.layer = gameObject.layer;
                go.transform.localPosition = Vector3.zero;
                go.transform.localRotation = Quaternion.identity;
                go.transform.localScale = Vector3.one;

                if (_hitScreen2D != null)
                {
                    _hitScreen2D.Layers = -1;
                    _hitScreen2D.GuiLayers = 0; // never suppress touches that start over UI
                    _hitScreen2D.Frequency = CwHitScreen2D.FrequencyType.OnceEveryFrame;
                    _hitScreen2D.Intercept = CwHitScreenBase2D.InterceptType.InterceptZ;

                    _hitScreen2D.Connector.ConnectHits = true;
                    _hitScreen2D.Connector.ClipConnected = true;
                    _hitScreen2D.Connector.HitSpacing = _brushRadius * 0.5f;
                }

                ApplyBrushMode();
            } catch (Exception ex) { CustomLogger.instance?.LogException(ex); }
        }

        private void ApplyBrushMode()
        {
            if (_paintSphere == null) { return; }

            _paintSphere.Radius = _brushRadius;

            switch (drawMode)
            {
                case DrawMode.Erase:
                {
                    _paintSphere.Color = Color.clear;
                    _paintSphere.BlendMode = CwBlendMode.Replace(Vector4.one);
                    break;
                }
                case DrawMode.Reveal:
                {
                    // ReplaceCustom samples the original sprite at each texel's own UV, restoring true colour + full alpha
                    _paintSphere.Color = Color.white;
                    _paintSphere.BlendMode = CwBlendMode.ReplaceCustom(Color.white, currentSprite != null ? currentSprite.texture : null, Vector4.one);
                    break;
                }
                case DrawMode.Paint:
                {
                    _paintSphere.Color = _penColour;
                    _paintSphere.BlendMode = CwBlendMode.Replace(Vector4.one);
                    break;
                }
                default:
                    _paintSphere.Color = _penColour;
                    _paintSphere.BlendMode = CwBlendMode.Replace(Vector4.one);
                    break;
            }
        }

        public CwPaintableTexture PaintableSpriteTexture => _paintableSpriteTexture;
        public void DrawAtMousePosition(Vector2 worldPosition, CwPaintableTexture targetTexture = null)
        {
            if (_paintSphere == null) { return; }
            var prev = _paintSphere.TargetTexture;
            if (targetTexture != null) { _paintSphere.TargetTexture = targetTexture; }
            _paintSphere.HandleHitPoint(false, 0, 1f, 0, worldPosition, Quaternion.identity);
            _paintSphere.TargetTexture = prev;
        }

        public void FillTransparentWithSprite(Sprite sprite)
        {
            if (sprite == null) { return; }
            if (_paintableSpriteTexture?.Activated != true || _paintableSpriteTexture.Current == null) { return; }

            var rt = _paintableSpriteTexture.Current;
            var temp = RenderTexture.GetTemporary(rt.descriptor);

            Graphics.Blit(sprite.texture, temp);

            var blendMat = new Material(Shader.Find("Sprites/Default"));
            Graphics.Blit(rt, temp, blendMat);
            Destroy(blendMat);

            Graphics.Blit(temp, rt);
            RenderTexture.ReleaseTemporary(temp);

            _paintableSpriteTexture.NotifyOnModified(false);
        }

        [Button]
        public async Task Complete()
        {
            try
            {
                if (_paintSphere == null || _paintableSpriteTexture == null || _paintableSpriteTexture.Activated == false) { return; }

                ApplyBrushMode();

                var bounds = Get<SpriteRenderer>().bounds;
                var savedRadius = _paintSphere.Radius;
                _paintSphere.Radius = bounds.extents.magnitude * 1.5f;

                DrawAtMousePosition(bounds.center, _paintableSpriteTexture);

                _paintSphere.Radius = savedRadius;
            } catch (Exception ex) { CustomLogger.instance?.LogException(ex); }

            await Task.CompletedTask;
        }

        private static T ReadField<T>(object obj, string fieldName)
        {
            var f = obj.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            return f != null ? (T)f.GetValue(obj) : default;
        }
    }
}