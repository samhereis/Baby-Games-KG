using DataClasses.Consts;
using Services;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using _Project.Scripts.Services.Purchase;
using UnityEngine;
using UnityEngine.Purchasing;

public class SubscriptionController : MonoBehaviour, ISubscriptionChecker, ISubscriptionManager
{
    [SerializeField] private InAppPurchacesManager _inAppPurchacesManager;
    public bool _isYearly = false;

    public bool IsSubscribed()
    {
        if (Application.isEditor)
        {
            if (DevelopmentConfigs.alwaysPurchased == true) { return true; }
        }

        if (Application.platform == RuntimePlatform.Android)
        {
            return true;
        }

        var isMonthly = IsSubscribed(PurchaseIDs.subscriptionMonthly);
        var isYearly = IsSubscribed(PurchaseIDs.subscriptionYearly);

        var result = isMonthly || isYearly;

        if (result && isYearly)
        {
            _isYearly = true;
        }

        return result;
    }

    public string GetYearlyPurchasePrice()
    {
        return _inAppPurchacesManager.GetPrice(PurchaseIDs.subscriptionYearly);
    }

    public string GetMonthlyPurchasePrice()
    {
        return _inAppPurchacesManager.GetPrice(PurchaseIDs.subscriptionMonthly);
    }

    public bool IsSubscribed(string subscriptionProductId)
    {
        try
        {
            var product = _inAppPurchacesManager.storeController.products.WithID(subscriptionProductId);
            if (product == null || !product.hasReceipt)
            {
                return false;
            }

            var subscriptionInfo = GetSubscriptionInfo(product);
            return subscriptionInfo.isSubscribed() == Result.True;
        } catch (Exception ex)
        {
            Debug.LogWarning("Error checking subscription status: " + ex);
            return false;
        }
    }

    public void Subscribe(string subscriptionID)
    {
        _inAppPurchacesManager.Purchase(subscriptionID);
    }

    public bool GetIsFreeTrial(string subscriptionProductId)
    {
        try
        {
            var product = _inAppPurchacesManager.storeController.products.WithID(subscriptionProductId);
            if (product == null || !product.hasReceipt)
            {
                return false;
            }

            var subscriptionInfo = GetSubscriptionInfo(product);
            if (subscriptionInfo.isSubscribed() == Result.True)
            {
                return subscriptionInfo.isFreeTrial() == Result.True;
            }

            return false;
        } catch (Exception ex)
        {
            Debug.LogWarning("Error getting GetIsFreeTrial: " + ex);
            return false;
        }
    }

    public string GetIsFreeTrialPeriod(string subscriptionProductId)
    {
        try
        {
            var product = _inAppPurchacesManager.storeController.products.WithID(subscriptionProductId);
            if (product == null || !product.hasReceipt)
            {
                return "";
            }

            var subscriptionInfo = GetSubscriptionInfo(product);
            if (subscriptionInfo.isSubscribed() == Result.True && subscriptionInfo.isFreeTrial() == Result.True)
            {
                return subscriptionInfo.getFreeTrialPeriodString();
            }

            return "";
        } catch (Exception ex)
        {
            Debug.LogWarning("Error getting GetIsFreeTrialPeriode: " + ex);
            return "";
        }
    }

    public SubscriptionInfo GetSubscriptionInfo()
    {
        string subscriptionType = _isYearly ? PurchaseIDs.subscriptionYearly : PurchaseIDs.subscriptionMonthly;
        var product = _inAppPurchacesManager.storeController.products.WithID(subscriptionType);
        if (product == null || !product.hasReceipt)
        {
            return null;
        }

        return GetSubscriptionInfo(product);
    }

    public string GetFullSubscriptionInfo()
    {
        var subscriptionType = _isYearly ? PurchaseIDs.subscriptionYearly : PurchaseIDs.subscriptionMonthly;
        var remainingTime = GetFormattedRemainingTime();
        var expireTime = GetSubscriptionExpirationDate(subscriptionType);
        var price = _inAppPurchacesManager.GetPrice(subscriptionType);
        var freeTrial = GetIsFreeTrial(subscriptionType) ? $"Free Trial({GetIsFreeTrialPeriod(subscriptionType)})" : "";

        string result = Regex.Replace(subscriptionType, @"\b(?!monthly\b|yearly\b)\w+\b", "").Trim();

        return $"{result} | {freeTrial} {remainingTime} | {expireTime} | {price}";
    }

    public string GetFormattedRemainingTime()
    {
        TimeSpan? remainingTime = GetRemainingSubscriptionTime();
        if (remainingTime == null) { return ""; }

        return $"{remainingTime.Value.Days}d {remainingTime.Value.Hours}h {remainingTime.Value.Minutes}m {remainingTime.Value.Seconds}s";
    }

    public DateTime? GetSubscriptionExpirationDate()
    {
        string subscriptionType = _isYearly ? PurchaseIDs.subscriptionYearly : PurchaseIDs.subscriptionMonthly;
        DateTime? remainingTime = GetSubscriptionExpirationDate(subscriptionType);

        return remainingTime;
    }

    public TimeSpan? GetRemainingSubscriptionTime()
    {
        string subscriptionType = _isYearly ? PurchaseIDs.subscriptionYearly : PurchaseIDs.subscriptionMonthly;
        TimeSpan? remainingTime = GetRemainingSubscriptionTime(subscriptionType);

        return remainingTime;
    }

    public DateTime? GetSubscriptionExpirationDate(string subscriptionProductId)
    {
        try
        {
            var product = _inAppPurchacesManager.storeController.products.WithID(subscriptionProductId);
            if (product == null || !product.hasReceipt)
            {
                return null;
            }

            var subscriptionInfo = GetSubscriptionInfo(product);
            if (subscriptionInfo.isSubscribed() == Result.True)
            {
                return subscriptionInfo.getExpireDate().ToLocalTime();
            }

            return null;
        } catch (Exception ex)
        {
            Debug.LogWarning("Error getting subscription expiration date: " + ex);
            return null;
        }
    }

    public TimeSpan? GetRemainingSubscriptionTime(string subscriptionProductId)
    {
        try
        {
            var product = _inAppPurchacesManager.storeController.products.WithID(subscriptionProductId);
            if (product == null || !product.hasReceipt)
            {
                return null;
            }

            var subscriptionInfo = GetSubscriptionInfo(product);
            if (subscriptionInfo.isSubscribed() == Result.True)
            {
                return subscriptionInfo.getRemainingTime();
            }

            return null;
        } catch (Exception ex)
        {
            Debug.LogWarning("Error getting subscription expiration date: " + ex);
            return null;
        }
    }

    private SubscriptionInfo GetSubscriptionInfo(Product subscriptionProduct)
    {
        if (subscriptionProduct.definition.type != ProductType.Subscription)
        {
            return null;
        }

        string receipt = subscriptionProduct.receipt;
        var receiptWrapper = (Dictionary<string, object>)MiniJson.JsonDecode(receipt);
        var store = receiptWrapper["Store"] as string;

        if (string.IsNullOrEmpty(store))
        {
            return null;
        }

        var subscriptionManager = new SubscriptionManager(subscriptionProduct, null);
        return subscriptionManager.getSubscriptionInfo();
    }
}