using CarTuning;
using DataClasses;
using Sounds;
using System.Collections.Generic;

namespace Identifiers
{
    public class CarTuning_Identifier : _ActivityBase_Identifier
    {
        public List<CarTuning_Car_Identifier> cars = new();

        public Sound setSound;
        public Sound finishSound;
        public Sound finishSound_2;

        public override void UpdateData(Activity activity)
        {
            base.UpdateData(activity);

            type = DataClasses.ActivityType.CarTuning;
        }
    }
}