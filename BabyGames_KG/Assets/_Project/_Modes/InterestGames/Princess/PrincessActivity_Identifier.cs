using DataClasses;
using Spine.Unity;
using UnityEngine;

namespace Identifiers
{
    public class PrincessActivity_Identifier : _ActivityBase_Identifier
    {
        [SerializeField] private SkeletonAnimation _princess;

        public override void UpdateData(Activity activity)
        {
            base.UpdateData(activity);

            type = DataClasses.ActivityType.InterestGame_Girl;
        }
    }
}