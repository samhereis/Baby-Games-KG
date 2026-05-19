using DataClasses.AssetReferences;
using DataClasses.Consts;
using Helpers;
using Interfaces;
using Loggers;
using Services;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using _Project.Scripts.Services.Purchase;
using TMPro;
using UI;
using UI.Helpers;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;
using Zenject;

namespace Coloring.ForParents
{
    public class SettingsTab_Purchase : SettingsTab_Base
    {
        [SerializeField] private Button _restoreButton;
        [SerializeField] private Button _fullAccess_BuyButton;
        [SerializeField] private TextMeshProUGUI _fullAccess__PriceText;

        [Space]
        [SerializeField] private ExternalAssetReference_HasComponent<LoaderPupup> _loaderScreenPrefab;
        [SerializeField] private List<LoaderPupup> _spawnedLoaderScreens = new();

        [Inject] private IPurchasesService _purchasesService;
        [Inject] private ISubscriptionChecker _lifetimePurchase;

        protected void Awake()
        {
            DiService.Inject(this);
        }

        private void OnEnable()
        {
            _restoreButton.onClick.AddListener(OnRestoreButtonClicked);
            _fullAccess_BuyButton.onClick.AddListener(OnClickButtonBuyLifeTime);

            _purchasesService.onPurchase += OnPurchased;
            _purchasesService.onPurchaseFailed += HideLoading;
            _purchasesService.onRestore += OnRestore;
            _purchasesService.onRestoreFailed += OnRestoreFailed;

            _fullAccess__PriceText.text = _purchasesService.GetPrice(PurchaseIDs.lifetimePurchase);
        }

        private void OnDisable()
        {
            _restoreButton?.onClick.RemoveListener(OnRestoreButtonClicked);
            _fullAccess_BuyButton.onClick.RemoveListener(OnClickButtonBuyLifeTime);

            _purchasesService.onPurchase -= OnPurchased;
            _purchasesService.onPurchaseFailed -= HideLoading;
            _purchasesService.onRestore -= OnRestore;
            _purchasesService.onRestoreFailed -= OnRestoreFailed;
        }

        [Button]
        private async void ShowLoading()
        {
            try
            {
                _spawnedLoaderScreens.RemoveNulls();
                _spawnedLoaderScreens.Add(await _loaderScreenPrefab?.InstantiateAsync(transform.root));
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

        private void OnClickButtonBuyLifeTime()
        {
            if (ApplicationHelper.HasInternetConnection() == false)
            {
                MessagesMenu.instance?.ShowMessage("no_internet_connection");
            }

            try
            {
                ShowLoading();
                _purchasesService.Purchase(PurchaseIDs.lifetimePurchase);
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

        private void OnPurchased(Item item, Product product)
        {
            if (_lifetimePurchase.IsSubscribed())
            {
                _model.onReloadRequested?.Invoke();
            }

            HideLoading(null, null);
        }

        [Button]
        private void OnRestore()
        {
            if (_lifetimePurchase.IsSubscribed())
            {
                _model.onReloadRequested?.Invoke();
                HideLoading(null, null);
            }
            else
            {
                OnRestoreFailed();
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