using _Project.Scripts.Data;
using Agents;
using DataClasses;
using Helpers;
using Services;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Modes.Coloring
{
    public class ScreenShotItem : MonoBehaviour
    {
        [SerializeField] private Image _imageScreen;
        [SerializeField] private AnimationAgent _animationAgent;

        private void Awake()
        {
            DiService.Inject(this);
            gameObject.gameObject.SetActive(false);
        }

        public void AnimationScreenShot(Texture2D texture = null)
        {
            if (texture != null)
            {
                var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100, 1, SpriteMeshType.FullRect);
                ClearablesHolder.instance.clearableSprites.SafeAdd(sprite);

                _imageScreen.sprite = sprite;
            }

            transform.position = Vector3.zero;
            transform.localScale = Vector3.one;
            transform.rotation = Quaternion.identity;

            AnimationMove();
        }

        [Button]
        private void AnimationMove()
        {
            gameObject.SetActive(true);

            _animationAgent.enabled = true;
            _animationAgent.PlayAnimation("Screenshot");

            _animationAgent.onAnimationCallback -= OnAnimationCallback;
            _animationAgent.onAnimationCallback += OnAnimationCallback;
        }

        private void OnAnimationCallback(string obj)
        {
            if (obj == "OnEnd")
            {
                gameObject.SetActive(false);
            }
        }
    }
}