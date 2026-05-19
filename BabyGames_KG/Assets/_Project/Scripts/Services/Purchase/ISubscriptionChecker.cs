namespace _Project.Scripts.Services.Purchase
{
    public interface ISubscriptionChecker
    {
        public bool IsSubscribed();
        public string GetYearlyPurchasePrice();
        public string GetMonthlyPurchasePrice();
    }
}