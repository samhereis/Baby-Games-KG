using _Project._Modes.Orchestra.Scripts.SO;
using _Project.Scripts.Sound;
using DG.Tweening;
using Identifiers;
using Loggers;
using Services;
using System;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Coocking
{
    public class CoockingFeeding_Mixing : CoockingFeeding_StateBase
    {
        [SerializeField] private CookingFeeding_Mixer _mixer;
        [Inject] private Orchestra_Data _orchestra_Data;

        [Space]
        public Transform miska;
        public Transform miskaTarget;

        [Space]
        public Panel_World panelItem;
        public Transform zerno;
        public Transform meat;
        public Transform carrot;

        public override async Task Enter()
        {
            DiService.Inject(this);

           await base.Enter();
            panelItem.transform.DOMoveX(25, 1);

            try
            {
                Sound_FX.Play_Static(Sound_Effect.Success);
                var confetti = await _orchestra_Data.transition.InstantiateAsync();
                confetti.transform.position = Vector3.zero;
                confetti.Play();
            }
            catch (Exception ex)
            {
                CustomLogger.instance?.LogException(ex);
            }

            zerno.gameObject.SetActive(false);
            meat.gameObject.SetActive(false);
            carrot.gameObject.SetActive(false);
            _mixer.Show();

            miska.transform.DOMove(miskaTarget.transform.position, 1);
            miska.transform.DORotate(miskaTarget.transform.eulerAngles, 1);
            miska.transform.DOScale(miskaTarget.transform.localScale, 1);

            _mixer.isReady.AddListener(OnMixerDone);
        }

        private async void OnMixerDone(bool obj)
        {
            zerno.gameObject.SetActive(false);
            meat.gameObject.SetActive(false);
            carrot.gameObject.SetActive(false);
            
            miska.transform.DOMoveX(25, 1);

            _nextState = _nextStateOnWin;
        }
    }
}