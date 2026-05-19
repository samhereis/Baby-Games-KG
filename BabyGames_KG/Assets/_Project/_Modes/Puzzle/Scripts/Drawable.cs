using CustomAttributes;
using DataClasses;
using Helpers;
using Services;
using Sirenix.OdinInspector;
using Sounds;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using _Project.Scripts.Data;
using Loggers;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Modes.Puzzle
{
    public class Drawable : MonoBehaviour
    {
        public bool isDrawing => _previousDragPosition != Vector2.zero;

        [field: SerializeField] public float percentageOfColoring { get; private set; }
        [field: SerializeField] public SpriteRenderer _spriteRenderer { get; private set; }
        [field: SerializeField] public SpriteMask _spriteMask { get; private set; }
        [field: SerializeField] public bool ignoreFinger { get; set; } = false;

        [Header("Settings")]
        [SerializeField] private LayerMask _drawingLayers;

        [SerializeField] private Color _penColour;
        [SerializeField] private bool _setUnpaintedColor = false;
        [SerializeField] private int _penWidth = 3;
        [SerializeField] private float _drawBetweenSpace = 2;
        [SerializeField] private float _minimumPixelTransparency = 0.5f;

        [Space]
        [SerializeField] private Sound _drawAudion;
        [SerializeField] private AudioSource _soundPlayer;

        [Fg_De] public volatile bool isActive;
        [Fg_De] public Sprite originalDrawableSprite;
        [Fg_De] public Texture2D originalDrawableTexture;
        [Fg_De] public Texture2D currentDrawableTexture;

        private Vector2 _previousDragPosition;

        private Rect _drawableSpriteRect;
        private Bounds _drawableSpriteBounds;

        private ConcurrentQueue<Action> _threadActions = new ConcurrentQueue<Action>();

        public Color32[] originalPixelsColorArray { get; set; }
        public Color32[] currentPixelsColorArray { get; set; }

        private CancellationToken dct;
        private Action _backgroundTask_Draw;
        private bool _lowMemoryFreed;

        [Button]
        public void Initialize()
        {
            Initialize(_spriteRenderer.sprite);
        }

        public void Initialize(Sprite sprite)
        {
            DiService.Inject(this);

            if (_spriteRenderer == null) { _spriteRenderer = GetComponent<SpriteRenderer>(); }
            if (_spriteRenderer == null) { _spriteRenderer = GetComponentInChildren<SpriteRenderer>(true); }
            if (_spriteMask == null) { _spriteMask = GetComponentInChildren<SpriteMask>(true); }

            _spriteRenderer.sprite = sprite;
            if (_spriteMask != null) { _spriteMask.sprite = _spriteRenderer.sprite; }

            Setup();
        }

        [Button]
        public async void Setup()
        {
            originalDrawableSprite = _spriteRenderer.sprite;
            originalDrawableTexture = originalDrawableSprite.texture;
            currentDrawableTexture = originalDrawableTexture.GetReadableCopy();
            ClearablesHolder.instance.clearableTextures.SafeAdd(currentDrawableTexture);

            currentPixelsColorArray = currentDrawableTexture.GetPixels32();
            originalPixelsColorArray = currentDrawableTexture.GetPixels32();

            _drawableSpriteRect = originalDrawableSprite.rect;
            _drawableSpriteBounds = originalDrawableSprite.bounds;

            _spriteRenderer.sprite = Sprite.Create(currentDrawableTexture, _drawableSpriteRect, new Vector2(0.5f, 0.5f));
            ClearablesHolder.instance.clearableSprites.SafeAdd(_spriteRenderer.sprite);

            if (_spriteMask != null) { _spriteMask.sprite = _spriteRenderer.sprite; }

            Material materialInstance = new Material(_spriteRenderer.material);
            materialInstance.mainTexture = currentDrawableTexture;
            _spriteRenderer.material = materialInstance;

            for (int i = 0; i < currentPixelsColorArray.Length; i++) { originalPixelsColorArray[i] = currentPixelsColorArray[i]; }

            isActive = currentDrawableTexture != null;

            _backgroundTask_Draw = () =>
            {
                if (isActive == false) { return; }
                if (currentPixelsColorArray == null) { return; }

                if (_threadActions.TryDequeue(out Action threadAction)) { threadAction?.Invoke(); }
            };

            if (_soundPlayer == null) { _soundPlayer = transform.root.GetComponentInChildren<AudioSource>(true); }
            if (_soundPlayer != null)
            {
                _soundPlayer.clip = await _drawAudion.GetSound();
                _soundPlayer.loop = true;
                _soundPlayer.Stop();
            }
        }

        private void Awake()
        {
            dct = destroyCancellationToken;
        }

        private void OnEnable()
        {
            isActive = false;
            LazyUpdator_Service.instance?.AddToQueue(UpdateColorDifferencePercentage);
            Application.lowMemory += OnLowMemory;

            SetThreadActions();
        }

        private void OnDisable()
        {
            isActive = false;
            LazyUpdator_Service.instance?.RemoveFromQueue(UpdateColorDifferencePercentage);
            Application.lowMemory -= OnLowMemory;
        }

        private void Update()
        {

#if UNITY_EDITOR
            if (Keyboard.current != null && Keyboard.current.lKey.wasReleasedThisFrame) { OnLowMemory(); }
#endif

            if (currentDrawableTexture == null)
            {
                isActive = false;
                return;
            }

            if (ignoreFinger) { return; }

            var pointer = Pointer.current;
            if (pointer == null) { return; }

            bool mouseDown = pointer.press.isPressed;

            if (mouseDown == true)
            {
                Vector2 mouseWorldPositioni = Camera.main.ScreenToWorldPoint(Pointer.current.position.ReadValue());
                DrawAtMousePosition(mouseWorldPositioni);
            }
            else if (mouseDown == false)
            {
                _previousDragPosition = Vector2.zero;
                _soundPlayer?.Stop();
            }
        }

        private void FixedUpdate()
        {
            if (isActive == true) { ApplyMarkedPixelChanges(); }
        }

        private void SetThreadActions()
        {
            Task.Run(async () =>
            {
                while (dct.IsCancellationRequested == false)
                {
                    if (isActive) { _backgroundTask_Draw?.Invoke(); }
                    else { await Task.Delay(100, dct); }
                }
            }, dct);
        }

        public void ApplyMarkedPixelChanges()
        {
            if (currentDrawableTexture == null) { return; }
            if (currentPixelsColorArray == null) { return; }
            if (currentPixelsColorArray.Length < 10) { return; }

            currentDrawableTexture.SetPixels32(currentPixelsColorArray);
            currentDrawableTexture.Apply();
        }

        public void DrawAtMousePosition(Vector2 mouseWorldPosition)
        {
            Collider2D hit = Physics2D.OverlapPoint(mouseWorldPosition, _drawingLayers.value);

            if (hit != null && hit.transform != null)
            {
                Vector3 localPosition = _spriteRenderer.transform.InverseTransformPoint(mouseWorldPosition);
                _threadActions.Enqueue(() => { Draw(localPosition); });
                if (_soundPlayer?.isPlaying == false) { _soundPlayer?.Play(); }
            }
        }

        public void Draw(Vector3 localPosition)
        {
            Task.Run(() =>
            {
                if (dct.IsCancellationRequested == false)
                {
                    Vector2 pixelPos = WorldToPixelCoordinates(localPosition);

                    if (_previousDragPosition == Vector2.zero) { MarkPixelsToColour(pixelPos, _penWidth, _penColour); }
                    else { ColourBetween(_previousDragPosition, pixelPos, _penWidth, _penColour); }

                    _previousDragPosition = pixelPos;
                }
            }, dct);
        }

        public void ColourBetween(Vector2 start_point, Vector2 end_point, int width, Color color)
        {
            float distance = Vector2.Distance(start_point, end_point);
            Vector2 direction = (end_point - start_point).normalized;
            float stepSize = _drawBetweenSpace / distance;

            for (float lerp = 0; lerp <= 1; lerp += stepSize)
            {
                Vector2 cur_position = Vector2.Lerp(start_point, end_point, lerp);
                MarkPixelsToColour(cur_position, width, color);
            }
        }

        public void MarkPixelsToColour(Vector2 center_pixel, int pen_radius, Color color_of_pen)
        {
            int center_x = (int)center_pixel.x;
            int center_y = (int)center_pixel.y;

            for (int x = center_x - pen_radius; x <= center_x + pen_radius; x++)
            {
                if (x >= (int)_drawableSpriteRect.width || x < 0) { continue; }

                for (int y = center_y - pen_radius; y <= center_y + pen_radius; y++)
                {
                    if (y >= (int)_drawableSpriteRect.height || y < 0) { continue; }
                    if (Vector2.Distance(new Vector2(x, y), center_pixel) <= pen_radius) { MarkPixelToChange(x, y, color_of_pen); }
                }
            }
        }

        public void MarkPixelToChange(int x, int y, Color color)
        {
            int array_pos = y * (int)_drawableSpriteRect.width + x;
            MarkPixelToChange(currentPixelsColorArray, array_pos, color);
        }

        public void MarkPixelToChange(Color32[] pixelArray, int arrayPosition, Color color)
        {
            if (arrayPosition > pixelArray.Length || arrayPosition < 0) { return; }
            if (pixelArray[arrayPosition].a < _minimumPixelTransparency) { return; }

            pixelArray[arrayPosition] = color;
        }

        public Vector2 WorldToPixelCoordinates(Vector3 localPosition)
        {
            float pixelWidth = _drawableSpriteRect.width;
            float pixelHeight = _drawableSpriteRect.height;
            float unitsToPixels = pixelWidth / _drawableSpriteBounds.size.x;

            float centered_x = localPosition.x * unitsToPixels + pixelWidth / 2;
            float centered_y = localPosition.y * unitsToPixels + pixelHeight / 2;

            Vector2 pixel_pos = new Vector2(Mathf.RoundToInt(centered_x), Mathf.RoundToInt(centered_y));

            return pixel_pos;
        }

        private async Task UpdateColorDifferencePercentage()
        {
            if (isActive == false) { return; }
            if (dct.IsCancellationRequested == true) { return; }

            if (currentPixelsColorArray == null || originalPixelsColorArray == null)
            {
                Debug.LogError("Pixel arrays not initialized.");
                return;
            }

            float nonTransparentElements = currentPixelsColorArray.Where(x => x.a > _minimumPixelTransparency).Count();
            percentageOfColoring = 100 - (nonTransparentElements / (float)originalPixelsColorArray.Length * 100f);

            await AsyncHelper.DelayFloat(0.1f);
        }

        [Button]
        public async Task Complete(int? interDelay = null)
        {
            try
            {
                if (currentPixelsColorArray == null) { return; }

                if (interDelay == null)
                {
                    await Task.Run(() =>
                    {
                        for (int x = 0; x <= currentPixelsColorArray.Length - 1; x++) { MarkPixelToChange(currentPixelsColorArray, x, _penColour); }
                    });
                }
                else
                {
                    await Task.Run(async () =>
                    {
                        int index = 0;
                        for (int x = 0; x <= currentPixelsColorArray.Length - 1; x++)
                        {
                            try
                            {
                                MarkPixelToChange(currentPixelsColorArray, x, _penColour);
                                if (index > _drawableSpriteRect.width)
                                {
                                    await Task.Delay(interDelay.Value);
                                    index = 0;
                                }

                                index++;
                            } catch (Exception ex) { CustomLogger.instance.LogException(ex); }
                        }
                    });
                }
            } catch (Exception ex) { CustomLogger.instance.LogException(ex); }
        }

        [Button]
        private void OnLowMemory()
        {
            if (_lowMemoryFreed) { return; }

            try
            {
                Destroy(currentDrawableTexture);
                Destroy(originalDrawableSprite);

                currentPixelsColorArray = null;
                originalPixelsColorArray = null;
                originalDrawableTexture = null;
                originalDrawableSprite = null;
                _threadActions = new ConcurrentQueue<Action>();
                isActive = false;
                percentageOfColoring = 100f;

                _lowMemoryFreed = true;

                Application.lowMemory -= OnLowMemory;
            } catch (Exception ex) { CustomLogger.instance.LogException(ex); }
        }
    }
}