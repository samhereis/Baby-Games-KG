using Services;
using System;
using System.Threading.Tasks;
using UnityEngine.Purchasing;

namespace Interfaces
{
    public interface IPurchasesService
    {
        public event Action onInitialize;
        public event Action onInitializeFailed;
        public event Action<Item, Product> onPurchase;
        public event Action<Item, Product> onPurchaseFailed;
        public event Action onRestore;
        public event Action onRestoreFailed;

        public Task<bool> Initialize();

        public bool IsPurchased(string productID);
        public void Purchase(string productID);
        public string GetPrice(string productID);
        public void Restore();
    }
}