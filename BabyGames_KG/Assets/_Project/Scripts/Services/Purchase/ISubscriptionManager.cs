using System;
using System.Threading.Tasks;

public interface ISubscriptionManager
{
    public void Subscribe(string subscriptionID);
    public  string GetFormattedRemainingTime();
    public DateTime? GetSubscriptionExpirationDate();
    public TimeSpan? GetRemainingSubscriptionTime();
}