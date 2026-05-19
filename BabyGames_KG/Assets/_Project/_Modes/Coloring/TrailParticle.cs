using System;
using CustomAttributes;
using Loggers;
using Modes.Coloring;
using Services;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace _Project._Modes.Coloring
{
    public class TrailParticle : MonoBehaviour
    {
        private enum TrailMode { General, Coloring }

        [SerializeField, Fg_Se] private TrailMode trailMode;
        [Fg_Se] public float dropParticleScale = 1;

        [SerializeField, Fg_Co] private Camera cam;
        [SerializeField, Re_Fg_Ref] private ParticleSystem particlePrefab;
        [SerializeField, Fg_De] private ParticleSystem currentParticle;

        [Inject] private Content content;

        private ParticleSystem GetParticle()
        {
            if (currentParticle == false)
            {
                currentParticle = Instantiate(particlePrefab);
                currentParticle.transform.localScale = Vector3.one * dropParticleScale;
            }

            return currentParticle;
        }

        private async void Awake()
        {
            try
            {
                DiService.Inject(this);
                cam = cam ?? Camera.main;

                if (particlePrefab == null)
                {
                    switch (trailMode)
                    {
                        case TrailMode.Coloring:
                        {
                            particlePrefab = await content.dragParticle_Coloring.GetAssetAsync();
                            break;
                        }
                        case TrailMode.General:
                        {
                            particlePrefab = await content.dragParticle_General.GetAssetAsync();
                            break;
                        }
                    }

                }
            } catch (Exception e)
            {
                CustomLogger.instance.LogException(e);
            }
        }

        private void OnEnable()
        {
            Destroy();
        }

        private void OnDisable()
        {
            Destroy();
        }

        private void Update()
        {
            var pointer = Pointer.current;
            if (pointer == null) { return; }
            if (particlePrefab == false) { return; }

            if (pointer.press.wasPressedThisFrame) { GetParticle().transform.localPosition = Vector3.zero; }
            if (pointer.press.wasReleasedThisFrame) { Destroy(); }

            if (pointer.press.isPressed)
            {
                Vector3 sp = new Vector3(pointer.position.ReadValue().x, pointer.position.ReadValue().y, cam.nearClipPlane + 1);
                Vector3 wp = cam.ScreenToWorldPoint(sp);

                wp.z = transform.position.z;
                GetParticle().transform.position = wp;
            }
        }

        private void Destroy()
        {
            if (currentParticle == false) { return; }
            var toDes = currentParticle;
            Destroy(toDes.gameObject, 1);
        }
    }
}