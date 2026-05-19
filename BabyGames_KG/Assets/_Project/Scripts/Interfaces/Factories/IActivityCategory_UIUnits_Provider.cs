using DataClasses;
using Identifiers.UI;
using SO;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Interfaces.Providers
{
    public interface IActivityCategory_UIUnits_Provider
    {
        public Task<List<ActivityCategory_UIUnit>> GetActivityCategory_UIUnits(Transform parent);
        public Task<ActivityCategory_UIUnit> GetActivityCategory_UIUnit(ActivityCategory_SO activityCategory, Transform parent);
    }
}