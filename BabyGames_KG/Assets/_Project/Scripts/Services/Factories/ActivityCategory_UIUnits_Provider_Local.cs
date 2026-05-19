using DataClasses;
using Identifiers.UI;
using Interfaces.Providers;
using SO;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Services.Factories
{
    public class ActivityCategory_UIUnits_Provider_Local : MonoBehaviour, IActivityCategory_UIUnits_Provider
    {
        [SerializeField] private List<KeyedObject<string, ActivityCategory_UIUnit>> _activityCategory_Prefab;

        [Inject] private IActivities_SO_Provider _activities_SO;

        private void Awake()
        {
            DiService.Inject(this);
        }

        public async Task<List<ActivityCategory_UIUnit>> GetActivityCategory_UIUnits(Transform parent)
        {
            foreach (var item in parent.GetComponentsInChildren<ActivityCategory_UIUnit>(true))
            {
                Destroy(item.gameObject);
            }

            List<ActivityCategory_UIUnit> activityCategory_UIUnits = new List<ActivityCategory_UIUnit>();
            Activities_SO activities_SO = await _activities_SO.GetActivities_SO();

            foreach (var activityCategory in activities_SO.activityCategories)
            {
                activityCategory_UIUnits.Add(await GetActivityCategory_UIUnit(activityCategory, parent));
            }

            return activityCategory_UIUnits;
        }

        public async Task<ActivityCategory_UIUnit> GetActivityCategory_UIUnit(ActivityCategory_SO activityCategory, Transform parent)
        {
            var item = _activityCategory_Prefab.Find(x => x.key == activityCategory.activityCategoryName);
            ActivityCategory_UIUnit activityCategory_UIUnit = Instantiate(item.value, parent);
            activityCategory_UIUnit.Initialize(activityCategory);

            await Task.CompletedTask;

            return activityCategory_UIUnit;
        }
    }
}