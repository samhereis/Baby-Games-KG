using DataClasses;
using Modes.Coloring;

namespace Identifiers
{
    public class ColoringActivity_Identifier : _ActivityBase_Identifier
    {
        public float minRamRequired = 1500f;

        public override void UpdateData(Activity activity)
        {
            base.UpdateData(activity);

            type = DataClasses.ActivityType.Coloring;
            Get<Spine_Identifier>().UpdateData();
        }
    }
}