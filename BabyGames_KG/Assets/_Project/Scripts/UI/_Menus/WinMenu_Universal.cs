using _Project.Scripts.FX.Movement;
using _Project.Scripts.Sound;
using CustomAttributes;
using FX;
using Loggers;
using Sirenix.OdinInspector;
using System;
using _Project.Scripts.Services.Purchase;
using DG.Tweening;
using Interfaces;
using Services;
using UI.Menus;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Modes.Sorting
{
    public class WinMenu_Universal : MenuBase
    {
        [SerializeField] [Re_Fg_Co] public Button mainMenu;
        [SerializeField] [Re_Fg_Co] public Button replay;
        [SerializeField] [Re_Fg_Co] public Button next;
        [SerializeField] [Re_Fg_Co] public Image nextGameAutoPlayProgressImage;

        [SerializeField] private FinalAnimation_FX _finalAnimation;
        [SerializeField] private FinalAnimation_FX _currentFinalAnimation;

        [Inject] private ISubscriptionChecker _subscriptionChecker;

        protected override void Awake()
        {
            base.Awake();
            DiService.Inject(this);
        }

        protected override void OnDestroy()
        {
            if (_currentFinalAnimation != null)
            {
                Destroy(_currentFinalAnimation.gameObject);
            }
        }

        [Button]
        public override void Enable(float? duration = null)
        {
            if (_dct.IsCancellationRequested) { return; }

            base.Enable(duration);

            if (_finalAnimation != null)
            {
                var position = Camera.main.transform.position;
                position.z = 0;

                _currentFinalAnimation = Instantiate(_finalAnimation, position, Quaternion.identity);
                _currentFinalAnimation.transform.position = position;
            }

            Get<Sound_FX>()?.Play(Sound_Effect.Complete);

            if (_subscriptionChecker.IsSubscribed())
            {
                nextGameAutoPlayProgressImage.DOFillAmount(1, 10).OnComplete(() => { next.onClick.Invoke(); });
            }

            try
            {
                foreach (var item in FindObjectsByType<HintHand_Click>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                {
                    item.SetIsActive(false);
                    item.enabled = false;
                }
            } catch (Exception ex) { CustomLogger.instance?.LogException(ex); }

            try
            {
                foreach (var item in FindObjectsByType<HintHand_Drag>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                {
                    item.SetIsActive(false);
                    item.enabled = false;
                }
            } catch (Exception ex) { CustomLogger.instance?.LogException(ex); }

            try
            {
                foreach (var item in FindObjectsByType<HintHand_DrawSimple>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                {
                    item.SetIsActive(false);
                    item.enabled = false;
                }
            } catch (Exception ex) { CustomLogger.instance?.LogException(ex); }
        }
    }
}