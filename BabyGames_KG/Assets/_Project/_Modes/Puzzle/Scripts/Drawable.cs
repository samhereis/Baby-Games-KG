using CustomAttributes;
using DataClasses;
using Helpers;
using Services;
using Sirenix.OdinInspector;
using Sounds;
using System;
using System.Collections.Concurrent;
using System.Threading;
using _Project._Modes.Puzzle.Scripts;
using UnityEngine;

namespace Modes.Puzzle
{
    [RequireComponent(typeof(DrawableP2D))]
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

        [Fg_De] public volatile bool isActive = false;
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
    }
}