using DataClasses;
using Identifiers;
using System;
using System.Threading.Tasks;

namespace Interfaces.Providers
{
    public interface IActivity_Identifier_Provider
    {
        public Task<bool> HasUpdate(Activity activity);
        public bool IsCashed(Activity activity);
        public Task<_ActivityBase_Identifier> InstantiaseActivity(Activity activity);
        public Task<_ActivityBase_Identifier> GetActivity(Activity activity, Action<float> onDownloadingUpdate = null);
        public Task<_ActivityBase_Identifier> Download(Activity activity, Action<float> onDownloadingUpdate = null);
    }
}