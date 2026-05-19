using DataClasses;
using Identifiers;
using Sirenix.OdinInspector;
using Spine.Unity;

namespace ColorfulTrain
{
    public class ColorfulTrainActivity_Identifier : _ActivityBase_Identifier
    {
        public SkeletonAnimation background;

        public override void UpdateData(Activity activity)
        {
            base.UpdateData(activity);

            type = DataClasses.ActivityType.ColorfulTrain;
        }

        [Button]
        private void Validate()
        {
            foreach (var item in background.GetComponentsInChildren<SkeletonPartsRenderer>())
            {
                item.MeshRenderer.sortingLayerName = item.name.Contains("sort") ? "Default" : "Spine";
            }
        }
    }
}