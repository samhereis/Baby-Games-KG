using SO;
using System;
using System.Threading.Tasks;

namespace Interfaces.Providers
{
    public interface IActivities_SO_Provider
    {
        public bool IsCashed();
        public Task<Activities_SO> GetActivities_SO(Action<float> onDownloadingUpdate = null);
        public Task<Activities_SO> Download(Action<float> onDownloadingUpdate = null);
    }
}