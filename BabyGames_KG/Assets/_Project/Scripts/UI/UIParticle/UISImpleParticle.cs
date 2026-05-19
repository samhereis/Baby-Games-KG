using System;
using System.Collections.Generic;
using DG.Tweening;
using Helpers;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace UI
{
    public class UISImpleParticle : MonoBehaviour
    {
        public List<Sprite> sprites = new();
        public Image bulletPrefab;
        public List<RectTransform> canvasParents = new();
        public Vector2 bulletSpeed;
        public Vector2 lifeTime;
        public Vector2 size;
        public Vector2 fadeDuration;
        public bool autoFire = true;
        public Vector2 fireInterval;

        RectTransform _rt;
        float _timer;
        List<Bullet> _bullets = new List<Bullet>();

        [Serializable]
        class Bullet
        {
            public RectTransform rt;
            public Image img;
            public Vector2 vel;
            public float age;
            public bool fading;
            public float fadeElapsed;
            public float life;
            public float fadeDur;
        }

        void Awake()
        {
            _rt = GetComponent<RectTransform>();
            if (canvasParents.Count < 1)
            {
                canvasParents.Add(GetComponent<RectTransform>());
            }
        }

        void Update()
        {
            if (autoFire)
            {
                _timer -= Time.deltaTime;
                if (_timer <= 0f)
                {
                    FireOnce();
                    _timer = fireInterval.GetRandom();
                }
            }

            for (int i = _bullets.Count - 1; i >= 0; i--)
            {
                Bullet b = _bullets[i];
                float dt = Time.deltaTime;

                if (!b.fading)
                {
                    b.rt.anchoredPosition += b.vel * dt;
                    b.age += dt;
                    if (b.age >= b.life)
                    {
                        b.fading = true;
                        b.fadeElapsed = 0f;
                    }
                }
                else
                {
                    b.rt.anchoredPosition += b.vel * dt;

                    b.fadeElapsed += dt;
                    float t = (b.fadeDur <= 0f) ? 1f : Mathf.Clamp01(b.fadeElapsed / b.fadeDur);
                    if (b.img != null)
                    {
                        Color c = b.img.color;
                        c.a = Mathf.Lerp(1f, 0f, t);
                        b.img.color = c;
                    }
                    if (t >= 1f)
                    {
                        if (b.rt != null && b.rt.gameObject != null) Destroy(b.rt.gameObject);
                        _bullets.RemoveAt(i);
                    }
                }
            }
        }

        public void FireOnce()
        {
            if (bulletPrefab == null || canvasParents.Count < 1 || _rt == null) return;

            var parent = canvasParents.GetRandom();

            Image inst = Instantiate(bulletPrefab, parent);
            RectTransform instRect = inst.GetComponent<RectTransform>();
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, _rt.position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, screenPoint, null, out Vector2 anchoredPos);
            instRect.anchoredPosition = anchoredPos;
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)).normalized;
            Bullet b = new Bullet
            {
                rt = instRect,
                img = inst,
                vel = dir * bulletSpeed,
                age = 0f,
                fading = false,
                fadeElapsed = 0f,
                life = lifeTime.GetRandom(),
                fadeDur = fadeDuration.GetRandom()
            };

            inst.sprite = sprites.GetRandom();

            var s = size.GetRandom();
            inst.rectTransform.sizeDelta = Vector2.zero;
            inst.rectTransform.DOSizeDelta(new Vector2(s, s), 0.25f);
            _bullets.Add(b);
        }
    }
}