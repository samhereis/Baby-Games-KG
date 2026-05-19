using DataClasses;
using Identifiers;

namespace CoockingSalade
{
    public class CoockingActivity_Identifier : _ActivityBase_Identifier
    {
        public override void UpdateData(Activity activity)
        {
            base.UpdateData(activity);

            type = DataClasses.ActivityType.Cooking;
        }
    }
}