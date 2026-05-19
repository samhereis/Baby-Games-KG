using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UI;
using UI.Menus;
using UnityEngine;
using UnityEngine.UI;

namespace Coloring.CheckForParents
{
    public class ParentalGateMenu : MenuBase, ISelfValidator
    {
        public enum Orientation { Horizontal, Vertical };
        private Orientation _orientation => Screen.width > Screen.height ? Orientation.Horizontal : Orientation.Vertical;

        [SerializeField] private RectTransform popupTransform;
        [SerializeField] private MathController mathController;

        [SerializeField] private List<ParentalGate_Button> numberButtons;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button clearAnswerButton;

        public bool isPopupShown = false;

        private Action _calbackOnSuccess;
        private Action _calbackOnFailure;

        public override void Validate(SelfValidationResult result)
        {
            base.Validate(result);

            numberButtons = transform.GetComponentsInChildren<ParentalGate_Button>().ToList();
            for (int i = 0; i < numberButtons.Count; i++)
            {
                numberButtons[i].SetNumber(i);
            }
        }

        protected override void Awake()
        {
            base.Awake();

            GetComponent<CanvasGroup>().blocksRaycasts = false;

            EnableTouch(false);
            SetupListenerToButtons();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            foreach (var btn in numberButtons)
            {
                btn.button.onClick.RemoveAllListeners();
            }

            closeButton.onClick.RemoveAllListeners();
            clearAnswerButton.onClick.RemoveAllListeners();

            GetComponent<CanvasGroup>().DOKill();
        }

        private void SetupListenerToButtons()
        {
            foreach (var btn in numberButtons)
            {
                btn.button.onClick.AddListener(() =>
                {
                    OnNumberPressed(btn.number);
                });
            }

            closeButton.onClick.AddListener(() =>
            {
                ClosePopup();
            });

            clearAnswerButton.onClick.AddListener(() =>
            {
                mathController.ClearAnswer();
            });
        }

        private void OnNumberPressed(int number)
        {
            mathController.Input(number);
        }

        public void ClosePopup(Action actionOnClose = null, bool withAnimation = true)
        {
            EnableTouch(false);
            if (withAnimation)
            {
                float animTime = 0.5f;
                popupTransform.DOKill();
                popupTransform.DOLocalMove(new Vector3(Screen.width, -Screen.height), animTime).SetEase(Ease.InBack);
                popupTransform.DOLocalRotate(new Vector3(0, 0, 90), animTime).SetEase(Ease.InSine).OnComplete(() =>
                {
                    if (actionOnClose == null)
                    {
                        _calbackOnFailure?.Invoke();
                    }
                    else
                    {
                        actionOnClose?.Invoke();
                    }

                    isPopupShown = false;

                    Disable();
                });
            }
            else
            {
                actionOnClose?.Invoke();
                Disable(0);
                popupTransform.SetPositionAndRotation(new Vector3(Screen.width, -Screen.height), Quaternion.Euler(new Vector3(0, 0, 90)));
            }
        }

        public void ShowPopup(Action callbackOnSuccess, Action callbackOnFailure)
        {
            if (Application.isEditor && DevelopmentConfigs.debugMode) { callbackOnSuccess?.Invoke(); return; }

            try
            {
                foreach (var duplicated in FindObjectsByType<ParentalGateMenu>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                {
                    if (duplicated != this) { Destroy(duplicated.gameObject); }
                }
            }
            catch { }

            isPopupShown = true;
            mathController.GenerateMathTask(OnMathCompleted);

            _calbackOnSuccess = callbackOnSuccess;
            _calbackOnFailure = callbackOnFailure;

            Enable();

            float animTime = 0.7f;
            popupTransform.DOKill();
            popupTransform.DOAnchorPos(Vector3.zero, animTime).SetEase(Ease.OutBack);
            popupTransform.DORotateQuaternion(Quaternion.Euler(0, 0, 0), animTime).SetEase(Ease.OutSine).OnComplete(() =>
            {
                EnableTouch(true);
            });

            Destroy(gameObject, 30);
        }

        private void OnMathCompleted(bool isCorrect)
        {
            EnableTouch(false);
            if (isCorrect)
            {
                StartCoroutine(mathController.CorrectAnswerAnimation(() => ClosePopup(_calbackOnSuccess)));
            }
            else
            {
                WrongSolutionAnimation();
            }
        }

        private void WrongSolutionAnimation()
        {
            popupTransform.DOKill();
            popupTransform.DOBlendableLocalMoveBy(new Vector3(-10, 0), 0.2f).SetEase(Ease.OutSine).OnComplete(() =>
            {
                mathController.ClearTask();
                popupTransform.DOBlendableLocalMoveBy(new Vector3(20, 0), 0.2f).SetEase(Ease.InOutSine).OnComplete(() =>
                {
                    popupTransform.DOBlendableLocalMoveBy(new Vector3(-10, 0), 0.2f).SetEase(Ease.InOutSine).OnComplete(() =>
                    {
                        mathController.GenerateMathTask(OnMathCompleted);
                        StartCoroutine(mathController.ShowTaskAgain(() => EnableTouch(true)));
                    });
                });
            });
        }

        private void EnableTouch(bool enable)
        {
            popupTransform.GetComponent<CanvasGroup>().blocksRaycasts = enable;
        }
    }
}