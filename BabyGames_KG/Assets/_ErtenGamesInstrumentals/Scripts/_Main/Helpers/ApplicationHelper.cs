using UnityEngine;

namespace Helpers
{
    public static class ApplicationHelper
    {
        public static bool HasInternetConnection()
        {
            return Application.internetReachability != NetworkReachability.NotReachable;
        }
    }
}