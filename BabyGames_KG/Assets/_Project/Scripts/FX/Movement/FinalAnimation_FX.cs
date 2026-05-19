using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _Project.Scripts.FX.Movement
{
    public class FinalAnimation_FX : MonoBehaviour
    {
        [System.Serializable]
        private class MotionData
        {
            public SpriteRenderer spriteRenderer;
            public float speed;
            public float frequency;
            public float amplitude;
            public float phaseOffset;
        }

        public Vector2 xPositions;
        public Vector2 yPositions;

        [Space]
        public Vector2 forwardSpeed;

        public Vector2 frequency;
        public Vector2 amplitude;

        [Space]
        public bool randomizePhase = true;

        [Space]
        public SpriteRenderer spriteRenderer;

        private readonly List<MotionData> _motions = new();

        private void Start()
        {
            var allInstances = FindObjectsByType<FinalAnimation_FX>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var item in allInstances)
            {
                if (item == this) { continue; }
                Destroy(item.gameObject);
            }

            xPositions.x += transform.position.x;
            xPositions.y += transform.position.x;

            var position = Camera.main.transform.position;
            position.z = 0;
            transform.position = position;

            var spriteRenderers = GetComponentsInChildren<SpriteRenderer>().ToList();
            spriteRenderers.Remove(spriteRenderer);

            foreach (var spriteRenderer in spriteRenderers)
            {
                var pos = spriteRenderer.transform.position;
                pos.x = Random.Range(xPositions.x, xPositions.y);
                pos.y = Random.Range(yPositions.x, yPositions.y);

                spriteRenderer.transform.position = pos;

                _motions.Add(new MotionData
                {
                    spriteRenderer = spriteRenderer,
                    speed = Random.Range(forwardSpeed.x, forwardSpeed.y),
                    frequency = Random.Range(frequency.x, frequency.y),
                    amplitude = Random.Range(amplitude.x, amplitude.y),
                    phaseOffset = randomizePhase ? Random.Range(0f, 2 * Mathf.PI) : 0f
                });
            }

            spriteRenderer.DOFade(0.75f, 1);
        }

        private void Update()
        {
            foreach (var motion in _motions)
            {
                Vector3 upward = Vector3.up * motion.speed * Time.deltaTime;
                float xOffset = Mathf.Sin(Time.time * motion.frequency + motion.phaseOffset) * motion.amplitude;
                Vector3 rightward = Vector3.right * xOffset;

                motion.spriteRenderer.transform.position += upward + rightward;
            }
        }
    }
}