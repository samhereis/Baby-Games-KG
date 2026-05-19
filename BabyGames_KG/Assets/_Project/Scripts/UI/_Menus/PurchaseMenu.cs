using DataClasses.AssetReferences;
using DataClasses.Consts;
using DG.Tweening;
using Helpers;
using Interfaces;
using Loggers;
using Services;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using _Project.Scripts.Services.Purchase;
using _Project.Scripts.UI.Purchase;
using CustomAttributes;
using TMPro;
using UI.Helpers;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Purchasing;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Zenject;

namespace UI.Menus
{
    public class PurchaseMenu : MenuBase
    {
        private UnityAction _actionBuy;
        private UnityAction _actionClose;

        [SerializeField] private ScreenOrientation _screenOrientationOnEnable;
        [SerializeField] private Button _buyButton;
        [SerializeField] private Button _restoreButton;
        [SerializeField] private Button _closeButton;

        [SerializeField, FoldoutGroup("FullTimeAccess")] private PurchaseMenu_PeriodUnit _switchYearly;
        [SerializeField, FoldoutGroup("FullTimeAccess")] private PurchaseMenu_PeriodUnit _switchMonthly;

        [SerializeField, FoldoutGroup("Advantages")] private List<Image> _advantageImages = new();
        [SerializeField, FoldoutGroup("Advantages")] private Sprite[] _advantageSprites;

        [Space]
        [SerializeField] private ExternalAssetReference_HasComponent<LoaderPupup> _loaderScreenPrefab;
        [SerializeField] private List<LoaderPupup> _spawnedLoaderScreens = new();
        
        [SerializeField, Fg_De] private PurchaseMenu_PeriodUnit _currentSwitch;

        [Inject] private IPurchasesService _purchasesService;
        [Inject] private ISubscriptionChecker _lifetimePurchase;
        [Inject] private ISubscriptionManager _subscriptionController;

        public override void Validate(SelfValidationResult result)
        {
            base.Validate(result);
        }

        [Button]
        public void SetAdvantages()
        {
            _advantageImages = GetComponentsInChildren<Image>().Where(x => x.name == "PurchaseMenu_Pro").ToList();

            int index = 0;
            foreach (var item in _advantageImages)
            {
                item.sprite = _advantageSprites[index];
                index++;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            
            DiService.Inject(this);
            Get<PurchaseMenu_Data>().Init(_lifetimePurchase);
        }

        private void OnEnable()
        {
            _restoreButton.onClick.AddListener(OnRestoreButtonClicked);
            _closeButton.onClick.AddListener(OnClickButtonClose);

            _buyButton.onClick.RemoveListener(Buy);
            _buyButton.onClick.AddListener(Buy);

            OnSelected(_switchYearly);

            _switchMonthly.onChoose += OnSelected;
            _switchYearly.onChoose += OnSelected;

            _purchasesService.onPurchase += OnPurchased;
            _purchasesService.onPurchaseFailed += HideLoading;
            _purchasesService.onRestore += OnRestore;
            _purchasesService.onRestoreFailed += OnRestoreFailed;

            DontDestroyOnLoad(gameObject);
        }

        private void OnDisable()
        {
            _restoreButton?.onClick.RemoveListener(OnRestoreButtonClicked);
            _closeButton?.onClick.RemoveListener(OnClickButtonClose);

            _buyButton.onClick.RemoveListener(Buy);

            _switchMonthly.onChoose -= OnSelected;
            _switchYearly.onChoose -= OnSelected;

            _purchasesService.onPurchase -= OnPurchased;
            _purchasesService.onPurchaseFailed -= HideLoading;
            _purchasesService.onRestore -= OnRestore;
            _purchasesService.onRestoreFailed -= OnRestoreFailed;

            ScreenHelper.SetAppOrientation(ScreenOrientation.LandscapeLeft);
        }

        public void Show(UnityAction actionBuy, UnityAction actionClose)
        {
            _actionBuy = actionBuy;
            _actionClose = actionClose;

            ScreenHelper.SetAppOrientation(_screenOrientationOnEnable);

            Enable();

        }

        [Button]
        public void Hide()
        {
            HideLoading(null, null);
            Disable();

            _actionBuy = null;
            _actionClose = null;

            Destroy(gameObject, 10);
        }

        [Button]
        private async void ShowLoading()
        {
            try
            {
                _spawnedLoaderScreens.RemoveNulls();
                _spawnedLoaderScreens.Add(await _loaderScreenPrefab?.InstantiateAsync(transform));
                _spawnedLoaderScreens.Last()?.Open();
            }
            catch (Exception ex)
            {
                CustomLogger.instance?.LogException(ex);
            }
        }

        [Button]
        private void HideLoading(Item item, Product product)
        {
            try
            {
                _spawnedLoaderScreens.RemoveNulls();
                _spawnedLoaderScreens.ForEach(x => x?.Close());
            }
            catch (Exception ex)
            {
                CustomLogger.instance?.LogException(ex);
            }
        }

        private void OnSelected(PurchaseMenu_PeriodUnit switchButton)
        {
            _currentSwitch = switchButton;
            
            _switchYearly.EnableState((_switchYearly == switchButton).ToInt());
            _switchMonthly.EnableState((_switchMonthly == switchButton).ToInt());
        }

        private void Buy()
        {
            if (_currentSwitch == _switchMonthly) { OnClickBuyMonthly(); }
            else { OnClickBuyYearly(); }
        }

        private void OnClickBuyMonthly()
        {
            if (ApplicationHelper.HasInternetConnection() == false)
            {
                MessagesMenu.instance?.ShowMessage("no_internet_connection");
            }

            try
            {
                ShowLoading();
                _subscriptionController.Subscribe(PurchaseIDs.subscriptionMonthly);
            }
            catch (Exception ex)
            {
                HideLoading(null, null);
                CustomLogger.instance?.LogException(ex, $"Could not start purchase");
            }
        }

        private void OnClickBuyYearly()
        {
            if (ApplicationHelper.HasInternetConnection() == false)
            {
                MessagesMenu.instance?.ShowMessage("no_internet_connection");
            }

            try
            {
                ShowLoading();
                _subscriptionController.Subscribe(PurchaseIDs.subscriptionYearly);
            }
            catch (Exception ex)
            {
                HideLoading(null, null);
                CustomLogger.instance?.LogException(ex, $"Could not start purchase");
            }
        }

        private void OnRestoreButtonClicked()
        {
            try
            {
                ShowLoading();
                _purchasesService.Restore();
            }
            catch (Exception ex)
            {
                HideLoading(null, null);
                CustomLogger.instance?.LogException(ex, $"Could not start restore");
            }
        }

        private void OnClickButtonClose()
        {
            _actionClose?.Invoke();
            Hide();
        }

        private void OnPurchased(Item item, Product product)
        {
            _actionBuy?.Invoke();

            Hide();
        }

        [Button]
        private void OnRestore()
        {
            if (_lifetimePurchase.IsSubscribed() == false)
            {
                OnRestoreFailed();
            }
            else
            {
                HideLoading(null, null);
            }
        }

        [Button]
        private void OnRestoreFailed()
        {
            if (ApplicationHelper.HasInternetConnection() == false)
            {
                MessagesMenu.instance?.ShowMessage("no_internet_connection");
            }
            else
            {
                MessagesMenu.instance?.ShowMessage("no_purchase");
            }

            HideLoading(null, null);
        }
    }
}