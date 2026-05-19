using _Project.Scripts.Sound;
using Helpers;
using Loggers;
using Modes.Coloring;
using Services;
using Sirenix.OdinInspector;
using System;
using UnityEngine;
using Zenject;

namespace FX
{
    public class StateEnd_FX : MonoBehaviour
    {
        [Inject] private Content _content;
        [SerializeField] private bool _canPlay = true;

        public async void DoFX(Vector3? particlePosition = null)
        {
            if (_canPlay == false) { return; }

            if (particlePosition == null)
            {
                var position = Camera.main.transform.position;
                position.z = 0;

                particlePosition = position;
            }

            try
            {
                _canPlay = false;

                if (_content != null) { _content = DiService.Get<Content>(); }

                Sound_FX.Play_Static(Sound_Effect.StateComplete);
                var particle = Instantiate(_content.completeParticle);
                particle.transform.position = particlePosition.Value;
                particle.Play();

                Destroy(particle.gameObject, 5);
            }
            catch (Exception ex)
            {
                CustomLogger.instance?.LogException(ex);
            }
            finally
            {
                await AsyncHelper.DelayFloat(1f);
                _canPlay = true;
            }
        }

        [Button]
        public void FX()
        {
            DoFX(null);
        }
    }
}