using _Project.Scripts.Sound;
using CustomAttributes;
using DG.Tweening;
using Helpers;
using Identifiers;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Modes.Puzzle
{
    public class PuzzlePiece : MonoBehaviour
    {
        private static RenderTexture renderTexture;

        [SerializeField, Re_Fg_Co] private SpriteRenderer spriteRenderer;
        [SerializeField, Re_Fg_Co] private List<SpriteRenderer> additionalSpriteRenderer;
        [SerializeField, Re_Fg_Co] private SpriteMask spriteMask;
        [field: SerializeField, Re_Fg_Co] public BoxCollider2D boxCollider { get; private set; }

        [SerializeField, Fg_Se] private int textureWidth = 512;
        [SerializeField, Fg_Se] private int textureHeight = 512;
        [SerializeField, Fg_Se] private float scale = 2.25f;
        [SerializeField, Fg_Se] private Sprite _sprite;

        [field: SerializeField, Fg_De] public Vector3 originalPosition { get; private set; }
        [field: SerializeField, Fg_De] private Vector3 offsetPosition;
        [SerializeField, Fg_De] private PuzzleActivity_Identifier puzzleActivity_Identifier;
        [SerializeField, Fg_De] private Texture2D texture;

        public bool isCompleted;

        [Button]
        public void Validate()
        {
            originalPosition = transform.localPosition;
            spriteRenderer.sprite = _sprite;

            Bounds spriteBounds = spriteRenderer.bounds;

            Vector3 localCenter = transform.InverseTransformPoint(spriteBounds.center);
            Vector3 localSize = transform.InverseTransformVector(spriteBounds.size);

            boxCollider.offset = localCenter;
            boxCollider.size = localSize;
        }

        [Button]
        public void SetSprite(Sprite sprite)
        {
            _sprite = sprite;
            spriteRenderer.sprite = _sprite;
        }

        public void Preinitialize(PuzzleActivity_Identifier puzzleActivity)
        {
            puzzleActivity_Identifier = puzzleActivity;

            spriteRenderer.sprite = _sprite;
            additionalSpriteRenderer.ForEach(x => x.sprite = _sprite);

            SetVisibility(true);
            additionalSpriteRenderer.ForEach(x => x.gameObject.SetActive(true));
        }

        public void OnBegginDrag()
        {
            transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack);
            Sound_FX.Play_Static(Sound_Effect.StartDrag);
        }

        public void OnPlaced()
        {
            isCompleted = true;
            boxCollider.enabled = false;

            spriteRenderer.sortingOrder = 2;

            transform.DOScale(1, 0.25f).SetEase(Ease.OutBack);
            offsetPosition = new Vector3(originalPosition.x, originalPosition.y, 0);
            transform.DOLocalMove(offsetPosition, 0.25f).SetEase(Ease.OutBack);

            Move_FX.MakeDoneParticle(originalPosition, 3);
        }

        public Tweener SetRandomPosition(Vector3 position)
        {
            offsetPosition = new Vector3(position.x, position.y, 0);
            return GoToOffset();
        }

        public Tweener GoToOffset(float duration = 1f)
        {
            spriteRenderer.sortingOrder = 3;

            transform.DOScale(0.75f, duration).SetEase(Ease.OutBack);
            return transform.DOMove(offsetPosition, duration).SetEase(Ease.OutBack);
        }

        [Button]
        public void SetVisibility(bool visible, float duration = 0)
        {
            if (duration == 0)
            {
                spriteRenderer.color = new Color(1, 1, 1, 0);
            }
            else
            {
                spriteRenderer.DOFade(visible.ToInt(), duration);
            }
        }
    }
}