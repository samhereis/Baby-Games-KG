using DataClasses;
using DataClasses.AssetReferences;
using Identifiers.UI;
using Interfaces.Providers;
using System.Threading.Tasks;
using UnityEngine;

namespace Services.Factories
{
    public class Activity_UIUnits_Provider_Local : MonoBehaviour, IActivity_UIUnits_Provider
    {
        [SerializeField] private ExternalAssetReference_HasComponent<Activity_UIUnit_Standart> _activityUIUnit_Standart;
        [SerializeField] private ExternalAssetReference_HasComponent<Activity_UIUnit_Video> _activityUIUnit_Video;
        [SerializeField] private ExternalAssetReference_HasComponent<Activity_UIUnit_Standart> _activityUIUnit_Coloring;

        public async Task<Activity_UIUnit_Base> GetActivity_UIUnit(Activity activity, Transform parent)
        {
            Activity_UIUnit_Base activity_UIUnit = null;

            if (activity.type == ActivityType.Video)
            {
                activity_UIUnit = await _activityUIUnit_Video.InstantiateAsync(parent);
            }
            else if (activity.type == ActivityType.Coloring)
            {
                activity_UIUnit = await _activityUIUnit_Coloring.InstantiateAsync(parent);
            }
            else
            {
                activity_UIUnit = await _activityUIUnit_Standart.InstantiateAsync(parent);
            }

            activity_UIUnit.activityData = activity;
            activity_UIUnit.gameObject.name = activity.GetName();

            return activity_UIUnit;
        }
    }
}