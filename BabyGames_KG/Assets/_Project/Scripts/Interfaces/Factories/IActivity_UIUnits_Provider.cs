using DataClasses;
using Identifiers.UI;
using SO;
using System.Threading.Tasks;
using UnityEngine;

namespace Interfaces.Providers
{
    public interface IActivity_UIUnits_Provider
    {
        public Task<Activity_UIUnit_Base> GetActivity_UIUnit(Activity activity, Transform parent);
    }
}