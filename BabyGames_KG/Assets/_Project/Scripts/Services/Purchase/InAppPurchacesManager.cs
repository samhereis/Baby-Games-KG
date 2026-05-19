using DataClasses.Consts;
using Interfaces;
using Loggers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

namespace Services
{
    [Serializable]
    public class InAppPurchacesManager_CurrencyCodes
    {
        public string currencyCode;
        public string currencyCharacter;
    }

    public class InAppPurchacesManager : MonoBehaviour, IDetailedStoreListener, IPurchasesService
    {
        [SerializeField] private List<Item> _items = new() { new Item { id = PurchaseIDs.lifetimePurchase, type = ProductType.NonConsumable } };
        [SerializeField] private List<InAppPurchacesManager_CurrencyCodes> _currencyCodes = new();

        public event Action onInitialize;
        public event Action onInitializeFailed;
        public event Action<Item, Product> onPurchase;
        public event Action<Item, Product> onPurchaseFailed;
        public event Action onRestore;
        public event Action onRestoreFailed;

        public IStoreController storeController { get; private set; }
        public IExtensionProvider extensionProvider { get; private set; }

        public bool IsPurchased(string productID)
        {
            try
            {
                return storeController.products.WithID(productID).hasReceipt;
            }
            catch (Exception ex)
            {
                Debug.LogWarning("Error checking for purchase: " + ex);
                return false;
            }
        }

        public string GetPrice(string productID)
        {
            try
            {
                Product product = GetProduct(productID);
                string currencyCode = product.metadata.isoCurrencyCode;
                if (_currencyCodes.Exists(x => x.currencyCode == currencyCode))
                {
                    currencyCode = _currencyCodes.Find(x => x.currencyCode == currencyCode).currencyCharacter;
                }
                return currencyCode + product.metadata.localizedPrice;
            }
            catch (Exception ex)
            {
                Debug.LogWarning("Error checking for purchase: " + ex);
                return "$4.99";
            }
        }

        public Product GetProduct(string productID)
        {
            Product product = storeController.products.WithID(productID);
            return product;
        }

        public async Task<bool> Initialize()
        {
            await UnityServices.InitializeAsync();
            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

            foreach (var item in _items)
            {
                builder.AddProduct(item.id, item.type);
            }

            UnityPurchasing.Initialize(this, builder);

            return true;
        }

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            Debug.Log("In-App Purchasing successfully initialized");
            storeController = controller;
            extensionProvider = extensions;
            onInitialize?.Invoke();
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            OnInitializeFailed(error, null);
        }

        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            string errorMessage = $"Purchasing failed to initialize. Reason: {error}.";
            if (message != null)
            {
                errorMessage += $" More details: {message}";
            }
            Debug.Log(errorMessage);
            onInitializeFailed?.Invoke();
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {
            var product = args.purchasedProduct;
            Item item = _items.Find(x => x.id == product.definition.id);

            if (item != null)
            {
                onPurchase?.Invoke(item, product);
            }
            else
            {
                onPurchaseFailed?.Invoke(null, product);
            }
            Debug.Log($"Purchase Complete - Product: {product.definition.id}");

            return PurchaseProcessingResult.Complete;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            Debug.Log($"Purchase failed - Product: '{product.definition.id}', PurchaseFailureReason: {failureReason}");

            Item item = _items.Find(x => x.id == product.definition.id);
            onPurchaseFailed?.Invoke(item, product);
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
        {
            Debug.Log($"Purchase failed - Product: '{product.definition.id}', " +
                      $"Purchase failure reason: {failureDescription.reason}, " +
                      $"Purchase failure details: {failureDescription.message}");

            Item item = _items.Find(x => x.id == product.definition.id);
            onPurchaseFailed?.Invoke(item, product);
        }

        public void Purchase(string productID)
        {
            try
            {
                storeController?.InitiatePurchase(productID);
            }
            catch (Exception ex)
            {
                onPurchaseFailed?.Invoke(_items.Find(x => x.id == productID), storeController.products.WithID(productID));
                Debug.LogWarning("Error while purchasing: " + ex);
            }
        }

        public void Restore()
        {
            try
            {
                extensionProvider?.GetExtension<IAppleExtensions>().RestoreTransactions((result, error) =>
                {
                    if (result)
                    {
                        Debug.Log("In-App Purchasing restored purchases");
                        onRestore?.Invoke();
                    }
                    else
                    {
                        onRestoreFailed?.Invoke();
                    }
                });
            }
            catch (Exception e)
            {
                CustomLogger.instance.LogException(e);
            }
        }
    }

    [Serializable]
    public class Item
    {
        public string id;
        public ProductType type;
    }
}