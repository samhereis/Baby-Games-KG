using CustomAttributes;
using Helpers;
using Interfaces;
using Interfaces.Providers;
using Loggers;
using Services;
using Sirenix.OdinInspector;
using SO;
using System;
using System.Collections.Generic;
using System.Linq;
using _Project.Scripts.Services.Purchase;
using TMPro;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.UI.Purchase
{
    public class PurchaseMenu_Data : MonoBehaviour
    {
        [field: SerializeField, Fg_De] public string priceYearly { get; set; } = "74,99 USD";
        [field: SerializeField, Fg_De] public string priceMonthly { get; set; } = "4,99 USD";
        [field: SerializeField, Fg_De] public string yearlySave { get; set; } = "62%";
        [field: SerializeField, Fg_De] public float yearlyPriceRaw { get; set; } = 5;
        [field: SerializeField, Fg_De] public float monthlyPriceRaw { get; set; } = 100;
        [field: SerializeField, Fg_De] public string expDate { get; set; } = "06.02.2026";
        [field: SerializeField, Fg_De] public DateTime? expDate_Time { get; set; }

        [field: SerializeField, Fg_De] public Activities_SO _spinesSO;

        [field: SerializeField, Fg_De] public List<string> _temp = new();

        [Inject] private IActivities_SO_Provider _spinesSoProvider;
        [Inject] private ISubscriptionManager _iSubscriptionManager;

        public async void Init(ISubscriptionChecker subscriptionChecker)
        {

            try
            {
                DiService.Inject(this);

                priceYearly = subscriptionChecker.GetYearlyPurchasePrice();
                priceMonthly = subscriptionChecker.GetMonthlyPurchasePrice();

                yearlyPriceRaw = float.Parse(new string(priceYearly.Where(x => char.IsDigit(x) || x == '.').ToArray()));
                monthlyPriceRaw = float.Parse(new string(priceMonthly.Where(x => char.IsDigit(x) || x == '.').ToArray()));

                CustomLogger.instance?.Log("In App Purchases", $"Prices: {yearlyPriceRaw} {monthlyPriceRaw}", this, LogTypes.General);

                _temp.Add(yearlyPriceRaw.ToString());
                _temp.Add(monthlyPriceRaw.ToString());

                CalculateYearlySave();

                expDate_Time = _iSubscriptionManager.GetSubscriptionExpirationDate();
                expDate = expDate_Time?.ToShortDateString();
            }
            catch (Exception ex)
            {
                CustomLogger.instance?.LogException(ex);

                _spinesSO = await _spinesSoProvider.GetActivities_SO();
                var yearlySaveObject = _spinesSO.floatSettings.Find(x => x.key == "purchase_YearlySave");
                yearlySave = (yearlySaveObject != null ? yearlySaveObject.value : 52).ToString();
            }
        }

        private void Update()
        {
            if (string.IsNullOrEmpty(priceYearly)) { priceYearly = "74,99 USD"; }
            if (string.IsNullOrEmpty(priceMonthly)) { priceMonthly = "4,99 USD"; }
            if (string.IsNullOrEmpty(yearlySave)) { yearlySave = "62%"; }
        }

        [Button]
        private void CalculateYearlySave()
        {
            float monthlyTotal = monthlyPriceRaw * 12f;
            if (monthlyTotal <= 0f) { yearlySave = "0%"; return; }

            // How much cheaper the yearly plan is vs paying monthly for 12 months.
            float priceRatio = NumberHelper.GetPercentageOf100(yearlyPriceRaw, monthlyTotal); // yearly as % of monthly total
            float percentSave = Mathf.Max(0f, 100f - priceRatio);

            yearlySave = $"{Mathf.RoundToInt(percentSave)}%";
        }
    }
}