using DataClasses;

namespace Identifiers
{
    public class WhoLivesWhereActivity_Identifier : _ActivityBase_Identifier
    {
        public override void UpdateData(Activity activity)
        {
            base.UpdateData(activity);

            type = ActivityType.WhoLivesWhere;
        }
    }
}